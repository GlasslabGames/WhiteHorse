using System;
using UnityEngine;
using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker {
	
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.TooltipAttribute("Sets the value of a field in an element of a Lua table..")]
	public class SetLuaField : FsmStateAction {
		
		[RequiredField]
		[HutongGames.PlayMaker.TooltipAttribute("The table to set")]
		public LuaTableEnum table;
		
		[RequiredField]
		[HutongGames.PlayMaker.TooltipAttribute("The element in the table (e.g., 'Player' in Actor['Player'].Age)")]
		public FsmString element;
		
		[RequiredField]
		[HutongGames.PlayMaker.TooltipAttribute("The field in the element (e.g., 'Age' in Actor['Player'].Age)")]
		public FsmString field;
		
		[HutongGames.PlayMaker.TooltipAttribute("The value of the field as a string")]
		public FsmString stringValue;
		
		[HutongGames.PlayMaker.TooltipAttribute("The value of the field as a float")]
		public FsmFloat floatValue;
		
		[HutongGames.PlayMaker.TooltipAttribute("The value of the field as a bool")]
		public FsmBool boolValue;
		
		public override void Reset() {
			table = LuaTableEnum.ItemTable;
			if (element != null) element.Value = string.Empty;
			if (field != null) field.Value = string.Empty;
			stringValue = null;
			floatValue = null;
			boolValue = null;
		}

		public override string ErrorCheck() {
			bool anyValue = (stringValue != null) || (floatValue != null) || (boolValue != null);
			return anyValue ? base.ErrorCheck() : "Assign at least one value field.";
		}
		
		public override void OnEnter() {
			if (PlayMakerTools.IsValueAssigned(element) && PlayMakerTools.IsValueAssigned(field)) {
				string tableName = PlayMakerTools.LuaTableName(table);
				if ((stringValue != null) && !stringValue.IsNone) DialogueLua.SetTableField(tableName, element.Value, field.Value, stringValue.Value);
				if ((floatValue != null) && !floatValue.IsNone) DialogueLua.SetTableField(tableName, element.Value, field.Value, floatValue.Value);
				if ((boolValue != null) && !boolValue.IsNone) DialogueLua.SetTableField(tableName, element.Value, field.Value, boolValue.Value);
			} else {
				LogWarning(string.Format("{0}: Element and Field must be assigned first.", DialogueDebug.Prefix));
			}
			Finish();

		}
		
	}
	
}