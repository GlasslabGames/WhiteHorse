using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory("QuestManager")]
  [Tooltip("Remove a conversation to an InteractiveObject")]
  public class RemoveConversationInteraction : FsmStateAction
  {
    [RequiredField]
    [Tooltip("The InteractiveObject's GameObject")]
    public FsmGameObject targetObject;
    
    [RequiredField]
    [Tooltip("Name of the conversation to remove.")]
    public FsmString conversationName;
    
    public override void Reset()
    {
      targetObject = null;
      conversationName = null;
    }
    
    public override void OnEnter()
    {
      GameObject target = targetObject.Value;
      
      Talk talkInteraction = target.GetComponent<Talk>();
      
      if (talkInteraction != null && talkInteraction.ConversationName == conversationName.Value)
      {
        Component.Destroy(talkInteraction);
      }
      else
      {
        Debug.LogWarning("[RemoveConversationInteraction] Could not find target Talk interaction with conversation '"+conversationName.Value+"' on target object ("+target.name+")");
      }
      
      Finish ();
    }
  }
}  