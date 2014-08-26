using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.NGUI {

	/// <summary>
	/// This component implements IDialogueUI using Tasharen Entertainment's NGUI. It's based on 
	/// AbstractDialogueUI and compiles the NGUI versions of the controls defined in 
	/// NGUISubtitleControls, NGUIResponseMenuControls, NGUIAlertControls, etc.
	///
	/// To use this component, build an NGUI layout (or use a pre-built one in the Prefabs folder)
	/// and assign the UI control properties. You can save an NGUIDialogueUI as a prefab and 
	/// assign the prefab or an instance to the DialogueManager.
	/// 
	/// The required controls are:
	/// - UIRoot
	/// - NPC subtitle line
	/// - PC subtitle line
	/// - Response menu buttons
	/// 
	/// The other control properties are optional. This component will activate and deactivate
	/// controls as they are needed in the conversation. You can add NGUI decorator scripts to the
	/// controls. If they trigger on enable, they'll play each time the control is shown.
	/// </summary>
	[AddComponentMenu("Dialogue System/Third Party/NGUI/Dialogue UI")]
	public class NGUIDialogueUI : AbstractDialogueUI {
		
		/// <summary>
		/// The UI root.
		/// </summary>
		public UIRoot uiRoot;
		
		/// <summary>
		/// The dialogue controls used in conversations.
		/// </summary>
		public NGUIDialogueControls dialogue;
		
		/// <summary>
		/// QTE (Quick Time Event) indicators.
		/// </summary>
		public UIPanel[] qteIndicators;
		
		/// <summary>
		/// The alert message controls.
		/// </summary>
		public NGUIAlertControls alert;

		public bool deactivateUIRootWhenHidden = true;
		
		private NGUIUIRoot nguiUIRoot;
		
		private NGUIQTEControls nguiQTEControls;
		
		public override AbstractUIRoot UIRoot {
			get { return nguiUIRoot; }
		}
		
		public override AbstractDialogueUIControls Dialogue {
			get { return dialogue; }
		}
		
		public override AbstractUIQTEControls QTEs {
			get { return nguiQTEControls; }
		}
		
		public override AbstractUIAlertControls Alert {
			get { return alert; }
		}
		
		/// <summary>
		/// Sets up the component.
		/// </summary>
		public override void Awake() {
			base.Awake();
			FindControls();
		}
		
		/// <summary>
		/// Makes sure we have a UIRoot and logs warnings if any critical controls are unassigned.
		/// </summary>
		private void FindControls() {
			if (uiRoot == null) uiRoot = GetComponentInChildren<UIRoot>();
			nguiUIRoot = new NGUIUIRoot(uiRoot, deactivateUIRootWhenHidden);
			nguiQTEControls = new NGUIQTEControls(qteIndicators);
			SetupContinueButton(dialogue.npcSubtitle.continueButton);
			SetupContinueButton(dialogue.pcSubtitle.continueButton);
			SetupContinueButton(alert.continueButton);
			if (DialogueDebug.LogErrors) {
				if (uiRoot == null) Debug.LogError(string.Format("{0}: NGUIDialogueUI can't find UIRoot and won't be able to display dialogue.", DialogueDebug.Prefix));
				if (DialogueDebug.LogWarnings) {
					if (dialogue.npcSubtitle.line == null) Debug.LogWarning(string.Format("{0}: NGUIDialogueUI NPC Subtitle Line needs to be assigned.", DialogueDebug.Prefix));
					if (dialogue.pcSubtitle.line == null) Debug.LogWarning(string.Format("{0}: NGUIDialogueUI PC Subtitle Line needs to be assigned.", DialogueDebug.Prefix));
					if (dialogue.responseMenu.buttons.Length == 0) Debug.LogWarning(string.Format("{0}: NGUIDialogueUI Response buttons need to be assigned.", DialogueDebug.Prefix));
					if (alert.line == null) Debug.LogWarning(string.Format("{0}: NGUIDialogueUI Alert Line needs to be assigned.", DialogueDebug.Prefix));
				}
			}
		}

		/// <summary>
		/// Sets up the continue button if the user has left it unconfigured. Makes sure that it
		/// has an OnClick event delegate that sends OnContinue to this dialogue UI.
		/// </summary>
		/// <param name='continueButton'>
		/// Continue button.
		/// </param>
		private void SetupContinueButton(UIButton continueButton) {
			if (continueButton != null) {
				if (continueButton.onClick == null) {
					continueButton.onClick = new List<EventDelegate>();
					foreach (EventDelegate eventDelegate in continueButton.onClick) {
						if ((eventDelegate.target == this) && string.Equals(eventDelegate.methodName, "OnContinue")) {
							return; // Already set up.
						}
					}
					continueButton.onClick.Add(new EventDelegate(this, "OnContinue"));
				}
			}
		}

		public override void ShowAlert(string message, float duration) {
			base.ShowAlert(message, duration);
			Invoke("FadeOutAlert", duration - alert.fadeOutDuration);
		}

		public override void OnContinue() {
			CancelInvoke("FadeOutAlert");
			base.OnContinue();
		}

		private void FadeOutAlert() {
			StartCoroutine(alert.FadeOut(null));
		}

	}

}
