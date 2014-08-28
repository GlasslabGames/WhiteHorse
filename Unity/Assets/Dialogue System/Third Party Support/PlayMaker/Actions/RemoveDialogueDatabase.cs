using System;
using UnityEngine;
using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker {
	
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.TooltipAttribute("Removes a loaded dialogue database from the Dialogue Manager's master database.")]
	public class RemoveDialogueDatabase : FsmStateAction {
		
		[RequiredField]
		[HutongGames.PlayMaker.TooltipAttribute("The dialogue database to remove")]
		public DialogueDatabase database;
		
		public override void Reset() {
			database = null;
		}
		
		public override void OnEnter() {
			DialogueManager.RemoveDatabase(database);
			Finish();
		}
		
	}
	
}