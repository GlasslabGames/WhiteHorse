using UnityEngine;
using System.Collections;

public class OpinionMeter : MonoBehaviour {
	public UITexture m_marker;
	public UITexture m_playerBar; // left
	public UITexture m_opponentBar; // bg
	public int m_maxWidth;

	public void Refresh (float popularVote) {
		Debug.Log ("Updating opinion meter: "+popularVote);
		popularVote = popularVote / 2f + 0.5f; // blueness, 0 to 1

		// if the player's not blue, use the inverse instead
		if (GameObjectAccessor.Instance.Player.m_leaning != Leaning.Blue) popularVote = 1 - popularVote;

		m_playerBar.width = (int) (m_maxWidth * popularVote);
		m_marker.transform.localPosition = new Vector3 (m_playerBar.width, m_marker.transform.localPosition.y, 0);
	}
}
