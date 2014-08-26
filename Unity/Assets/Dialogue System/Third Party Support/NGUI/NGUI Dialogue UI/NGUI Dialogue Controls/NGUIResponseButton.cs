using UnityEngine;
using System;
using System.Collections;

namespace PixelCrushers.DialogueSystem.NGUI {
	
	/// <summary>
	/// An NGUI response button for use with NGUIDialogueUIControls. Add this component to every
	/// response button in the dialogue UI. The button should have, at minimum, a label and a collider.
	/// A background sprite is optional.
	/// </summary>
	[AddComponentMenu("Dialogue System/Third Party/NGUI/Response Button")]
	public class NGUIResponseButton : MonoBehaviour {

		/// <summary>
		/// The NGUI button.
		/// </summary>
		public UIButton nguiButton;
		
		/// <summary>
		/// The NGUI label that will display the response text.
		/// </summary>
		public UILabel nguiLabel;
		
		/// <summary>
		/// The default color for response text.
		/// </summary>
		public Color defaultColor = Color.white;

		/// <summary>
		/// Set <c>true</c> to set the button color when applying emphasis tags.
		/// </summary>
		public bool setButtonColor = true;

		/// <summary>
		/// Set <c>true</c> to set the label color when applying emphasis tags.
		/// </summary>
		public bool setLabelColor = true;
		
		/// <summary>
		/// Gets or sets the response text.
		/// </summary>
		/// <value>
		/// The text.
		/// </value>
		public string Text {
			get { 
				return (nguiLabel != null) ? nguiLabel.text : string.Empty; 
			}
			set { 
				if (nguiLabel != null) {
					nguiLabel.text = value; 
				} else {
					if (DialogueDebug.LogErrors) Debug.LogError(string.Format("{0}: No UILabel is unassigned on {1}", DialogueDebug.Prefix, name));
				}
			}
		}
		
		/// <summary>
		/// Indicates whether the button is an allowable response.
		/// </summary>
		/// <value>
		/// <c>true</c> if clickable; otherwise, <c>false</c>.
		/// </value>
		public bool clickable {
			get { return (collider != null) ? collider.enabled : false; }
			set { if (collider != null) collider.enabled = value; }
		}
		
		/// <summary>
		/// Indicates whether the button is shown or not.
		/// </summary>
		/// <value>
		/// <c>true</c> if visible; otherwise, <c>false</c>.
		/// </value>
		public bool visible { get; set; }
		
		/// <summary>
		/// Gets or sets the response associated with this button. If the player clicks this 
		/// button, this response is sent back to the dialogue system.
		/// </summary>
		/// <value>
		/// The response.
		/// </value>
		public Response response { get; set; }
		
		/// <summary>
		/// Gets or sets the target that will receive click notifications.
		/// </summary>
		/// <value>
		/// The target.
		/// </value>
		public Transform target { get; set; }
		

		/// <summary>
		/// Clears the button.
		/// </summary>
		public void Reset() {
			Text = string.Empty;
			clickable = false;
			visible = false;
			response = null;
			SetColor(defaultColor);
		}

		/// <summary>
		/// Sets the button's text using the specified formatted text.
		/// </summary>
		/// <param name='formattedText'>
		/// The formatted text for the button label.
		/// </param>
		public void SetFormattedText(FormattedText formattedText) {
			if (formattedText != null) {
				Text = formattedText.text;
				SetColor((formattedText.emphases.Length > 0) ? formattedText.emphases[0].color : defaultColor);
			}
		}
		
		/// <summary>
		/// Sets the button's text using plain text.
		/// </summary>
		/// <param name='unformattedText'>
		/// Unformatted text for the button label.
		/// </param>
		public void SetUnformattedText(string unformattedText) {
			Text = unformattedText;
			SetColor(defaultColor);
		}
		
		protected virtual void SetColor(Color currentColor) {
			if (nguiButton != null) {
				if (setButtonColor) nguiButton.defaultColor = currentColor;
			} else {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: No UIButton is assigned to {1}", DialogueDebug.Prefix, name));
			}
			if (nguiLabel != null) {
				if (setLabelColor) nguiLabel.color = currentColor;
			} else {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: No UILabel is assigned to {1}", DialogueDebug.Prefix, name));
			}
		}
	
		/// <summary>
		/// Handles a button click by calling the response handler.
		/// </summary>
		public void OnClick() {
			if (target != null) target.SendMessage("OnClick", response, SendMessageOptions.RequireReceiver);
		}
		
	}

}