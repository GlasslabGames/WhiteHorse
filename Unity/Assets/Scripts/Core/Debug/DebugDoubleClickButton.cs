using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugDoubleClickButton : MonoBehaviour {

	public List<EventDelegate> onClick = new List<EventDelegate>();
	public string OriginText;
	public string SetText;
	public float WaitTime = 2.0f;
	private int m_state = 0;
	private float m_currentTime = 0f;
	private UILabel label;

	void Awake()
	{
		label = GetComponent<UILabel>();
	}

	void Start()
	{
		if (label != null)
			label.text = OriginText;
	}

	void Update()
	{
		m_currentTime += Time.deltaTime;
		if (m_currentTime > WaitTime && m_state != 0)
		{
			m_state = 0;
			if (label != null)
				label.text = OriginText;
		}
	}
	
	public void Click ()
	{
		if (enabled)
		{
			m_state++;
			if (m_state == 1)
			{
				if (label != null)
					label.text = SetText;
				m_currentTime = 0;
			}
			else if (m_state > 1)
			{
				if (label != null)
					label.text = OriginText;
				m_state = 0;
				EventDelegate.Execute(onClick);
			}
		}
	}
	
	void OnClick () // NGUI
	{
		Click ();
	}
	
	void OnMouseUpAsButton () // non-NGUI
	{
		Click ();
	}
}
