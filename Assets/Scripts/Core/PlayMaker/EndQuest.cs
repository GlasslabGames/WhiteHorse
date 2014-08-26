using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory("QuestManager")]
  [Tooltip("End a quest (optionally marking it complete)")]
  public class EndQuest : FsmStateAction
  {
    [RequiredField]
    [Tooltip("The GameObject of the quest to end.")]
    public FsmOwnerDefault gameObject;
    
    public override void Reset()
    {
      gameObject = null;
    }
    
    public override void OnEnter()
    {
      GameObject target = Fsm.GetOwnerDefaultTarget (gameObject);
      Quest targetQuest = target.GetComponent<Quest> ();

      targetQuest.CompleteQuest ();
      
      Finish ();
    }
  }
}
