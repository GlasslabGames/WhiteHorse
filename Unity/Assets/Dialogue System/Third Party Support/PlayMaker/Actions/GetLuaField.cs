using System;
using UnityEngine;
using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker {
	
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.TooltipAttribute("Gets the value of a field in a Lua table.")]
	public class GetLuaField : FsmStateAction {
		
		[RequiredField]
		[HutongGames.PlayMaker.TooltipAttribute("The table to get")]
		public LuaTableEnum table;

		[RequiredField]
		[HutongGames.PlayMaker.TooltipAttribute("The element in the table (e.g., 'Player' in Actor['Player'].Age)")]
		public FsmString element;

		[RequiredField]
		[HutongGames.PlayMaker.TooltipAttribute("The field in the element (e.g., 'Age' in Actor['Player'].Age)")]
		public FsmString field;
		
		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.TooltipAttribute("The value of the field as a string")]
		public FsmString storeStringResult;
		
		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.TooltipAttribute("The value of the field as a float")]
		public FsmFloat storeFloatResult;
		
		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.TooltipAttribute("The value of the field as a bool")]
		public FsmBool storeBoolResult;
		
		[HutongGames.PlayMaker.TooltipAttribute("Repeat every frame while the state is active.")]
		public bool everyFrame;
		
		public override void Reset() {
			table = LuaTableEnum.ItemTable;
			if (element != null) element.Value = string.Empty;
			if (field != null) field.Value = string.Empty;
			storeStringResult = null;
			storeFloatResult = null;
			storeBoolResult = null;
		}

		public override string ErrorCheck() {
			bool anyResultVariable = (storeStringResult != null) || (storeFloatResult != null) || (storeBoolResult != null);
			return anyResultVariable ? base.ErrorCheck() : "Assign at least one store result variable.";
		}
		
		public override void OnEnter() {
			if (PlayMakerTools.IsValueAssigned(element) && PlayMakerTools.IsValueAssigned(field)) {
				string tableName = PlayMakerTools.LuaTableName(table);
				Lua.Result luaResult = DialogueLua.GetTableField(tableName, element.Value, field.Value);
				if (storeStringResult != null) storeStringResult.Value = luaResult.AsString;
				if (storeFloatResult != null) storeFloatResult.Value = luaResult.AsFloat;
				if (storeBoolResult != null) storeBoolResult.Value = luaResult.AsBool;
			} else {
				LogWarning(string.Format("{0}: Element and Field must be assigned first.", DialogueDebug.Prefix));
			}
			if (!everyFrame) Finish();
		}

	}
	
}