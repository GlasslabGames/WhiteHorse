using UnityEngine;
using System.Linq;
using System;

[RequireComponent(typeof(ParentChangedListener))]
public class GLDragDropItem : UIDragDropItem
{
  public event OnDragEventHandler OnDragStarted;
  public event OnDragEventHandler OnDragEnded;
  public event OnDragEventHandler OnDropped; // after all the processing of a drop
  public delegate void GLDragDropItemDel( GLDragDropItem item );
  public event GLDragDropItemDel OnDragged; // constantly when the item is being dragged
  private event Action OnSnap; // internal event which is called when it snaps onto the target container (if that container has Snap Into Place active)
  
  private GLDragDropContainer m_container; // Container that this object is over at all times
  public GLDragDropContainer OverContainer {
    get { return m_container; }
  }
  private GLDragDropContainer m_containerBeforeDrag; // Container that this object was over before drag started
  
  private static GameObject PREV_HOVERED_OBJECT; // made static for optimization
  private static GLDragDropContainer PREV_HOVERED_CONTAINER;

  // if this is true, the restriction (e.g. Vertical) is set to none when we're not under a scrollview
  public bool ApplyRestrictionForScrollViewOnly;
  private Restriction m_originalRestriction;
  private UIScrollView m_originalScrollView;

  // if this is true, then after temporarily being stored in some container and then removed,
  // the object will be reparented by to its original parent
  public bool ReturnToOriginalParent;
  private Transform m_originalParent;

  // When this is true, we want to disable the collider on other GLDragDrop Items while dragging this one
  // This is useful so that previously moved DragDropItems don't block us attaching to the DragDropContainer
  public bool DisableOthersOnDrag = true;
  private Collider[] m_otherDragColliders;
  
  private static Vector3 m_mouseOffset; // remember the mouse offset
	private Vector3 m_targetPosition = new Vector3(float.NaN, 0); // X is NaN'd when the position is not valid.
  private bool m_magnetized; // true if we're on a magnetic container that we can attach to
  private bool m_returning; // true if we're flying back to our starting position.

  public float m_snapIntoPlaceSpeed;
  public float m_returnSpeed;
  public bool CenterOnMouse;

  public bool m_constrainWithinPanel;
  
  // Static reference to whatever object is currently being dragged
  public static GLDragDropItem CurrentlyDragging;

  void Awake() {
    if (ReturnToOriginalParent) m_originalParent = transform.parent;

    if (ApplyRestrictionForScrollViewOnly) {
      m_originalRestriction = restriction;
      m_originalScrollView = Utility.FirstAncestorOfType<UIScrollView>(transform);
	  }

    if (DisableOthersOnDrag) {
      m_otherDragColliders = transform.root.GetComponentsInChildren<GLDragDropItem>().
        Select<GLDragDropItem, Collider>(x => x.collider).ToArray();
    }
    
    ParentChangedListener parentListener = GetComponent<ParentChangedListener>();
    if (parentListener != null)
    {
      parentListener.OnParentChangedLastFrame += onParentChanged;
    }
    else
    {
      Debug.LogWarning("[GLDragDropItem("+name+")] Could not find ParentChangedListener component! Creating and attaching one...", this);
      gameObject.AddComponent<ParentChangedListener>().OnParentChangedLastFrame += onParentChanged;
    }
    refreshParentContainer();
	}

  void OnDestroy()
  {
    ParentChangedListener parentListener = GetComponent<ParentChangedListener>();
    if (parentListener != null)
    {
      parentListener.OnParentChangedLastFrame -= onParentChanged;
    }
  }

  private void onParentChanged(Transform oldParent, Transform newParent)
  {
    if (newParent != null &&
        newParent.GetComponent<UIDragDropRoot>() == null &&
        (m_container == null || newParent != m_container.GetReparentTarget()))
    {
      refreshParentContainer();
    }
  }

  private void refreshParentContainer()
  {
    m_container = findParentContainer();
  }

  private GLDragDropContainer findParentContainer()
  {
    GLDragDropContainer[] potentialParents = UnityEngine.Object.FindObjectsOfType<GLDragDropContainer>();
    for (int i=potentialParents.Length-1; i>=0; i--)
    {
      Transform containerTarget = potentialParents[i].GetReparentTarget();
      
      if (containerTarget == transform.parent)
      {
        return potentialParents[i];
      }
    }

    return null;
  }

  override protected void Update()
  {
    if (!float.IsNaN (m_targetPosition.x)) // if we have a target position
    {
      if (m_targetPosition == transform.position) // we're in position
      {
        //Debug.Log (this+" reached target position: "+m_targetPosition, this);
        m_targetPosition.x = float.NaN; // we no longer need this target position
        m_returning = false; // if we had been trying to fly home, we made it
        if (OnSnap != null) OnSnap();
        return;
      }

      float speed = m_snapIntoPlaceSpeed;
      if (m_returning) { // if we're trying to fly back home, use the returning speed instead
        speed = m_returnSpeed;
      }

      Vector3 delta = m_targetPosition - transform.position;

      if (speed > 0 && // if we want to translate gradually
          delta.sqrMagnitude / UICamera.currentCamera.transform.localScale.x > .0001f) // if we're not at our position yet
      {
        delta *= speed;
      }

      transform.Translate(delta); // translate to that position
    }
  }

  public void SetOriginalParent(Transform parent)
  {
    m_originalParent = parent;
  }

  protected override void OnDragDropStart ()
  {
    Debug.Log ("Start drag "+this, this);
    m_containerBeforeDrag = m_container;
    GLDragDropContainer container = m_container ? m_container.GetComponent<GLDragDropContainer>() : null;
    if (OnDragStart != null) {
      OnDragStart(new GLDragEventArgs(this, container));
    }

    collider.enabled = false;
    base.OnDragDropStart ();

    Vector3 touchPoint = InputManager.GetWorldTouchPoint (UICamera.currentCamera);

    if (CenterOnMouse) {
      Vector3 center = NGUIMath.CalculateAbsoluteWidgetBounds (transform).center;
      transform.Translate ( touchPoint - center, Space.World ); // move so that the mouse is in the middle
	  }

    m_mouseOffset = transform.position - touchPoint; // store the position relative to the mouse

    // Make sure there isn't a tween still active on the dragging object
    SpringPosition springPositionComponent = GetComponent<SpringPosition>();
    if (springPositionComponent != null && springPositionComponent.enabled)
    {
      springPositionComponent.enabled = false;
    }
    

    if (DisableOthersOnDrag) m_otherDragColliders.ToList().ForEach(delegate(Collider obj) {
      if (obj != null) obj.enabled = false;
    });

    CurrentlyDragging = this;

    // Since we just started dragging, we shouldn't have a target position yet
    m_targetPosition = new Vector3(float.NaN, 0);
    OnSnap = null; // Also, cancel the event that would happen when we finished snapping into place

    if (SignalManager.ItemDragStarted != null) SignalManager.ItemDragStarted(this);
  }

  protected override void OnDragDropRelease(UnityEngine.GameObject surface)
  {
    m_targetPosition.x = float.NaN; // we've released, so we don't have a target to magnetize to

    collider.enabled = true;

    PREV_HOVERED_OBJECT = null;
    PREV_HOVERED_CONTAINER = null;

    bool isObjectProcessed = attemptDrop (); // true if we've dropped onto a valid surface

    if (!isObjectProcessed)
    {
      // mParent is where we return to if we release w/o a valid surface
      // So if we want to return to our original parent instead of the last container we were on, here's the place to set it
      if (ReturnToOriginalParent && mParent != m_originalParent)
      {
        if (m_containerBeforeDrag != null)
        {
          m_containerBeforeDrag.OnItemTaken(this); // tell the container that we left it
        }

        base.mParent = m_originalParent;
      }

      m_container = null;
    }
    else if (mParent != null && mParent != m_container.GetReparentTarget() && m_containerBeforeDrag != null)
    {
      m_containerBeforeDrag.OnItemTaken(this); // tell the container that we left it
    }


    base.OnDragDropRelease (isObjectProcessed ? surface : null);

    if (DisableOthersOnDrag) m_otherDragColliders.ToList().ForEach(delegate(Collider obj) {
      if (obj != null) obj.enabled = true;
    });

    bool delayDrop = false; // true iff we're animating into place and want to wait to send drop event

    if (surface != null && isObjectProcessed) { // we found a place to drop onto
      GLDragDropContainer container = surface.GetComponent<GLDragDropContainer>();
      if (container != null && container.SnapIntoPlace) {
        // target the center of the container we want to snap to
        m_targetPosition = container.transform.position;
        m_magnetized = true;

        delayDrop = true; // dont' start the drop event right now, wait until we're in position
        OnSnap += delegate {
          if (OnDrop != null) {
            OnDrop(new GLDragEventArgs(this, isObjectProcessed && surface ? surface.GetComponent<GLDragDropContainer>() : null));
          }
        };
        // OnDrop will happen when we reach our target position. But if we start dragging again before that happens, it will be canceled.
      }
    }
    // The problem is that if we pick it up as we're animating local position, we end up finishing the animation while being dragged and are then offset?
    // Or we start with a big offset? 

    if (!delayDrop && OnDrop != null) {
      OnDrop(new GLDragEventArgs(this, isObjectProcessed && surface ? surface.GetComponent<GLDragDropContainer>() : null));
    }
  }

  protected override void OnDragDropMove(Vector2 delta)
  {
    GameObject over = UICamera.hoveredObject;
    if (over != PREV_HOVERED_OBJECT) { // if we're changing what we're over
      if (over != null)
      {
        m_container = over.GetComponent<GLDragDropContainer> ();
      }
      else m_container = null;
      
      if (m_container != PREV_HOVERED_CONTAINER)
      {
        m_magnetized = false; // only magnetize in one case...

        if (m_container != null)
        {
          //Debug.Log (this+" entered new container "+m_container);
          // OnDragEnter
          bool canEnter = m_container.OnDragEnter(this);

          if (canEnter && m_container.Magnetic)
          {
            m_magnetized = true; // ... when we enter a container that we can stick to
            m_targetPosition = m_container.transform.position; // then we should move to that position
          }
        }
        
        if (PREV_HOVERED_CONTAINER != null) // if we had a previous container we were over
        {
          // OnDragExit
          PREV_HOVERED_CONTAINER.OnDragExit(this);
          if (PREV_HOVERED_CONTAINER.Magnetic && (m_container == null || !m_magnetized)) // if our previous container was magnetic and this one is not
          {
            // Reset the target position to follow the touch

            m_targetPosition = InputManager.GetWorldTouchPoint (UICamera.currentCamera) - m_mouseOffset;
            delta = Vector3.zero; // Zero the delta so the dragged object doesn't get transformed any more after this point
          }
        }
        
        PREV_HOVERED_CONTAINER = m_container;
      }
    }
    
    PREV_HOVERED_OBJECT = over;

    if (!m_returning && (m_container == null || !m_magnetized)) // if we're not returning or trying to stick to a container, the target position is the player's finger
    {
      base.OnDragDropMove (delta);
      m_targetPosition = InputManager.GetWorldTouchPoint (UICamera.currentCamera) + m_mouseOffset; // plus the offset from when we started dragging
      //Debug.Log (this+" has new target position from mouse: "+m_targetPosition, this);
    }

/*
    // Note: This is a bit redundant with the movement in Update, but it seems to be necessary to avoid getting stuck on initial drag
    if (m_container == null || !m_magnetized) // if we're not magnetized, follow the mouse with OnDragDropMove
    {
      base.OnDragDropMove (delta);
    }
*/

    /*
    if (m_constrainWithinPanel) {
      UIPanel panel = Utility.FirstAncestorOfType<UIPanel>(transform);
      Bounds b = NGUIMath.CalculateRelativeWidgetBounds(panel.transform);
      //panel.ConstrainTargetToBounds(transform, ref b, true); // TODO
    }
    */

    if (OnDrag != null) OnDrag(this);
  }

  protected override void OnDragDropEnd()
  {
    base.OnDragDropEnd();

    if (OnDragEnd != null) {
      OnDragEnd(new GLDragEventArgs(this, m_container ? m_container.GetComponent<GLDragDropContainer>() : null));
    }

    if (ApplyRestrictionForScrollViewOnly) {
      // Check if we're currently under our original scroll view
      bool underScrollView = (Utility.FirstAncestorOfType<UIScrollView>(transform) == m_originalScrollView);
      // If so, reset restriction to the original.
      if (underScrollView) restriction = m_originalRestriction;
      else restriction = Restriction.None;
    }

    if (CurrentlyDragging == this) CurrentlyDragging = null;

    // reverse the setting done in UIDragDropItem so that we don't start scrolling the scrollview immediately
    if (mDragScrollView != null) {
      GLGrid grid = mGrid as GLGrid;
      if (grid != null) {
        mDragScrollView.enabled = false; // disable the drag component for now

        EventDelegate.Add (grid.onRepositionEventDelegate, delegate() {
          mDragScrollView.enabled = true; // but when we're done moving the grid into place, re-enable it
        }, true);
      }
    }

    // If we want to fly back, set the target position to return to our parent's position
    if (m_returnSpeed > 0) {
      m_returning = true;
      m_targetPosition = transform.parent.position;
    }

    if (SignalManager.ItemDragStopped != null) SignalManager.ItemDragStopped(this);
  }

  // Call this to pretend we just dropped the item, e.g. to make it fly back to its grid
  // Since UIDragDrop's functions are private, duplicate some of the functionality here
  public void SimulateDrop() {
    if (ReturnToOriginalParent) {
      mParent = m_originalParent;
      mGrid = NGUITools.FindInParents<UIGrid>(mParent);
      mTable = NGUITools.FindInParents<UITable>(mParent);
    }

    m_container = mParent.GetComponent<GLDragDropContainer>();

    mTrans.parent = mParent;
    NGUITools.MarkParentAsChanged(gameObject);

    OnDragDropEnd();
  }

  private bool attemptDrop()
  {
		return m_container != null && m_container.OnItemDrop (this);
  }

  ~GLDragDropItem()
  {
    OnDragEnd = null;
    OnDragStart = null;
  }
}

