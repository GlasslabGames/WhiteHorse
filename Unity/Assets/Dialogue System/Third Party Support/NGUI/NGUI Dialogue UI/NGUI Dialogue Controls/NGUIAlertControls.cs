using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.NGUI {
	
	/// <summary>
	/// NGUI controls for NGUIDialogueUI's alert message.
	/// </summary>
	[System.Serializable]
	public class NGUIAlertControls : AbstractUIAlertControls {

		/// <summary>
		/// The panel containing the alert controls. A panel is optional, but you may want one
		/// so you can include a background image, panel-wide effects, etc.
		/// </summary>
		public UIPanel panel;
		
		/// <summary>
		/// The label used to show the alert message text.
		/// </summary>
		public UILabel line;
		
		/// <summary>
		/// Optional continue button to close the alert immediately.
		/// </summary>
		public UIButton continueButton;

		/// <summary>
		/// The duration to fade out at the end of showing the alert.
		/// </summary>
		public float fadeOutDuration = 1;
		
		private enum AlertState { Hidden, Showing, Fading };
		
		private AlertState state = AlertState.Hidden;

		/// <summary>
		/// Is an alert currently showing?
		/// </summary>
		/// <value>
		/// <c>true</c> if showing; otherwise, <c>false</c>.
		/// </value>
		public override bool IsVisible {
			get { return state == AlertState.Showing; }
		}
		
		/// <summary>
		/// Sets the alert controls active.
		/// </summary>
		/// <param name='value'>
		/// <c>true</c> for active.
		/// </param>
		public override void SetActive(bool value) {
			if (line != null) NGUIDialogueUIControls.SetControlActive(line.gameObject, value);
			if (panel != null) NGUIDialogueUIControls.SetControlActive(panel.gameObject, value);
		}
		
		/// <summary>
		/// Sets the alert message and begins the fade in/out routine.
		/// </summary>
		/// <param name='message'>
		/// Alert message.
		/// </param>
		/// <param name='duration'>
		/// Duration to show message.
		/// </param>
		public override void SetMessage(string message, float duration) {
			if (!string.IsNullOrEmpty(message)) {
				alertDoneTime = DialogueTime.time + duration;
				if (line != null) line.text = FormattedText.Parse(message, DialogueManager.MasterDatabase.emphasisSettings).text;
				Show();
				state = AlertState.Showing;
				TweenAlpha.Begin(line.gameObject, 0.2f, 1);
			} else {
				Hide();
				state = AlertState.Hidden;
			}
		}
		

		/// <summary>
		/// Fades out the alert line.
		/// </summary>
		/// <returns>
		/// IEnumerator (coroutine).
		/// </returns>
		/// <param name='FadedOutHandler'>
		/// Handler to call when finished fading out.
		/// </param>
		public IEnumerator FadeOut(Action FadedOutHandler) {
			state = AlertState.Fading;
			TweenAlpha.Begin(line.gameObject, fadeOutDuration, 0);
			yield return new WaitForSeconds(fadeOutDuration + 0.1f); // Wait 0.1s extra to allow TweenAlpha to close out.
			Hide();
			state = AlertState.Hidden;
			if (FadedOutHandler != null) FadedOutHandler();
		}

	}
		
}
