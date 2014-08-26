using System;
using UnityEngine;
using HutongGames.PlayMaker;
using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace PixelCrushers.DialogueSystem.PlayMaker {
	
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.TooltipAttribute("Gets the description of a quest entry in a quest.")]
	public class GetQuestEntry : FsmStateAction {
		
		[RequiredField]
		[HutongGames.PlayMaker.TooltipAttribute("The name of the quest")]
		public FsmString questName;
		
		[RequiredField]
		[HutongGames.PlayMaker.TooltipAttribute("The quest entry number (from 1)")]
		public FsmInt entryNumber;
		
		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.TooltipAttribute("Store the result in a String variable")]
		public FsmString storeResult;
		
		public override void Reset() {
			if (questName != null) questName.Value = string.Empty;
			if (entryNumber != null) entryNumber.Value = 0;
			storeResult = null;
		}
		
		public override void OnEnter() {
			if ((questName == null) || (string.IsNullOrEmpty(questName.Value))) {
				LogError(string.Format("{0}: Quest Name is null or blank.", DialogueDebug.Prefix));
			} else if (entryNumber == null) {
				LogError(string.Format("{0}: Entry Number is not assigned.", DialogueDebug.Prefix));
			} else if (storeResult != null) {
				storeResult.Value = QuestLog.GetQuestEntry(questName.Value, Mathf.Max (1, entryNumber.Value));
			}
			Finish();
		}
		
	}
	
}