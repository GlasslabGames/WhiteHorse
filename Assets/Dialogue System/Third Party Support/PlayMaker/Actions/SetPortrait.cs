using System;
using UnityEngine;
using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker {
	
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.TooltipAttribute("Sets an actor's current portrait.")]
	public class SetPortrait : FsmStateAction {
		
		[RequiredField]
		[HutongGames.PlayMaker.TooltipAttribute("The name of the actor as defined in the database")]
		public FsmString actorName;
		
		[RequiredField]
		[HutongGames.PlayMaker.TooltipAttribute("Use 'default', 'pic=#', or a texture name in a Resources folder")]
		public FsmString portraitValue;
		
		public override void Reset() {
			if (actorName != null) actorName.Value = string.Empty;
			if (portraitValue != null) portraitValue.Value = string.Empty;
		}
		
		public override void OnEnter() {
			if (PlayMakerTools.IsValueAssigned(actorName) && PlayMakerTools.IsValueAssigned(portraitValue)) {
				DialogueManager.SetPortrait(actorName.Value, portraitValue.Value);
			} else {
				LogError(string.Format("{0}: You must assign the actor name and portrait value.", DialogueDebug.Prefix));
			}
			Finish();
		}
		
	}
	
}