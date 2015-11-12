using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ExitGames.Client.Photon;

public class PhotonErrorPopup : SingletonBehavior<PhotonErrorPopup> {
	public Text infoLabel;
	public LobbyManager lobbyManager;
	public GameObject okButton;
	public GameObject retryButton;
	public GameObject quitButton;

	[HideInInspector]
	public GameObject lobbyOverlay;

	public void ShowError(string info, object[] error) {
		short errorCode = (short) error[0];
		if (errorCode == ErrorCode.MaxCcuReached) ShowCcuError();
		else Show(info + (string) error[1], false);
	}

	public void ShowConnectionError(DisconnectCause cause) {
		if (cause == DisconnectCause.DisconnectByServerUserLimit || cause == DisconnectCause.MaxCcuReached) {
			ShowCcuError();
		} else {
		    Show("Couldn't connect to the game server. Error: " + System.Enum.GetName(typeof(DisconnectCause), cause), true);
		}
	}

	public void ShowCcuError() {
		Show("Unfortunately, the game server can't accept any more players right now. Please try again later.", true);
	}

	public void Show(string info, bool canRetry) {
		if (lobbyOverlay != null) lobbyOverlay.SetActive (false);

		gameObject.SetActive(true);
		infoLabel.text = info;

		//quitButton.SetActive(canRetry);
		retryButton.SetActive(canRetry);
		okButton.SetActive(!canRetry);
	}

	public void Retry() {
		Hide();
		NetworkManager.Connect();
	}

	public void Quit() {
		Hide();
		lobbyManager.ScrollToModePanel();
	}

	public void Hide() {
		gameObject.SetActive(false);
	}
}
