using UnityEngine;
using System.Collections;

public class VoteMeter : MonoBehaviour {
	public UITexture[] m_markers;
	public UITexture m_opponentBar; // right anchor
	public UITexture m_playerBar; // left anchor
	public int m_maxWidth;
	public int maxVotes;

	private float m_time = -1;
	private float m_startingPlayerWidth;
	private float m_targetPlayerWidth;
	private float m_startingOpponentWidth;
	private float m_targetOpponentWidth;

	public void Refresh(int playerVotes, int opponentVotes) {
		Debug.Log ("Updating vote meter: "+playerVotes+", "+opponentVotes);

		float playerPercent = (float) playerVotes / maxVotes;
		m_startingPlayerWidth = m_playerBar.width;
		m_targetPlayerWidth = (int) (playerPercent * m_maxWidth);

		float opponentPercent = (float) opponentVotes / maxVotes;
		m_startingOpponentWidth = m_opponentBar.width;
		m_targetOpponentWidth = (int) (opponentPercent * m_maxWidth);

		Debug.Log (m_startingPlayerWidth + ", " + m_targetPlayerWidth);

		m_time = 0;
	}

	void Update() {
		if (m_time > -1) {
			m_time += Time.deltaTime;

			m_playerBar.width = (int) Mathf.Lerp(m_startingPlayerWidth, m_targetPlayerWidth, m_time / GameObjectAccessor.Instance.VoteUpdateTime);
			m_opponentBar.width = (int) Mathf.Lerp(m_startingOpponentWidth, m_targetOpponentWidth, m_time / GameObjectAccessor.Instance.VoteUpdateTime);
			
			m_markers[0].transform.localPosition = new Vector3 (m_playerBar.width, m_markers[0].transform.localPosition.y, 0);
			m_markers[1].transform.localPosition = new Vector3 (m_maxWidth - m_opponentBar.width, m_markers[1].transform.localPosition.y, 0);

			if (m_time >= GameObjectAccessor.Instance.VoteUpdateTime) m_time = -1; // stop lerping
		}
	}
}
