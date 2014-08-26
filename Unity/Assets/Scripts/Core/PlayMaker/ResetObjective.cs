using UnityEngine;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory("QuestManager")]
  [Tooltip("Reset Objective")]
  public class ResetObjective : FsmStateAction
  {

    [RequiredField]
    [Tooltip("Name of the Objective GameObject")]
    public FsmString objectiveName;

    [Tooltip("Name of the Quest GameObject. If empty the action will attempt to search the owner of this FSM.")]
    public FsmString questName;

    public override void Reset()
    {
      questName = null;
      objectiveName = null;
    }
    
    public override void OnEnter()
    {
      Quest q;
      if (questName != null && questName.Value != "")
      {
        q = QuestManager.Instance.GetQuest (questName.Value);
      } else
      {
        Quest[] ownerQuests = Fsm.Owner.GetComponentsInChildren<Quest>(true);
        if (ownerQuests.Length != 1)
        {
          Debug.LogError("[CompleteObjective(FSMAction)] Tried to find owner quest, but FSM has " + ownerQuests.Length + " quests!");
        }

        q = ownerQuests[0];
      }

      List<Objective> questObjectives = q.GetObjectives ();
      foreach (Objective o in questObjectives)
      {
        if (o.name == objectiveName.Value)
        {
          o.Reset();
          o.gameObject.SetActive(false);
          Finish ();
          return;
        }
      }

      Debug.LogError ("[CompleteObjective(FSMAction)] Could not find objective '"+objectiveName.Value+"' in quest '"+q.name+"'");
      Finish ();
    }
  }
}