using UnityEngine;
using System.Collections;

public class VoteCount : MonoBehaviour {
	private int m_current;
	private int m_target;
	//private float m_time;
	//private float m_waitTime;
	private UILabel m_label;

	void Awake() {
		m_label = GetComponent<UILabel> ();
	}

	public void Set(int target) {
		m_target = target;
	}

	void Update() {
		if (m_current != m_target) {
			if (m_current < m_target) m_current ++;
			else m_current --;

			m_label = m_target;
		}
	}

}
