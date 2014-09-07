using UnityEngine;
using System.Collections;

public class ZoomedViewManager : MonoBehaviour {
	public Camera MainCamera;
	public float SecsBeforeDisplay; // when they press on a state, don't pop up the zoom view immediately
	private float m_wait;
	public bool Active;

	void Start() {
		Show (false);
	}

	void Update () {
		// if they released the button, hide the zoom
		if (Input.GetMouseButtonUp(0)) Show(false);
		// else if they pressed the button, start the countdown to showing the zoom
		else if (Input.GetMouseButtonDown (0)) {
			m_wait = SecsBeforeDisplay;
		}

		// then as long as they're pressing down, count down to showing the zoom
		if (Input.GetMouseButton (0) && m_wait >= 0) {
			m_wait -= Time.deltaTime;
			if (m_wait <= 0) {
				Show (true);
				m_wait = -1;
			}
		}

		// if the view is active, update its position
		if (Active) {
			Vector3 pos = MainCamera.ScreenToWorldPoint(Input.mousePosition);
			pos.z = transform.position.z;
			transform.position = pos;

			// change the magnification depending on how far east you are
			float t = Mathf.InverseLerp(-10f, 10f, pos.x);
			GetComponentInChildren<Camera>().orthographicSize = Mathf.Lerp(1.5f, 0.5f, t);
		}
	}
	
	void Show(bool visible) {
		Active = visible;
		foreach (Renderer r in GetComponentsInChildren<Renderer> (true)) {
			r.enabled = visible;
		}
	}
}
