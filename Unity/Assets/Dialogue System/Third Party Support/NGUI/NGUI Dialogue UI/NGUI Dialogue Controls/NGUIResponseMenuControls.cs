using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.NGUI {
	
	/// <summary>
	/// Response menu controls for NGUIDialogueUI.
	/// </summary>
	[System.Serializable]
	public class NGUIResponseMenuControls : AbstractUIResponseMenuControls {
		
		/// <summary>
		/// The panel containing the response menu controls. A panel is optional, but you may want one
		/// so you can include a background image, panel-wide effects, etc.
		/// </summary>
		public UIPanel panel;
		
		/// <summary>
		/// The label that will show the PC portrait image.
		/// </summary>
		public UITexture pcImage;
		
		/// <summary>
		/// The label that will show the PC name.
		/// </summary>
		public UILabel pcName;
		
		/// <summary>
		/// The reminder of the last subtitle.
		/// </summary>
		public NGUISubtitleControls subtitleReminder;
		
		/// <summary>
		/// The (optional) timer.
		/// </summary>
		public UISlider timer;
		
		/// <summary>
		/// The response buttons.
		/// </summary>
		public NGUIResponseButton[] buttons;
		
		private NGUITimer nguiTimer = null;
		
		private Texture2D pcPortraitTexture = null;
		private string pcPortraitName = null;
		
		/// <summary>
		/// Sets the PC portrait name and texture to use in the response menu.
		/// </summary>
		/// <param name="portraitTexture">Portrait texture.</param>
		/// <param name="portraitName">Portrait name.</param>
		public override void SetPCPortrait(Texture2D portraitTexture, string portraitName) {
			pcPortraitTexture = portraitTexture;
			pcPortraitName = portraitName;
		}
		
		/// <summary>
		/// Sets the portrait texture to use in the response menu if the named actor is the player.
		/// This is used to immediately update the GUI control if the SetPortrait() sequencer 
		/// command changes the portrait texture.
		/// </summary>
		/// <param name="actorName">Actor name in database.</param>
		/// <param name="portraitTexture">Portrait texture.</param>
		public override void SetActorPortraitTexture(string actorName, Texture2D portraitTexture) {
			if (string.Equals(actorName, pcPortraitName)) {
				Texture2D actorPortraitTexture = AbstractDialogueUI.GetValidPortraitTexture(actorName, portraitTexture);
				pcPortraitTexture = actorPortraitTexture;
				if ((pcImage != null) && (DialogueManager.MasterDatabase.IsPlayer(actorName))) {
					pcImage.mainTexture = actorPortraitTexture;
				}
			}
		}
		
		public override AbstractUISubtitleControls SubtitleReminder {
			get { return subtitleReminder; }
		}
		
		/// <summary>
		/// Sets the controls active/inactive, except this method never activates the timer. If the
		/// UI's display settings specify a timeout, then the UI will call StartTimer() to manually
		/// activate the timer.
		/// </summary>
		/// <param name='value'>
		/// Value (<c>true</c> for active; otherwise inactive).
		/// </param>
		public override void SetActive(bool value) {
			SubtitleReminder.SetActive(value && SubtitleReminder.HasText);
			foreach (var button in buttons) {
				NGUIDialogueUIControls.SetControlActive(button.gameObject, value && button.visible);
			}
			if (timer != null) NGUITools.SetActive(timer.gameObject, false);
			if (pcName != null) NGUIDialogueUIControls.SetControlActive(pcName.gameObject, value);
			if (pcImage != null) NGUIDialogueUIControls.SetControlActive(pcImage.gameObject, value);
			if (panel != null) NGUIDialogueUIControls.SetControlActive(panel.gameObject, value);
			if (value == true) {
				if ((pcImage != null) && (pcPortraitTexture != null)) pcImage.mainTexture = pcPortraitTexture;
				if ((pcName != null) && (pcPortraitName != null)) pcName.text = pcPortraitName;
			}
		}
		
		/// <summary>
		/// Clears the response buttons.
		/// </summary>
		protected override void ClearResponseButtons() {
			if (buttons != null) {
				for (int i = 0; i < buttons.Length; i++) {
					buttons[i].Reset();
					buttons[i].visible = showUnusedButtons;
				}
			}
		}
		
		/// <summary>
		/// Sets the response buttons.
		/// </summary>
		/// <param name='responses'>
		/// Responses.
		/// </param>
		/// <param name='target'>
		/// Target that will receive OnClick events from the buttons.
		/// </param>
		protected override void SetResponseButtons(Response[] responses, Transform target) {
			if ((buttons != null) && (buttons.Length > 0) && (responses != null)) {
				
				// Add explicitly-positioned buttons:
				for (int i = 0; i < responses.Length; i++) {
					if (responses[i].formattedText.position != FormattedText.NoAssignedPosition) {
						int position = Mathf.Clamp(responses[i].formattedText.position, 0, buttons.Length - 1);
						SetResponseButton(buttons[position], responses[i], target);
					}
				}
				
				// Auto-position remaining buttons:
				if (buttonAlignment == ResponseButtonAlignment.ToFirst) {
					
					// Align to first, so add in order to front:
					for (int i = 0; i < Mathf.Min(buttons.Length, responses.Length); i++) {
						if (responses[i].formattedText.position == FormattedText.NoAssignedPosition) {
							int position = Mathf.Clamp(GetNextAvailableResponseButtonPosition(0, 1), 0, buttons.Length - 1);
							SetResponseButton(buttons[position], responses[i], target);
						}
					}
				} else {
					
					// Align to last, so add in reverse order to back:
					for (int i = Mathf.Min(buttons.Length, responses.Length) - 1; i >= 0; i--) {
						if (responses[i].formattedText.position == FormattedText.NoAssignedPosition) {
							int position = Mathf.Clamp(GetNextAvailableResponseButtonPosition(buttons.Length - 1, -1), 0, buttons.Length - 1);
							SetResponseButton(buttons[position], responses[i], target);
						}
					}
				}
			}
		}
		
		private void SetResponseButton(NGUIResponseButton button, Response response, Transform target) {
			if (button != null) {
				button.visible = true;
				button.clickable = true;
				button.target = target;
				if (response != null) button.SetFormattedText(response.formattedText);
				button.response = response;
			}
		}
		
		private int GetNextAvailableResponseButtonPosition(int start, int direction) {
			if (buttons != null) {
				int position = start;
				while ((0 <= position) && (position < buttons.Length)) {
					if (buttons[position].clickable) {
						position += direction;
					} else {
						return position;
					}
				}
			}
			return 5;
		}
	
		/// <summary>
		/// Starts the timer.
		/// </summary>
		/// <param name='timeout'>
		/// Timeout duration in seconds.
		/// </param>
		public override void StartTimer(float timeout) {
			if (timer != null) {
				if (nguiTimer == null) {
					NGUIDialogueUIControls.SetControlActive(timer.gameObject, true);
					nguiTimer = timer.GetComponent<NGUITimer>();
					if (nguiTimer == null) nguiTimer = timer.gameObject.AddComponent<NGUITimer>();
					NGUIDialogueUIControls.SetControlActive(timer.gameObject, false);
				}
				if (nguiTimer != null) {
					NGUIDialogueUIControls.SetControlActive(timer.gameObject, true);
					nguiTimer.StartCountdown(timeout, OnTimeout);
				} else {
					if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: No NGUITimer component found on timer", DialogueDebug.Prefix));
				}
			}
		}
		
		/// <summary>
		/// This method is called if the timer runs out. It selects the first response.
		/// </summary>
		public void OnTimeout() {
			DialogueManager.Instance.SendMessage("OnConversationTimeout");
		}
		
	}

}
