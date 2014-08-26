using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.NGUI {

	/// <summary>
	/// This works just like NGUIDialogueUI except it uses the speaker's bark UI
	/// instead of the dialogue UI labels.
	/// </summary>
	[AddComponentMenu("Dialogue System/Third Party/NGUI/Dialogue Bark UI")]
	public class NGUIDialogueBarkUI : NGUIDialogueUI {
		
		public override void ShowSubtitle(Subtitle subtitle) {
			StartCoroutine(BarkController.Bark(subtitle));
		}
		
	}

}
