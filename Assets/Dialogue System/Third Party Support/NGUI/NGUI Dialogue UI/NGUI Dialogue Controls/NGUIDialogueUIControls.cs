using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.NGUI {
	
	/// <summary>
	/// Static utility class for NGUIDialogueUI.
	/// </summary>
	[System.Serializable]
	public static class NGUIDialogueUIControls {
		
		/// <summary>
		/// Sets a control active/inactive.
		/// </summary>
		/// <param name='control'>
		/// Control to set.
		/// </param>
		/// <param name='value'>
		/// <c>true</c> to set active; <c>false</c> to set inactive.
		/// </param>
		public static void SetControlActive(GameObject control, bool value) {
			if (control != null) {
				if ((value == true) && !control.activeSelf) {
					NGUITools.SetActive(control, true);
				} else if ((value == false) && control.activeSelf) {
					NGUITools.SetActive(control, false);
				}
			}
		}
		
	}
		
}
