using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;

public class ColliderButton : MonoBehaviour {
	public bool debug;
	public UnityEvent ClickHandler; // This is visible in the inspector so you can add events there
	public Action OnClick; // This is easy to add callbacks to in code: button.OnClick += handleButtonClick

	void Start() {} // need a Start function or the enable checkbox won't be shown

	void OnMouseUpAsButton() {
		if (enabled) {
			if (debug) Debug.Log ("Clicked on " + this, this);

			if (ClickHandler != null) ClickHandler.Invoke();
			if (OnClick != null) OnClick();
		}
	}
}
