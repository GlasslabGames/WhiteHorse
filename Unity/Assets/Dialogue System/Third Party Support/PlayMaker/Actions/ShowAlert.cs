using System;
using UnityEngine;
using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker {
	
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.TooltipAttribute("Shows an alert message using the dialogue UI.")]
	public class ShowAlert : FsmStateAction {
		
		[RequiredField]
		[HutongGames.PlayMaker.TooltipAttribute("The alert message to show")]
		public FsmString message;
		
		[HutongGames.PlayMaker.TooltipAttribute("The duration in seconds to show the alert message")]
		public FsmFloat duration = 5f;
		
		public override void Reset() {
			if (message != null) message.Value = string.Empty;
			if (duration != null) duration.Value = 5f;
		}
		
		public override void OnEnter() {
			DialogueManager.ShowAlert(message.Value, duration.Value);
			Finish();
		}
		
	}
	
}