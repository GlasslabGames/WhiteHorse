using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using ExitGames.Client.Photon;

public class LobbyManager : MonoBehaviour {
	public GameObject ModePanel;
	private CanvasGroup modeCanvasGroup;
	public GameObject GamePanel;
	private CanvasGroup gameCanvasGroup;
	public GameObject ScenarioPanel;
	private CanvasGroup scenarioCanvasGroup;

	public RoomList roomList;

	public GameObject WaitingModal;
	public GameObject Overlay;
	public PhotonErrorPopup photonConnectionModal;
	
	public Text ScenarioDescription;
	public Image ScenarioImage;

	public Toggle GroupToggle;
	public GameObject GroupModal;
	public Text GroupLabel;
	public InputField GroupKeywordEntry;
	private bool preventGroupToggleCallback;

	public Leaning CurrentColor;
	public ScenarioModel CurrentScenarioModel;

	public GameObject ScenarioEntryPrefab;

	public DebugSettings debugSettings;

	public static LobbyManager Instance;
	public bool ShowingModePanel {
		get {
			return modeCanvasGroup.interactable;
		}
	}

	// Transition
	private Vector3 panelCenterPos;
	private float panelOffsetY;
	private float panelOffsetX;
	private float transitionDuration = 0.5f;
	private Ease transitionEase = Ease.InOutCubic;

	public void Start() {
		Instance = this;

		panelCenterPos = GamePanel.transform.position;
		panelOffsetY = Screen.height * 1.25f;
		panelOffsetX = Screen.width * 1.25f;

		ModePanel.SetActive(true);
		modeCanvasGroup = ModePanel.GetComponent<CanvasGroup>();
		NetworkManager.EndMultiplayer(); // because we start with the mode panel showing

		GamePanel.SetActive(true);
		gameCanvasGroup = GamePanel.GetComponent<CanvasGroup>();
		TogglePanel(gameCanvasGroup, false);

		ScenarioPanel.SetActive(true);
		scenarioCanvasGroup = ScenarioPanel.GetComponent<CanvasGroup>();
		TogglePanel(scenarioCanvasGroup, false);

		WaitingModal.SetActive(false);
		Overlay.SetActive(false);

		photonConnectionModal.lobbyOverlay = Overlay;

		RefreshGroupToggle();
		FillInScenarios();
		if (Random.value >= 0.5) ToggleColor(); // randomly choose whether to start blue (default) or red
	}

	void FillInScenarios() {
		ToggleGroup parent = ScenarioPanel.GetComponentInChildren<ToggleGroup>();
		GameObject go;
		ScenarioEntry entry;
		bool defaultSet = false;
		foreach (ScenarioModel model in ScenarioModel.Models) {
			go = Object.Instantiate(ScenarioEntryPrefab) as GameObject;
			go.transform.SetParent(parent.transform, false);

			entry = go.GetComponent<ScenarioEntry>();
			entry.Set(model);
			entry.OnSelected += OnScenarioEntrySelected;

			if (!defaultSet) {
				entry.GetComponent<Toggle>().isOn = true; // if it was already on (thanks to PrefabEntry), this doesn't call OnSelected..
				OnScenarioEntrySelected(model); // so call it directly

				defaultSet = true;
			}
		}

		LayoutRebuilder.MarkLayoutForRebuild(parent.transform as RectTransform);
	}

	public void OnScenarioEntrySelected(ScenarioModel scenario) {
		CurrentScenarioModel = scenario;
		ScenarioDescription.text = scenario.Description;
		ScenarioImage.sprite = Resources.Load<Sprite>(scenario.Image);
	}

	public void StartMultiplayer() {
		NetworkManager.StartMultiplayer();
		ScrollPanel(modeCanvasGroup, "right", false);
		ScrollPanel(gameCanvasGroup, "right", true);
	}

	public void StartSinglePlayer() {
		ScrollPanel(modeCanvasGroup, "left", false);
		ScrollPanel(scenarioCanvasGroup, "left", true);
	}

	public void LeaveScenarioPanel() {
		if (NetworkManager.MultiplayerMode) { // return to the game panel
			ScrollPanel(scenarioCanvasGroup, "up", false);
			ScrollPanel(gameCanvasGroup, "up", true);
		} else { // return to the mode panel
			ScrollToModePanel();
		}
	}

	public void ScrollToModePanel() {
		string dir = (NetworkManager.MultiplayerMode)? "left" : "right";
		ScrollPanel(modeCanvasGroup, dir, true);

		if (scenarioCanvasGroup.interactable) ScrollPanel(scenarioCanvasGroup, dir, false);
		if (gameCanvasGroup.interactable) ScrollPanel(gameCanvasGroup, dir, false);

		NetworkManager.EndMultiplayer();
	}
	
	public void GameToScenarioPanel() {
		ScrollPanel(scenarioCanvasGroup, "down", true);
		ScrollPanel(gameCanvasGroup, "down", false);
	}

	public void ScrollPanel(CanvasGroup canvasGroup, string direction, bool entering) {
		Vector3 startPos = panelCenterPos;
		Vector3 endPos = panelCenterPos;
		switch (direction) {
		case "left":
			if (entering) startPos.x += panelOffsetX;
			else endPos.x -= panelOffsetX;
			break;
		case "right":
			if (entering) startPos.x -= panelOffsetX;
			else endPos.x += panelOffsetX;
			break;
		case "up":
			if (entering) startPos.y += panelOffsetY;
			else endPos.y -= panelOffsetY;
			break;
		case "down":
			if (entering) startPos.y -= panelOffsetY;
			else endPos.y += panelOffsetX;
			break;
		}

		canvasGroup.transform.position = startPos;
		TogglePanel(canvasGroup, true);
		Tweener t = canvasGroup.transform.DOMove(endPos, transitionDuration).SetEase(transitionEase);
		if (!entering) {
			t.OnComplete(() => TogglePanel(canvasGroup, false));

			SoundController.Play("PanelSwipe");
		}
	}

	public void TogglePanel(CanvasGroup canvasGroup, bool on) {
		canvasGroup.alpha = (on)? 1 : 0;
		canvasGroup.interactable = on;
		canvasGroup.blocksRaycasts = on;
	}

	public void ToggleColor() {
		CurrentColor = (CurrentColor == Leaning.Blue)? Leaning.Red : Leaning.Blue;
		ColorSwapperBase[] colorSwappers = ScenarioPanel.GetComponentsInChildren<ColorSwapperBase>();
		foreach (ColorSwapperBase colorSwapper in colorSwappers) {
			colorSwapper.SetColor(CurrentColor);
		}
	}

	public void ToggleGroup(bool on) {
		if (preventGroupToggleCallback) return;

		if (on) {
			GroupKeywordEntry.text = "";
			GroupModal.SetActive (true);
		} else {
			SetGroupKeyword("");
		}
	}

	public void CancelGroup() {
		SetGroupKeyword("");
		GroupModal.SetActive (false);
	}

	public void SubmitGroupKeyword() {
		SetGroupKeyword(GroupKeywordEntry.text);
		GroupModal.SetActive (false);
	}

	private void SetGroupKeyword(string keyword) {
		NetworkManager.Instance.GroupKeyword = keyword.ToUpper();
		if (SignalManager.RoomGroupChanged != null) SignalManager.RoomGroupChanged ();
		RefreshGroupToggle ();
	}

	private void RefreshGroupToggle() {
		preventGroupToggleCallback = true; // hacks because setting isOn calls the callback
		if (NetworkManager.Instance.GroupKeyword.Length > 0) {
			GroupToggle.isOn = true;
			GroupLabel.text = "Class keyword: " + NetworkManager.Instance.GroupKeyword;
		} else {
			GroupToggle.isOn = false;
			GroupLabel.text = "Show classmates only";
		}
		preventGroupToggleCallback = false;
	}

	public void Host() {
		GameSettings settings = GameSettings.InstanceOrCreate;
		settings.currentScenarioId = CurrentScenarioModel.Id;
		settings.currentColor = CurrentColor;
		settings.currentDuration = (debugSettings != null)? Mathf.RoundToInt(debugSettings.weekSlider.value) : 0;
		settings.currentIncrement = (debugSettings != null)? debugSettings.workerSlider.value : 0;

		Overlay.SetActive(true);

		if (NetworkManager.MultiplayerMode) {
			NetworkManager.CreateRoom(PhotonNetwork.playerName, settings);
			// Photon will automatically join the room once it's created, and then we'll start the game
		} else {
			// Start the game without Photon
			NetworkManager.StartOfflineGame();
		}

	}

	public void Join() {
		ToggleGroup group = GamePanel.GetComponentInChildren<ToggleGroup>();
		foreach (Toggle toggle in group.ActiveToggles()) { // I couldn't find a better way to access the single toggled
			MatchmakerEntry entry = toggle.GetComponent<MatchmakerEntry>();
			if (entry) {
				Overlay.SetActive(true);

				NetworkManager.JoinRoom(entry.RoomName);
				return;
			}
		}
	}

	public void CancelMatch() {
		//TODO MatchMaker.DestroyMatch((UnityEngine.Networking.Types.NetworkID) hostedMatchId, OnMatchDestroyed);
		WaitingModal.SetActive(false);
	}

	public void Logout() {
		Overlay.SetActive(true);
		SdkManager.Instance.Logout();
		Application.LoadLevel("title");
	}
}
