using UnityEngine;

public class GLDragDropContainer : UIDragDropContainer
{
  public bool IsOver { get { return m_overTarget != null; } } // Whether an object is currently dragged over. Used to exit over if the object is dropped on the container. 
  
  public event OnDragEventHandler ItemDropped;
  public event OnDragEventHandler ItemTaken; // Not implemented!
  public event OnDragEventHandler ItemDragEnter;
  public event OnDragEventHandler ItemDragExit;

  public bool Magnetic = false; // whenever the obj is anywhere over this container, it snaps to the center
  public bool SnapIntoPlace = false; // only when the obj is released over this container, then it snaps to the center
  public bool ConsumeEvents = false;
  //public bool ShowTargetOnDrag = false; // Whether to show a target when a drag event is started

  private GLDragDropItem m_overTarget; // Current object dragged over this container

  void Start() {}

  public Transform GetReparentTarget()
  {
    return reparentTarget != null ? reparentTarget : transform;
  }

  internal bool OnItemDrop(GLDragDropItem obj)
  {
    if (!enabled) return false;

    // If the object is still dragged over this container, fire an exit event
    if (IsOver)
      OnDragExit (obj);
    
    return attemptConsume (ItemDropped, obj);
  }

  internal bool OnItemTaken(GLDragDropItem obj)
  {
    if (!enabled) return false;
    return attemptConsume (ItemTaken, obj);
  }
  
  internal bool OnDragEnter(GLDragDropItem obj)
  {
    if (!enabled) return false;
    m_overTarget = obj;
    return attemptConsume (ItemDragEnter, obj);
  }
  
  internal bool OnDragExit(GLDragDropItem obj)
  {
    if (!enabled) return false;
    m_overTarget = null;
    return attemptConsume (ItemDragExit, obj);
  }

  private bool attemptConsume(OnDragEventHandler handler, GLDragDropItem obj)
  {
    if (handler != null) {
      GLDragEventArgs eventArgs = new GLDragEventArgs(obj, this);
      handler(eventArgs);

      return ConsumeEvents || eventArgs.isConsumed;
    } else
      return ConsumeEvents;
  }
}
