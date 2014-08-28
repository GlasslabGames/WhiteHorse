using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugOpenButton : MonoBehaviour {

	public List<EventDelegate> onClick = new List<EventDelegate>();
	private UILabel label;

  void Awake() {
		label = GetComponent<UILabel>();
	}

	// Use this for initialization
	void Start () {
		UpdateText();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Click ()
	{
		if (enabled)
		{
			EventDelegate.Execute(onClick);
			UpdateText();
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

	void UpdateText()
	{
		if (label != null)
		{
			if (DebugSystemManager.Instance.DebugWindow != null)
			{
				bool isShowing = DebugSystemManager.Instance.DebugWindow.activeInHierarchy;
				if (isShowing) label.text = "CLOSE";
				else label.text = "OPEN";
			}
		}
	}

	public void Close()
	{
		if (label != null)
		{
			if (label.text.Equals("CLOSE"))
				Click();
		}
	}
}
