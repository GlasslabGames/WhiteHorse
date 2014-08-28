using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugTapListener : MonoBehaviour {

	public System.Action Callback { get; set; }

	public float AcceptableDist; // if < 1, use percent of the screen
	public List<Vector2> Points; //(0, 0) for the bottom left and (1, 1) for the top right
	public List<float> RequiredTimes;
	private int m_index;
	private bool m_clicking;
	private float m_time;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < Points.Count; i++) {
			Vector2 v = Points[i];
			if (v.x <= 1f && v.y <= 1f) {
				v.Scale(new Vector2(Screen.width, Screen.height));
				Points[i] = v;
			}
		}

		if (AcceptableDist < 1) {
			AcceptableDist *= (Screen.width + Screen.height) * 0.5f; // average the height and width
			// e.g. 0.5 corresponds to about half the screen
		}
		m_time = 0f;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			Vector2 pos = Input.mousePosition;
			if (Vector2.Distance(pos, Points[m_index]) < AcceptableDist) {
				m_clicking = true;
			} else if (Vector2.Distance(pos, Points[0]) < AcceptableDist) {
				m_clicking = true;
				m_index = 0;
			} else {
				m_index = 0;
			}
			m_time = 0f;
		}
		if (m_clicking && m_index < Points.Count && m_index < RequiredTimes.Count) {
			if ((Input.GetMouseButtonUp(0) && RequiredTimes[m_index] <= 0)
			    || (m_time >= RequiredTimes[m_index] && RequiredTimes[m_index] > 0)) {
				Vector2 pos = Input.mousePosition;
				if (Vector2.Distance(pos, Points[m_index]) < AcceptableDist) {
					m_index ++;
//					Debug.Log("DEBUGTAP: m_index = " + m_index);
					if (m_index >= Points.Count) {
						m_index = 0;
						if (Callback != null) Callback();
					}
				}
			}
			else if (Input.GetMouseButtonUp(0)) {
				m_clicking = false;
			}
		}
		m_time += Time.deltaTime;
	}
}
