using UnityEngine;
using System.Collections;

public class TitleManager : MonoBehaviour {

	public GameObject overlay;
	public GameObject credits;
	public GameObject webview;
	
	void Start() {
		overlay.SetActive(false);
		LoginPanelWebView webviewPanel = webview.GetComponent<LoginPanelWebView> ();
		webviewPanel.onLoginComplete += OnLoginComplete;
	}

	public void ShowLogin() {
		#if (UNITY_IOS && !UNITY_EDITOR) || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		overlay.SetActive(true);
		webview.SetActive(true);
		#else
		OnLoginComplete();
		#endif
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

	void OnLoginComplete() {
		// webview is closed, now just do the scene transition
		Application.LoadLevel("lobby"); // proceed to the lobby
	}
}
