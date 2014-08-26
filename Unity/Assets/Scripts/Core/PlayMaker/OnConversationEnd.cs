
// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

#if !UNITY_FLASH

using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory("Dialogue System")]
  [Tooltip("Send a global event on conversation ended. PER FRAME")]
  public class OnConversationEnd : FsmStateAction
  {
    [Tooltip("Where to send the event.")]
    public FsmEventTarget eventTarget;

    [RequiredField]
    [Tooltip("The event to send. NOTE: Events must be marked Global to send between FSMs.")]
    public FsmEvent sendEvent = FsmEvent.Finished;

    private bool m_conversationWasActive;
    
    public override void Reset()
    {
      eventTarget = null;
      sendEvent = null;
    }

    public override void Awake()
    {
      m_conversationWasActive = DialogueManager.IsConversationActive;
    }
    
    public override void OnEnter()
    {
      if (!DialogueManager.IsConversationActive && m_conversationWasActive)
       {
        onFinish ();
      }

      m_conversationWasActive = DialogueManager.IsConversationActive;
    }

    private void onFinish()
    {
      Fsm.Event (sendEvent);

      Finish ();
    }
  }
}

#endif