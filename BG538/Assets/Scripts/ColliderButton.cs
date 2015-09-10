using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class ColliderButton : MonoBehaviour {
	public bool debug;
	public UnityEvent OnClick;

	void Start() {} // need a Start function or the enable checkbox won't be shown

	void OnMouseUpAsButton() {
		if (enabled) {
			if (debug) Debug.Log ("Clicked on " + this, this);

			OnClick.Invoke ();
		}
	}
}
