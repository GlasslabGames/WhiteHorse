
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory(ActionCategory.GameObject)]
  [Tooltip("Waits for object to deactivate, sends a message when it happens. PER FRAME")]
  public class OnObjectDeactivated : FsmStateAction
  {
    [RequiredField]
    [Tooltip("The GameObject to activate/deactivate.")]
    public FsmOwnerDefault gameObject;
    
    [Tooltip("Where to send the event.")]
    public FsmEventTarget eventTarget;

    [RequiredField]
    [Tooltip("The event to send. NOTE: Events must be marked Global to send between FSMs.")]
    public FsmEvent sendEvent = FsmEvent.Finished;
    
    // store the game object that we activated on enter
    // so we can de-activate it on exit.
    private GameObject m_gameObject;

    private bool m_wasObjectActivated;
    
    public override void Reset()
    {
      gameObject = null;
      eventTarget = null;
      sendEvent = null;
    }
    
    public override void OnEnter()
    {
      m_gameObject = Fsm.GetOwnerDefaultTarget(gameObject);

      m_wasObjectActivated = m_gameObject.activeInHierarchy;
    }
    
    public override void OnUpdate()
    {
      if (m_wasObjectActivated && !m_gameObject.activeInHierarchy)
      {
        onFinish ();
      }

      m_wasObjectActivated = m_gameObject.activeInHierarchy;
    }
    
    private void onFinish()
    {
      Fsm.Event (sendEvent);
      
      Finish ();
    }
  }
}