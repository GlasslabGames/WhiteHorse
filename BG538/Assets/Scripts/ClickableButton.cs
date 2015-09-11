using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ClickableButton : MonoBehaviour, IPointerClickHandler {
	public bool debug;
	public UnityEvent ClickHandler; // This is visible in the inspector so you can add events there
	public Action OnClick; // This is easy to add callbacks to in code: button.OnClick += handleButtonClick

	void Start() {} // Require for the enable checkbox in the editor

	public void OnPointerClick(PointerEventData eventData) {
		if (enabled) {
			if (debug) Debug.Log ("Clicked on " + this, this);
			
			if (ClickHandler != null) ClickHandler.Invoke();
			if (OnClick != null) OnClick();
		}
	}
}