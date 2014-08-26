using System;
using UnityEngine;
using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker {
	
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.TooltipAttribute("Checks if tracking is enabled on a quest.")]
	public class IsQuestTrackingEnabled : FsmStateAction {
		
		[RequiredField]
		[HutongGames.PlayMaker.TooltipAttribute("The name of the quest")]
		public FsmString questName;
		
		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.TooltipAttribute("Store the result in a Bool variable")]
		public FsmBool storeResult;

		public FsmEvent enabledEvent;
		public FsmEvent disabledEvent;

		public override void Reset() {
			if (questName != null) questName.Value = string.Empty;
			storeResult = null;
		}
		
		public override void OnEnter() {
			if ((questName == null) || (string.IsNullOrEmpty(questName.Value))) {
				LogError(string.Format("{0}: Quest Name is null or blank.", DialogueDebug.Prefix));
			} else {
				bool track = QuestLog.IsQuestTrackingEnabled(questName.Value);
				if (storeResult != null) storeResult.Value = track;
				Fsm.Event(track ? enabledEvent : disabledEvent);
			}
			Finish();
		}
		
	}
	
}