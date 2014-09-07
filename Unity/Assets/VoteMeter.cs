using UnityEngine;
using System.Collections;

public class VoteMeter : MonoBehaviour {
	public UITexture[] m_markers;
	public UITexture m_redBar; // right anchor
	public UITexture m_blueBar; // left anchor
	public int m_maxWidth;
	public int maxVotes;

	void Start() {
		Refresh (50, 50);
	}

	public void Refresh(int blueVotes, int redVotes) {

	}
}
