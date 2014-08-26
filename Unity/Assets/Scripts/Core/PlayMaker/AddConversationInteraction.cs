using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory("QuestManager")]
  [Tooltip("Add a conversation to an InteractiveObject")]
  public class AddConversationInteraction : FsmStateAction
   {
    [RequiredField]
    [Tooltip("The InteractiveObject's GameObject")]
    public FsmGameObject targetObject;

    [RequiredField]
    [Tooltip("Name of the conversation to add.")]
    public FsmString conversationName;
    
    public override void Reset()
    {
      targetObject = null;
      conversationName = null;
    }
    
    public override void OnEnter()
    {
      GameObject target = targetObject.Value;

      Talk talkInteraction = target.AddComponent<Talk>();

      talkInteraction.ConversationName = conversationName.Value;

      Finish ();
    }
  }
}  