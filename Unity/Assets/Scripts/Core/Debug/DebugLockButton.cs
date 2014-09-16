using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GlassLab.Core.Serialization;

public class DebugLockButton : MonoBehaviour {

	public LockType Type;
	public bool isLockAtFirst = false;

	public enum LockType{
		Interaction = 0,
		UpdateMessage,
		IgnoreSave,
	};

	private UILabel label;
	private bool isEnable = false;

  void Awake() {
		label = GetComponent<UILabel>();
	}

	// Use this for initialization
	void Start () {
		if (isLockAtFirst)
			TurnLock();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void Click ()
	{
		if (enabled) {
			TurnLock();
		}
	}

	public void TurnLock()
	{
		//bool isEnable = false;
		switch (Type)
		{
		case LockType.Interaction:
			isEnable = DebugSystemManager.Instance.SwitchInteraction();
      DebugSystemManager.Instance.UpdateAllSaveSlots();
			break;
		case LockType.UpdateMessage:
			isEnable = DebugDataCollector.Instance.SwitchUpdateMessage();
			break;
		case LockType.IgnoreSave:
			if (SessionManager.Instance != null)
				isEnable = SessionManager.Instance.ContextMenuToggleSaves();
			else isEnable = false;
			break;
		default:
			break;
		}
		UpdateColor();
	}

	public void SyncLock()
	{
		switch (Type)
		{
		case LockType.Interaction:
			isEnable = DebugSystemManager.Instance.SyncInteraction();
			break;
		case LockType.UpdateMessage:
			isEnable = DebugDataCollector.Instance.SyncUpdateMessage();
			break;
		case LockType.IgnoreSave:
			if (SessionManager.Instance != null)
				isEnable = SessionManager.Instance.SyncContextMenuToggleSaves();
			else isEnable = false;
			break;
		default:
			break;
		}
		UpdateColor();
	}

	void UpdateColor()
	{
		if (label == null) label = GetComponent<UILabel>();
		if (label != null)
		{
			//label.color = new Color(255, 255 * (isEnable ? 0 : 1), 255 * (isEnable ? 0 : 1));
			label.color = isEnable ? Color.red : Color.white;
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

	public void TurnOff()
	{
		if (isEnable)
			Click();
	}
}
