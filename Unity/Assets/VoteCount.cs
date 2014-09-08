using UnityEngine;
using System.Collections;

public class VoteCount : MonoBehaviour {
	private int m_current;
	private int m_target;
	private float m_count;
	private float m_waitTime = 0.1f;
	private UILabel m_label;

	void Awake() {
		m_label = GetComponent<UILabel> ();
	}

	public void Set(int target, bool animate = true) {
		m_target = target;
		m_count = m_waitTime;
	}

	void Update() {
		if (m_current != m_target) {
			m_count -= Time.deltaTime;
			if (m_count <= 0) {
				m_count = m_waitTime;

				if (m_current < m_target) m_current ++;
				else m_current --;

				m_label.text = m_current.ToString();
			}
		}
	}

}
