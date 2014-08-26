using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.NGUI {

	/// <summary>
	/// NGUI Quick Time Event (QTE) indicator controls.
	/// </summary>
	[System.Serializable]
	public class NGUIQTEControls : AbstractUIQTEControls {

		/// <summary>
		/// QTE (Quick Time Event) indicators.
		/// </summary>
		public UIPanel[] qteIndicators;
		
		private int numVisibleQTEIndicators = 0;
		
		public NGUIQTEControls(UIPanel[] qteIndicators) {
			this.qteIndicators = qteIndicators;
		}
	
		/// <summary>
		/// Gets a value indicating whether any QTE indicators are visible.
		/// </summary>
		/// <value>
		/// <c>true</c> if visible; otherwise, <c>false</c>.
		/// </value>
		public override bool AreVisible {
			get { return (numVisibleQTEIndicators > 0); }
		}
		
		/// <summary>
		/// Sets the QTE controls active/inactive.
		/// </summary>
		/// <param name='value'>
		/// <c>true</c> for active; <c>false</c> for inactive.
		/// </param>
		public override void SetActive(bool value) {
			if (value == false) {
				numVisibleQTEIndicators = 0;
				foreach (var qteIndicator in qteIndicators) {
					if (qteIndicator.gameObject != null) NGUIDialogueUIControls.SetControlActive(qteIndicator.gameObject, false);
				}
			}
		}
		
		/// <summary>
		/// Shows the QTE indicator specified by the index. 
		/// </summary>
		/// <param name='index'>
		/// Zero-based index of the indicator.
		/// </param>
		public override void ShowIndicator(int index) {
			if (IsValidQTEIndex(index) && !IsQTEIndicatorVisible(index)) {
				if (qteIndicators[index] != null) NGUIDialogueUIControls.SetControlActive(qteIndicators[index].gameObject, true);
				numVisibleQTEIndicators++;
			}
		}

		/// <summary>
		/// Hides the QTE indicator specified by the index.
		/// </summary>
		/// <param name='index'>
		/// Zero-based index of the indicator.
		/// </param>
		public override void HideIndicator(int index) {
			if (IsValidQTEIndex(index) && IsQTEIndicatorVisible(index)) {
				if (qteIndicators[index] != null) NGUIDialogueUIControls.SetControlActive(qteIndicators[index].gameObject, false);
				numVisibleQTEIndicators--;
			}
		}

		private bool IsQTEIndicatorVisible(int index) {
			return IsValidQTEIndex(index) ? qteIndicators[index].gameObject.activeSelf : false;
		}
		
		private bool IsValidQTEIndex(int index) {
			return (0 <= index) && (index < qteIndicators.Length);
		}
		
	}

}
