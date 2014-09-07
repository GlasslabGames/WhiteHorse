using UnityEngine;
using System.Collections;

public class VoteMeter : MonoBehaviour {
	public UITexture[] m_markers;
	public UITexture m_opponentBar; // right anchor
	public UITexture m_playerBar; // left anchor
	public int m_maxWidth;
	public int maxVotes;

	void Start() {
		m_opponentBar.color = (GameObjectAccessor.Instance.Player.m_opponentLeaning == Leaning.Blue) ?
			GameObjectAccessor.Instance.GameColorSettings.blueDarker : GameObjectAccessor.Instance.GameColorSettings.redDarker;

		m_playerBar.color = (GameObjectAccessor.Instance.Player.m_leaning == Leaning.Blue) ?
			GameObjectAccessor.Instance.GameColorSettings.blueDarker : GameObjectAccessor.Instance.GameColorSettings.redDarker;
	}

	public void Refresh(int playerVotes, int opponentVotes) {
		Debug.Log ("Updating vote meter: "+playerVotes+", "+opponentVotes);

		float playerPercent = (float) playerVotes / maxVotes;
		m_playerBar.width = (int) (playerPercent * m_maxWidth);
		m_markers[0].transform.localPosition = new Vector3 (m_playerBar.width, m_markers[0].transform.localPosition.y, 0);

		float opponentPercent = (float) opponentVotes / maxVotes;
		m_opponentBar.width = (int) (opponentPercent * m_maxWidth);
		m_markers[1].transform.localPosition = new Vector3 (m_maxWidth - m_opponentBar.width, m_markers[1].transform.localPosition.y, 0);
	}
}
