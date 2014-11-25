using UnityEngine;
using System.Collections;

public class OpinionMeter : MonoBehaviour {
	public UITexture m_marker;
	public UITexture m_playerBar; // left
	public UITexture m_opponentBar; // bg
	public int m_maxWidth;
	
	private float m_time = -1;
	private float m_startingPlayerWidth;
	private float m_targetPlayerWidth;

	public void Refresh (float popularVote) {
		popularVote = popularVote / 2f + 0.5f; // blueness, 0 to 1

		// if the player's not blue, use the inverse instead
		if (GameObjectAccessor.Instance.Player.m_leaning != Leaning.Blue) popularVote = 1 - popularVote;

		Set (popularVote);
	}

	public void Set (float amount, bool lerp = true) {
		m_targetPlayerWidth = (int)(m_maxWidth * amount);
		if (lerp) {
			m_startingPlayerWidth = m_playerBar.width;
			m_time = 0;
		} else {
			m_playerBar.width = (int) m_targetPlayerWidth;
		}
	}

	void Update() {
		if (m_time > -1) {
			m_time += Time.deltaTime;
			
			m_playerBar.width = (int) Mathf.Lerp(m_startingPlayerWidth, m_targetPlayerWidth, m_time / GameObjectAccessor.Instance.VoteUpdateTime);
			if (m_marker != null) {
				m_marker.transform.localPosition = new Vector3 (m_playerBar.width, m_marker.transform.localPosition.y, 0);
			}
			
			if (m_time >= GameObjectAccessor.Instance.VoteUpdateTime) m_time = -1; // stop lerping
		}
	}
}
