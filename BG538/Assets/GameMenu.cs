using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameMenu : MonoBehaviour {
	public Text SoundToggleText;
	public Text MusicToggleText;
	public GameObject CancelButton;
	public bool recordTelemetry;

	public void Toggle() {
		if (!gameObject.activeSelf) Open();
		else Close();
	}

	public void Open() {
		if (!gameObject.activeSelf && recordTelemetry) SdkManager.Instance.SaveTelemEvent("open_game_menu", SdkManager.EventCategory.Player_Action);

		gameObject.SetActive(true);

		if (CancelButton != null) {
			CancelButton.SetActive (LobbyManager.Instance != null && !LobbyManager.Instance.ShowingModePanel);
			RectTransform child = transform.GetChild (0) as RectTransform;
			child.sizeDelta = new Vector2 (child.sizeDelta.x, (CancelButton.activeSelf) ? 690 : 580);
		}

		RefreshAudioButtons();
	}

	public void Close() {
		gameObject.SetActive(false);
	}

	void RefreshAudioButtons() {
		SoundToggleText.text = "TURN SFX " + (SoundController.SoundOn? "OFF" : "ON");
		MusicToggleText.text = "TURN MUSIC " + (SoundController.MusicOn? "OFF" : "ON");
	}

	public void ToggleSound() {
		SoundController.ToggleSound();
		RefreshAudioButtons();
	}

	public void ToggleMusic() {
		SoundController.ToggleMusic();
		RefreshAudioButtons();
	}
}
