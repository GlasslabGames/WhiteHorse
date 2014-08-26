using UnityEngine;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory("QuestManager")]
  [Tooltip("Activate Objective")]
  public class ActivateObjective : FsmStateAction
  {
    private const string ALERT_TEXT = "Objective Received!";

    [RequiredField]
    [Tooltip("Name of the Objective GameObject")]
    public FsmString objectiveName;

    [Tooltip("Name of the Quest GameObject. If empty the action will attempt to search the owner of this FSM.")]
    public FsmString questName;
    
    [Tooltip("Whether to display an alert that a new objective has been activated.")]
    public FsmBool showAlert;

    public override void Reset()
    {
      showAlert = false;
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
          Debug.LogError("[ActivateObjective(FSMAction)] Tried to find owner quest, but FSM has " + ownerQuests.Length + " quests!");
        }

        q = ownerQuests[0];
      }

      List<Objective> questObjectives = q.GetObjectives ();
      foreach (Objective o in questObjectives)
      {
        if (o.name == objectiveName.Value)
        {
          o.gameObject.SetActive(true);
          if (showAlert.Value)
          {
            //DialogueManager.ShowAlert(ALERT_TEXT);
          }
          Finish ();
          return;
        }
      }

      Debug.LogError ("[ActivateObjective(FSMAction)] Could not find objective '"+objectiveName.Value+"' in quest '"+q.name+"'");
      Finish ();
    }
  }
}