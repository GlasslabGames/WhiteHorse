using System;
using UnityEngine;
using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker {
	
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.TooltipAttribute("Decrements a relationship value between two actors.")]
	public class DecRelationship : FsmStateAction {
		
		[RequiredField]
		[HutongGames.PlayMaker.TooltipAttribute("The name of actor1 in the Actor[] table")]
		public FsmString actor1Name;
		
		[RequiredField]
		[HutongGames.PlayMaker.TooltipAttribute("The name of actor2 in the Actor[] table")]
		public FsmString actor2Name;
		
		[RequiredField]
		[HutongGames.PlayMaker.TooltipAttribute("The relationship type")]
		public FsmString relationshipType;
		
		[RequiredField]
		[HutongGames.PlayMaker.TooltipAttribute("The amount to decrement the relationship value")]
		public FsmFloat decrementAmount;
		
		public override void Reset() {
			if (actor1Name != null) actor1Name.Value = string.Empty;
			if (actor2Name != null) actor2Name.Value = string.Empty;
			if (relationshipType != null) relationshipType.Value = string.Empty;
			if (decrementAmount != null) decrementAmount.Value = 0;
		}
		
		public override void OnEnter() {
			if ((actor1Name != null) && (actor2Name != null) && (relationshipType != null) && (decrementAmount != null)) {
				Lua.Run(string.Format("DecRelationship(Actor[\"{0}\"], Actor[\"{1}\"], \"{2}\", {3})", 
					DialogueLua.StringToTableIndex(actor1Name.Value), DialogueLua.StringToTableIndex(actor2Name.Value),
					relationshipType.Value, decrementAmount.Value), DialogueDebug.LogInfo);
			}
			Finish();
		}
		
	}
	
}