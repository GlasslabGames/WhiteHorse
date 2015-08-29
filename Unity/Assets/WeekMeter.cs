using UnityEngine;
using System.Collections;

public class WeekMeter : MonoBehaviour {
	public UITexture m_fill;
	public UIGrid m_rulerGrid;
	public float m_rulerWidth;
	public float m_maxFillWidth;

	private int m_totalTurns;
	
	private float m_time = -1;
	private float m_startingWidth;
	private float m_targetWidth;

	void Awake () {
		m_totalTurns = GameObjectAccessor.Instance.GameStateManager.TotalWeeks;
		GameObject copy = m_rulerGrid.transform.GetChild (0).gameObject;
		while (m_rulerGrid.transform.childCount < m_totalTurns) {
			Utility.InstantiateAsChild(copy, m_rulerGrid.transform);
		}
		m_rulerGrid.cellWidth = m_rulerWidth / m_totalTurns;
		m_rulerGrid.Reposition ();
		m_rulerGrid.GetChild (0).gameObject.SetActive (false); // hide the first one (on the left edge)

		Refresh (0);
	}

	public void Refresh(int weeks) {
		float beginning = m_maxFillWidth - m_rulerWidth;
		m_startingWidth = m_fill.width;
		m_targetWidth = (int) ((m_rulerWidth / m_totalTurns) * weeks + beginning);
		m_time = 0;
	}

	void Update() {
		if (m_time > -1) {
			m_time += Time.deltaTime;

			m_fill.gameObject.SetActive(m_targetWidth <= 0); // since the min displayable width is 2, we need to hide it when it should be 0
			m_fill.width = (int) Mathf.Lerp(m_startingWidth, m_targetWidth, m_time / GameObjectAccessor.Instance.VoteUpdateTime);
			
			if (m_time >= GameObjectAccessor.Instance.VoteUpdateTime) m_time = -1; // stop lerping
		}
	}
}
