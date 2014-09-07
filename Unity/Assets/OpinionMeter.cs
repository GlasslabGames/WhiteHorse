using UnityEngine;
using System.Collections;

public class OpinionMeter : MonoBehaviour {
	public UITexture m_marker;
	public UITexture m_blueBar;
	public int m_maxWidth;

	void Start() {}

	public void Refresh (float popularVote) {
		Debug.Log ("Updating opinion meter: "+popularVote);
		popularVote = popularVote / 2f + 0.5f; // 0 to 1
		m_blueBar.width = (int) (m_maxWidth * popularVote);
		m_marker.transform.localPosition = new Vector3 (m_blueBar.width, m_marker.transform.localPosition.y, 0);
	}
}
