using UnityEngine;
using System.Collections;

public class DebugLabelControl : MonoBehaviour {

	public UIScrollView ScrollView;
	public float LoadLinesThreshold = 100.0f;

	public static int[] FONT_GROUP = new int[]{
		10,11,12,14,16,18,20,22,24,26,28,32,36,40,48,56,64,72,80,92,104,116,128,144
	};

	private int m_currentSize = -1;

	void Awake() {
		GetFontSize();
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnPress (bool pressed)
	{
		//Debug.Log("Press");
		if (pressed == false)
		{
			if (ScrollView != null)
			{
				Vector2 constrains = ScrollView.panel.CalculateConstrainOffset(ScrollView.bounds.min, ScrollView.bounds.max);
				if (Mathf.Abs(constrains.y) >= LoadLinesThreshold)
					if (DebugDataCollector.Instance != null)
						DebugDataCollector.Instance.LoadMoreLines(constrains.y > 0);
				//Debug.Log(constrains);
			}
		}
	}

	public void ResetScrollViewPosition() {
		if (ScrollView != null && ScrollView.gameObject.activeInHierarchy)
			ScrollView.ResetPosition();
	}

	public void IncreaseFontSize() {
		if (m_currentSize < 0)
			GetFontSize();
		if (m_currentSize >= 0 && m_currentSize + 1 < FONT_GROUP.Length)
		{
			m_currentSize++;
			SetFontSize();
		}
	}

	public void DecreaseFontSize() {
		if (m_currentSize < 0)
			GetFontSize();
		if (m_currentSize > 0)
		{
			m_currentSize--;
			SetFontSize();
		}
	}

	private void GetFontSize() {
		UILabel label = GetComponent<UILabel>();
		if (label != null) {
			int fontSize = label.fontSize;
			for (int i = FONT_GROUP.Length - 1; i >= 0; i--)
			{
				if (fontSize >= FONT_GROUP[i])
				{
					m_currentSize = i;
					break;
				}
			}
			if (m_currentSize == -1)
				m_currentSize = 0;
		}
		else m_currentSize = -1;
	}

	private void SetFontSize() {
		if (m_currentSize >= 0)
		{
			UILabel label = GetComponent<UILabel>();
			if (label != null) {
				label.fontSize = FONT_GROUP[m_currentSize];
				AdjustColliderSize();
			}
		}
	}

	public void AdjustColliderSize()
	{
		UILabel label = GetComponent<UILabel>();
		Collider c = GetComponent<Collider>();
		if (label != null && c != null)
		{
			if (label.autoResizeBoxCollider)
			{
				BoxCollider box = c as BoxCollider;
				if (box != null)
					NGUITools.UpdateWidgetCollider(box, true);
			}
		}
	}
}
