using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory("QuestManager")]
  [Tooltip("Activate a quest")]
  public class ActivateQuest : FsmStateAction
  {
    [RequiredField]
    [Tooltip("Name of the Quest GameObject")]
    public FsmString questName;
    
    [Tooltip("Whether to start the quest, otherwise it will simply become available.")]
    public FsmBool startQuest;

    
    public override void Reset()
    {
      startQuest = false;
      questName = null;
    }

    public override void OnEnter()
    {
      Quest q = QuestManager.Instance.GetQuest (questName.Value);
      q.gameObject.SetActive (true);
      if (startQuest.Value)
      {
        q.StartQuest ();
      }

      Finish ();
    }
  }
}