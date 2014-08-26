using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugBackgroundControl : MonoBehaviour {

	public List<DebugScrollBar> ScrollBars;
	public List<ScrollBarControlValue> ValueNames;

	public enum ScrollBarControlValue{
		Opacity = 0,
	};

	private float[] m_lastPercents;

	void Awake() {
		m_lastPercents = new float[ScrollBars.Count];
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < m_lastPercents.Length; i++)
		{
			if (i >= ScrollBars.Count) break;
			if (m_lastPercents[i] != ScrollBars[i].Percentage)
			{
				m_lastPercents[i] = ScrollBars[i].Percentage;
				if (i < ValueNames.Count)
					UpdateValue(m_lastPercents[i], ValueNames[i]);
			}
		}
	}

	void UpdateValue(float percent, ScrollBarControlValue type)
	{
		if (type == ScrollBarControlValue.Opacity)
		{
			AdjustOpacity(percent);
		}
	}

	void AdjustOpacity(float percent)
	{

		UISprite sprite = GetComponent<UISprite>();
		if (sprite != null)
		{
			percent = Mathf.Clamp(percent, 0.0f, 1.0f);
			Color color = sprite.color;
			color.a = percent;
			sprite.color = color;
		}
	}
}
