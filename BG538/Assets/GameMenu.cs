using UnityEngine;
using System.Collections;

public class GameMenu : MonoBehaviour {

	void Awake() {
		gameObject.SetActive(false);
	}

	public void Toggle() {
		if (!gameObject.activeSelf) Open();
		else Close();
	}

	public void Open() {
		gameObject.SetActive(true);
		//TODO: Debug.Log("Open menu? " + gameObject.activeSelf + ", " + gameObject.activeInHierarchy, gameObject);

		SdkManager.Instance.SaveTelemEvent("open_game_menu", SdkManager.EventCategory.Player_Action);
	}

	public void Close() {
		gameObject.SetActive(false);
	}
}
