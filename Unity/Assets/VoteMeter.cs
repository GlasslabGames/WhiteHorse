using UnityEngine;
using System.Collections;

public class VoteMeter : MonoBehaviour {
	public UITexture[] m_markers;
	public UITexture m_redBar; // right anchor
	public UITexture m_blueBar; // left anchor
	public int m_maxWidth;
	public int maxVotes;

	void Start() {}

	public void Refresh(int blueVotes, int redVotes) {
		Debug.Log ("Updating vote meter: "+blueVotes+", "+redVotes);

		float bluePercent = (float) blueVotes / maxVotes;
		m_blueBar.width = (int) (bluePercent * m_maxWidth);
		m_markers[0].transform.localPosition = new Vector3 (m_blueBar.width, m_markers[0].transform.localPosition.y, 0);

		float redPercent = (float) redVotes / maxVotes;
		m_redBar.width = (int) (redPercent * m_maxWidth);
		m_markers[1].transform.localPosition = new Vector3 (m_maxWidth - m_redBar.width, m_markers[1].transform.localPosition.y, 0);
	}
}
