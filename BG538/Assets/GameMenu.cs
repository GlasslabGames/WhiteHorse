using UnityEngine;
using System.Collections;

public class GameMenu : MonoBehaviour {

	void Start() {
		gameObject.SetActive(false);
	}

	public void Toggle() {
		if (!gameObject.activeSelf) Open();
		else Close();
	}

	public void Open() {
		gameObject.SetActive(true);
	}

	public void Close() {
		gameObject.SetActive(false);
	}
}
