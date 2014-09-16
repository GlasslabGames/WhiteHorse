

using UnityEngine;
using GlassLab.Core.Serialization;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory("QuestManager")]
  public class SetBackupQuestInfo : FsmStateAction
  {
    public FsmString DisplayedQuest;
    public FsmString DisplayedObjective;
    
    public override void Reset()
    {
      DisplayedQuest = null;
      DisplayedObjective = null;
    }
    
    public override void OnEnter()
    {
      QuestManager.Instance.BackupQuestName = DisplayedQuest.Value;
      QuestManager.Instance.BackupObjective = DisplayedObjective.Value;
      QuestManager.Instance.IsNewBackupQuest = true;

      if (SignalManager.QuestChanged != null) SignalManager.QuestChanged(null);

      SessionManager.Instance.Save();

      Finish();
    }
  }
}