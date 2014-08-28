using System;
using UnityEngine;
using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker {
	
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.TooltipAttribute("Applies savegame data stored in a string variable.")]
	public class ApplySavegameData : FsmStateAction {
		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.TooltipAttribute("The variable containing savegame data")]
		public FsmString savegameData;
		
		[HutongGames.PlayMaker.TooltipAttribute("Tick to reset to the default dialogue database, clear to keep all loaded databases")]
		public FsmBool resetToInitialDatabase;
		
		public override void Reset() {
			savegameData = null;
			if (resetToInitialDatabase != null) resetToInitialDatabase.Value = false;
		}
		
		public override void OnEnter() {	
			DatabaseResetOptions databaseResetOption = resetToInitialDatabase.Value ? DatabaseResetOptions.RevertToDefault : DatabaseResetOptions.KeepAllLoaded;
			PersistentDataManager.ApplySaveData(savegameData.Value, databaseResetOption);
			Finish();
		}
		
	}
	
}