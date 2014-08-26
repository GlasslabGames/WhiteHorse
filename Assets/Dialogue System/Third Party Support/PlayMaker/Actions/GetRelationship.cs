using System;
using UnityEngine;
using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker {
	
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.TooltipAttribute("Gets the relationship value between two actors.")]
	public class GetRelationship : FsmStateAction {
		
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
		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.TooltipAttribute("Store the result in a Float variable")]
		public FsmFloat storeResult;
		
		public override void Reset() {
			if (actor1Name != null) actor1Name.Value = string.Empty;
			if (actor2Name != null) actor2Name.Value = string.Empty;
			if (relationshipType != null) relationshipType.Value = string.Empty;
			storeResult = null;
		}
		
		public override void OnEnter() {
			if ((actor1Name != null) && (actor2Name != null) && (relationshipType != null) && (storeResult != null)) {
				try {
					storeResult.Value = Lua.Run(string.Format("return GetRelationship(Actor[\"{0}\"], Actor[\"{1}\"], \"{2}\")", 
						DialogueLua.StringToTableIndex(actor1Name.Value), DialogueLua.StringToTableIndex(actor2Name.Value),
							relationshipType.Value), DialogueDebug.LogInfo).AsFloat;
				} catch (System.NullReferenceException) {
				}
			}
			Finish();
		}
		
	}
	
}
