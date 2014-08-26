using System;
using UnityEngine;
using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker {
	
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.TooltipAttribute("Deletes a quest from the quest system.")]
	public class DeleteQuest : FsmStateAction {
		
		[RequiredField]
		[HutongGames.PlayMaker.TooltipAttribute("The name of the quest")]
		public FsmString questName;
		
		public override void Reset() {
			if (questName != null) questName.Value = string.Empty;
		}
		
		public override void OnEnter() {
			QuestLog.DeleteQuest(questName.Value);
			Finish();
		}
		
	}
	
}