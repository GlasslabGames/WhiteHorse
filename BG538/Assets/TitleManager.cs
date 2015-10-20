using UnityEngine;
using System.Collections;

public class TitleManager : MonoBehaviour {

	public GameObject overlay;
	public GameObject credits;
	public GameObject webview;

	void Start() {
		overlay.SetActive(false);
	}

	public void ShowLogin() {
		overlay.SetActive(true);
		webview.SetActive(true);
	}

	public void HideWindows() {
		overlay.SetActive(false);
		overlay.SetActive(false);
		credits.SetActive(false);
		webview.SetActive (false);
	}

	public void ShowCredits() {
		overlay.SetActive(true);
		credits.SetActive(true);
	}
}
