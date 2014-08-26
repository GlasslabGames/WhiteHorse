using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Listens for the player tapping in certain places around screen. This was chosen as an input that would be easy to use but not to do accidentally, for the Telemetry pop-up.
/// You can give the points out of 1 (so 1,1 is the top right) or give actual coordinates.
/// </summary>
public class TapSequenceListener : MonoBehaviour {
  public System.Action Callback { get; set; }

  public float AcceptableDist; // if < 1, use percent of the screen
  public List<Vector2> Points; //(0, 0) for the bottom left and (1, 1) for the top right
  private int m_index;
  private bool m_clicking;

  void Start() {
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
  }

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
      //m_startTime = Time.time;

    } 
    if (m_clicking && Input.GetMouseButtonUp(0)) {
			Vector2 pos = Input.mousePosition;
			if (Vector2.Distance(pos, Points[m_index]) < AcceptableDist) {
				m_index ++;
				if (m_index >= Points.Count) {
					m_index = 0;
					if (Callback != null) Callback();
				}
			}
      
    }
	}
}
