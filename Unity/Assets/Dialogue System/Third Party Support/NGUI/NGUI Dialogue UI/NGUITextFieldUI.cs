using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.NGUI {

	/// <summary>
	/// NGUI text field UI implementation.
	/// </summary>
	[AddComponentMenu("Dialogue System/Third Party/NGUI/Text Field UI")]
	public class NGUITextFieldUI : MonoBehaviour, ITextFieldUI {

		/// <summary>
		/// The (optional) panel. If your text field UI contains more than a label and text field, you should
		/// assign the panel, too.
		/// </summary>
		public UIPanel panel;
		
		/// <summary>
		/// The label that will contain any label text prompting the user what to enter.
		/// </summary>
		public UILabel label;
		
		/// <summary>
		/// The text field.
		/// </summary>
		public UIInput textField;

		/// <summary>
		/// Set this <c>true</c> to prevent the user from deselecting the text field.
		/// </summary>
		public bool alwaysKeepFocus = true;
		
		/// <summary>
		/// This delegate must be called when the player accepts the input in the text field.
		/// </summary>
		private AcceptedTextDelegate acceptedText = null;

		private UIInput uiInput = null;

		void Start() {
			Show();
			uiInput = (textField != null) ? textField.GetComponent<UIInput>() : null;
			if (DialogueDebug.LogWarnings && (uiInput == null)) Debug.LogWarning(string.Format("{0}: No UIInput was found in the text field {1}. TextInput() sequencer commands won't work.", DialogueDebug.Prefix, name));
			Hide();
		}
		
		/// <summary>
		/// Starts the text input field.
		/// </summary>
		/// <param name="labelText">The label text.</param>
		/// <param name="text">The current value to use for the input field.</param>
		/// <param name="maxLength">Max length, or <c>0</c> for unlimited.</param>
		/// <param name="acceptedText">The delegate to call when accepting text.</param>
		public void StartTextInput(string labelText, string text, int maxLength, AcceptedTextDelegate acceptedText) {
			if (label != null) label.text = labelText;
			if (uiInput != null) {
				uiInput.defaultText = text;
				uiInput.characterLimit = maxLength;
				uiInput.value = text;
				uiInput.isSelected = true;
			}
			this.acceptedText = acceptedText;
			Show();
		}

		/// <summary>
		/// Ensure that the text field keeps focus.
		/// </summary>
		public void Update() {
			if (alwaysKeepFocus && (uiInput != null) && uiInput.gameObject.activeInHierarchy) {
				if (!uiInput.isSelected) StartCoroutine(ReclaimFocus());
			}
		}

		/// <summary>
		/// It takes one full frame (i.e., after the complete input cycle) for another control
		/// to get focus. This coroutine waits two frames, then gives focus back to the text field.
		/// </summary>
		private IEnumerator ReclaimFocus() {
			yield return null;
			yield return null;
			uiInput.isSelected = true;
		}
		
		/// <summary>
		/// Cancels the text input field.
		/// </summary>
		public void CancelTextInput() {
			Hide();
		}
		
		/// <summary>
		/// Accepts the text input and calls the accept handler delegate.
		/// </summary>
		public void AcceptTextInput() {
			if (acceptedText != null) {
				if (uiInput != null) acceptedText(uiInput.value);
				acceptedText = null;
			}
			Hide();
		}

		private void Show() {
			SetActive(true);
		}

		private void Hide() {
			SetActive(false);
		}

		private void SetActive(bool value) {
			if (uiInput != null) uiInput.enabled = value;
			if (panel != null) {
				NGUITools.SetActive(panel.gameObject, value);
			} else {
				if (label != null) NGUITools.SetActive(label.gameObject, value);
				if (textField != null) NGUITools.SetActive(textField.gameObject, value);
			}
		}
		
	}

}
