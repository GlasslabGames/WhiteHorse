using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory("QuestManager")]
  [Tooltip("Change the current chapter")]
  public class ChangeChapter : FsmStateAction
  {
    [RequiredField]
    [Tooltip("Name of the Chapter GameObject")]
    public FsmString chapterName;
    
    public override void Reset()
    {
      chapterName = null;
    }

    public override void OnEnter()
    {
      QuestManager.Instance.SetActiveChapter (QuestManager.Instance.GetChapter(chapterName.Value));

      Finish ();
    }
  }
}