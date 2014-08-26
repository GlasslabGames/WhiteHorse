using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;

public class InteractiveObject : OnClickHandler {
  public string DefaultName;
  public string DefaultDescription;
  public bool AttachDragDropContainer;

  private List<Interaction> m_interactions;

  private GLDragDropContainer m_uiContainer;

  public static Time LastInteractionTime;

  private GLAfterEffectsAnimationController m_animationController;

  private bool refreshingDropRing = false; // Used to track whether drop ring was refreshing before component was disabled

  public InteractiveObject()
  {
    DefaultName = "";
    DefaultDescription = "";
    AttachDragDropContainer = false;
  }
  
  void Awake() {
    m_interactions = new List<Interaction>( GetComponentsInChildren<Interaction>() ); // gets components attached to this object and to its children

    if (AttachDragDropContainer) {
      m_uiContainer = GLDragDropContainerLayer.Instance.AddContainer(collider);

      refreshDropRing();

      SignalManager.QuestChanged += onQuestChanged;
      SignalManager.ObjectiveChanged += onObjectiveChanged;
    }
  }

  private IEnumerator refreshDropRingNextFrame()
  {
    refreshingDropRing = true;
    yield return null;

    refreshDropRing();
  }

  private void refreshDropRing()
  {
    if (hasValidDragDropInteraction())
    {
      if (m_animationController == null)
      {
        m_animationController = PoolManager.InstanceOrCreate.GetObject("DropRingGLAEA").GetComponent<GLAfterEffectsAnimationController>();
        m_animationController.transform.parent = m_uiContainer.transform;
        m_animationController.transform.localPosition = new Vector3(115.0f, 0); // HACK offset to compensate non-centered animation
        m_animationController.transform.localScale = Vector3.one;
      }
    }
    else if (m_animationController != null)
    {
      PoolManager.InstanceOrCreate.CacheObject(m_animationController.gameObject, "DropRingGLAEA");
      m_animationController = null;
    }

    refreshingDropRing = false;
  }

  private void onQuestChanged(Quest q)
  {
    if (gameObject.activeInHierarchy)
    {
      StartCoroutine(refreshDropRingNextFrame());
    }
    else
    {
      refreshDropRingNextFrame();
    }
  }

  private void onObjectiveChanged(Objective o)
  {
    if (gameObject.activeInHierarchy)
    {
      StartCoroutine(refreshDropRingNextFrame());
    }
    else
    {
      refreshDropRingNextFrame();
    }
  }
  
  private void DropRingAnimateIn()
  {
    if (m_animationController != null && (!m_animationController.IsPlaying ||
                                          ((m_animationController.GetCurrentAnimationName() != "dropRing_animateIn" || m_animationController.IsPlayingBackwards) && 
                                          m_animationController.GetCurrentAnimationName() != "dropRing_pulseLoop"))
        )
    {
      m_animationController.PlayAnimation("dropRing_animateIn");
      // TODO ew a delegate
      m_animationController.AnimationFinishedOnceOnly += delegate(GLAfterEffectsAnimationController glaea) {
        if (m_animationController != null)
        {
          m_animationController.PlayAnimation("dropRing_pulseLoop", true, true);
        }
        else
        {
          Debug.LogError ("m_animationController was set to null on callback?", this);
        }
      };
    }
  }
  
  private void DropRingAnimateOut()
  {
    if (m_animationController != null &&
        (!m_animationController.IsPlaying || (m_animationController.GetCurrentAnimationName() != "dropRing_animateIn" || !m_animationController.IsPlayingBackwards )))
    {
      m_animationController.PlayAnimation("dropRing_animateIn", true, false, true);
    }
  }

  private bool hasValidDragDropInteraction()
  {
    // See if this object is used for any actions. If so, do it.
    for (int i=m_interactions.Count-1; i>=0; i--) {
      Interaction action = m_interactions[i];
      if (action.UsesDragDropInteractions && (action.Properties.IsSatisfied()))
      {
        return true;
      }
    }

    return false;
  }

  private void OnDragExit(GLDragEventArgs args)
  {
    if (m_animationController != null)
    {
      m_animationController.PlayAnimation("dropRing_hoverOn", true, false, true);
      // TODO ew a delegate
      m_animationController.AnimationFinishedOnceOnly += delegate(GLAfterEffectsAnimationController glaea) {
        if (m_animationController != null)
        {
          m_animationController.PlayAnimation("dropRing_pulseLoop", true, true);
        }
        else
        {
          Debug.LogError ("m_animationController was set to null on callback?", this);
        }
      };
    }
  }

  private void OnDragEnter(GLDragEventArgs args)
  {
    if (m_animationController != null)
    {
      m_animationController.PlayAnimation("dropRing_hoverOn");
    }
  }

  public void Refresh() {
    // re-find interactions in children (call this if we append new interactions)
    m_interactions = new List<Interaction>( GetComponentsInChildren<Interaction>() );
  }

  private void OnItemDragStarted(GLDragDropItem item)
  {
    DropRingAnimateIn();
  }

  private void OnItemDragStopped(GLDragDropItem item)
  {
    DropRingAnimateOut();
  }

  void OnEnable() {
    if (m_uiContainer != null) {
      SignalManager.EquipmentTrayOpened += DropRingAnimateIn;
      SignalManager.EquipmentTrayClosed += DropRingAnimateOut;
      SignalManager.ItemDragStarted += OnItemDragStarted;
      SignalManager.ItemDragStopped += OnItemDragStopped;

      m_uiContainer.ItemDropped += OnItemDropped;
      m_uiContainer.ItemDragExit += OnDragExit;
      m_uiContainer.ItemDragEnter += OnDragEnter;
    }
  }

  void OnDisable() {
    if (refreshingDropRing)
    {
      refreshDropRing();
    }

    if (m_uiContainer != null) {
      SignalManager.EquipmentTrayOpened -= DropRingAnimateIn;
      SignalManager.EquipmentTrayClosed -= DropRingAnimateOut;
      SignalManager.ItemDragStarted -= OnItemDragStarted;
      SignalManager.ItemDragStopped -= OnItemDragStopped;
      
      m_uiContainer.ItemDropped -= OnItemDropped;
      m_uiContainer.ItemDragExit -= OnDragExit;
      m_uiContainer.ItemDragEnter -= OnDragEnter;
    }
  }

  void OnDestroy()
  {
    if (AttachDragDropContainer && GLDragDropContainerLayer.Instance) { // Layer manager might get destroyed before this does when game is shutting down
      GLDragDropContainer container = GLDragDropContainerLayer.Instance.RemoveContainer(collider);
    }
    
    SignalManager.QuestChanged -= onQuestChanged;
    SignalManager.ObjectiveChanged -= onObjectiveChanged;
  }
  
  public void OnItemDropped(GLDragEventArgs args) {
    Debug.Log ("OnItemDropped: "+args.DragObject.name +" on "+name);

    if (m_animationController != null)
    {
      m_animationController.PlayAnimation("dropRing_receiveObject");
    }

    // See if this object is used for any actions. If so, do it.
    for (int i=m_interactions.Count-1; i>=0; i--) {
      Interaction action = m_interactions[i];
      if ((action.Properties.IsSatisfied()) && action.CanUse(args.DragObject))
      {
        // THIS DEPENDS ON ACTION CONSUMING THE EVENT
        action.Do (args);
        break;
     }
    }
  }

	protected override void OnMouseClick() {
    if (DialogueManager.IsConversationActive) {
      // don't count clicks while we're in a conversation or the game is paused
      return;
    }

		// Write Telemetry Data
		PegasusManager.Instance.GLSDK.AddTelemEventValue( "name", name );
    PegasusManager.Instance.AppendDefaultTelemetryInfo();
		PegasusManager.Instance.GLSDK.SaveTelemEvent( "Examine_object" );
    
    List<Interaction> actions = new List<Interaction>();
    Refresh();

    // check which actions we can take
    foreach (Interaction action in m_interactions) {
      if (!action.Properties.IsSatisfied() || !action.IsPossible()) {
        continue; // we can't do that right now
      } else if (action.Properties.ActivatedBy > 0) {
                 // && m_manager.GetItem(action.Properties.ActivatedBy) == null) { // nevermind, don't auto use items in inventory
        continue; // we didn't have the item we needed
      } else {
        actions.Add (action);
      }
    }

    if (actions.Count > 0) {
      Interaction highestPriority = actions.Aggregate((i1,i2) =>
        (int) i1.Properties.Priority > (int) i2.Properties.Priority ? i1 : i2);
      highestPriority.Do ();
    }
  }
}
