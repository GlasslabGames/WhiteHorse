using UnityEngine;
using System.Collections;

public class VoteCount : MonoBehaviour {
	private int m_current;
	private int m_start;
	private int m_target;
	private float m_time = -1;
	private UILabel m_label;

	void Awake() {
		m_label = GetComponent<UILabel> ();
	}

	public void Set(int target, bool animate = true) {
		m_target = target;
		if (animate) {
			m_start = m_current;
			m_target = target;
			m_time = 0;
		} else {
			m_current = target;
			m_label.text = m_current.ToString();
		}
	}

	
	void Update() {
		if (m_time > -1) {
			m_time += Time.deltaTime;
			
			m_current = (int) Mathf.Lerp(m_start, m_target, m_time / GameObjectAccessor.Instance.VoteUpdateTime);
			m_label.text = m_current.ToString();

			if (m_time >= GameObjectAccessor.Instance.VoteUpdateTime) m_time = -1; // stop lerping
		}
	}
}
