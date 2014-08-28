using UnityEngine;
using System.Collections;

public class DebugScrollBar : MonoBehaviour {

	/// <summary>
	/// The y value of the position of the object will be used for clamp
	/// </summary>
	public Transform MovingObject;
	public float MaxValue = 1;
	public float MinValue = 0;

	public float Percentage{ get {return m_percent; }}
	private float m_percent;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnPress (bool pressed)
	{
		UpdatePercent();
		//Debug.Log("Press");
	}

	void OnDrag (Vector2 delta)
	{
		UpdatePercent();
		//Debug.Log("Drag");
	}

	void OnScroll (float delta)
	{
		UpdatePercent();
		//Debug.Log("Scroll");
	}

	public void UpdatePercent()
	{
		CheckBorder();
		float value = 0;
		if (MovingObject != null)
			value = MovingObject.localPosition.y;
		value = Mathf.Clamp(value, MinValue, MaxValue);
		if (Mathf.Abs(MinValue - MaxValue) > 1e-3f)
		{
			m_percent = (value - MinValue) / (MaxValue - MinValue);
			m_percent = Mathf.Clamp(m_percent, 0.0f, 1.0f);
		}
		else m_percent = 1.0f;
		//Debug.Log("Scroll: " + m_percent + " y: " + value);
	}

	void CheckBorder()
	{
		if (MaxValue < MinValue)
		{
			float tmp = MinValue;
			MinValue = MaxValue;
			MaxValue = tmp;
		}
	}
}
