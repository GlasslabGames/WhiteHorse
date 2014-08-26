using UnityEngine;
using System.Collections;

public class DebugSelectButton : MonoBehaviour {

	public DebugDataCollector.DebugMessageType MessageType;
	public GameObject ShowHideObjects = null;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Click ()
	{
		if (enabled)
		{
			DebugDataCollector col = DebugDataCollector.Instance;
			if (col != null)
				col.ChangeDisplay(MessageType);
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
	
	public void SetButtonColor(Color color)
	{
		UILabel label = GetComponent<UILabel>();
		if (label != null)
			label.color = color;
		if (ShowHideObjects != null)
		{
			if (color.Equals(Color.white))
				ShowHideObjects.SetActive(false);
			else ShowHideObjects.SetActive(true);
		}
	}
}
