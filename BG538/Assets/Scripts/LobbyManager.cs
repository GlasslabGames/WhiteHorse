using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class LobbyManager : MonoBehaviour {
	public GameObject ModePanel;
	private CanvasGroup modeCanvasGroup;
	public GameObject GamePanel;
	private CanvasGroup gameCanvasGroup;
	public GameObject ScenarioPanel;
	private CanvasGroup scenarioCanvasGroup;

	public GameObject WaitingModal;
	public GameObject Overlay;
	
	public Text ScenarioDescription;
	public Image ScenarioImage;

	public Leaning CurrentColor;
	public ScenarioModel CurrentScenarioModel;
	public GameObject ScenarioEntryPrefab;

	public DebugSettings debugSettings;

	/*
	private NetworkManager _networkManager;
	public NetworkManager NetworkManager {
		get {
			if (!_networkManager) _networkManager = FindObjectOfType<NetworkManager>();
			return _networkManager;
		}
	}*/

	private uint hostedMatchId; // UnityEngine.Networking.Types.NetworkID

	// Transition
	private Vector3 panelCenterPos;
	private float panelOffsetY;
	private float panelOffsetX;
	private float transitionDuration = 0.5f;
	private Ease transitionEase = Ease.InOutCubic;

	public void Start() {
		FillInScenarios();

		// Clear whatever options we had set before
		GameSettings.InstanceOrCreate.CurrentOptions.Clear();

		panelCenterPos = GamePanel.transform.position;
		panelOffsetY = Screen.height * 1.25f;
		panelOffsetX = Screen.width * 1.25f;

		ModePanel.SetActive(true);
		modeCanvasGroup = ModePanel.GetComponent<CanvasGroup>();

		GamePanel.SetActive(true);
		gameCanvasGroup = GamePanel.GetComponent<CanvasGroup>();
		gameCanvasGroup.alpha = 0;
		gameCanvasGroup.interactable = false;

		ScenarioPanel.SetActive(true);
		scenarioCanvasGroup = ScenarioPanel.GetComponent<CanvasGroup>();
		scenarioCanvasGroup.alpha = 0;
		scenarioCanvasGroup.interactable = false;

		WaitingModal.SetActive(false);
		Overlay.SetActive(false);
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
			ScrollPanel(modeCanvasGroup, "right", true);
			ScrollPanel(scenarioCanvasGroup, "right", false);
		}
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
		canvasGroup.alpha = 1;
		Tweener t = canvasGroup.transform.DOMove(endPos, transitionDuration).SetEase(transitionEase);
		if (entering) {
			t.OnComplete(() => canvasGroup.interactable = true );
		} else {
			t.OnComplete(() => { canvasGroup.alpha = 0; canvasGroup.interactable = false; });
		}
	}

	public void ToggleColor() {
		CurrentColor = (CurrentColor == Leaning.Blue)? Leaning.Red : Leaning.Blue;
		ColorSwapperBase[] colorSwappers = ScenarioPanel.GetComponentsInChildren<ColorSwapperBase>();
		foreach (ColorSwapperBase colorSwapper in colorSwappers) {
			colorSwapper.SetColor(CurrentColor);
		}
	}

	public void Host() {
		Overlay.SetActive(true); // TODO: make sure we have a way to get out of this state if there's an error

		// check for custom settings
		int duration = GameSettings.InstanceOrCreate.TotalWeeks;
		float increment = GameSettings.InstanceOrCreate.WorkerIncrement;
		if (debugSettings != null) {
			duration = Mathf.RoundToInt(debugSettings.weekSlider.value);
			increment = debugSettings.workerSlider.value;
		}

		// Store info in gameSettings
		Dictionary<string, object> options = GameSettings.InstanceOrCreate.CurrentOptions;
		options["scenarioId"] = CurrentScenarioModel.Id;
		options["color"] = (int) CurrentColor;
		options["duration"] = duration;
		options["increment"] = increment;
		options["name"] = PhotonNetwork.playerName;

		Debug.Log (">> "+GameSettings.InstanceOrCreate.CurrentOptions["scenarioId"]);

		if (NetworkManager.MultiplayerMode) {
			NetworkManager.CreateRoom(options);
			// Photon will automatically join the room once it's created, and then we'll start the game
		} else {
			// Join an offline room
			PhotonNetwork.offlineMode = true;
			PhotonNetwork.JoinRoom("offline");
		}
	}

	public void Join() {
		ToggleGroup group = GamePanel.GetComponentInChildren<ToggleGroup>();
		foreach (Toggle toggle in group.ActiveToggles()) { // I couldn't find a better way to access the single toggled
			MatchmakerEntry entry = toggle.GetComponent<MatchmakerEntry>();
			if (entry) {
				NetworkManager.JoinRoom(entry.RoomName);
				return;
			}
		}
	}

	public void CancelMatch() {
		Debug.Log (hostedMatchId);
		//TODO MatchMaker.DestroyMatch((UnityEngine.Networking.Types.NetworkID) hostedMatchId, OnMatchDestroyed);
		WaitingModal.SetActive(false);
	}

	public void Logout() {
		Overlay.SetActive(true);
		SdkManager.Instance.Logout();
		Application.LoadLevel("title");
	}
}
