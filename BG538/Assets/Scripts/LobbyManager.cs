using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class LobbyManager : MonoBehaviour {
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
	private float panelStartY;
	private float panelOffsetY;
	private float transitionDuration = 0.5f;
	private Ease transitionEase = Ease.InOutCubic;

	public static char MatchInfoDivider = '|';

	public void Start() {
		FillInScenarios();

		panelStartY = GamePanel.transform.position.y;
		panelOffsetY = Screen.height * 1.25f;
		
		GamePanel.SetActive(true);
		gameCanvasGroup = GamePanel.GetComponent<CanvasGroup>();

		ScenarioPanel.SetActive(true);
		scenarioCanvasGroup = ScenarioPanel.GetComponent<CanvasGroup>();
		scenarioCanvasGroup.alpha = 0;

		Vector3 pos = ScenarioPanel.transform.position;
		pos.y = panelStartY - panelOffsetY;
		ScenarioPanel.transform.position = pos;

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

	public void ScrollToGamePanel() {
		gameCanvasGroup.alpha = 1;
		GamePanel.transform.DOMoveY(panelStartY, transitionDuration).SetEase(transitionEase);
		ScenarioPanel.transform.DOMoveY(panelStartY - panelOffsetY, transitionDuration)
			.SetEase(transitionEase)
				.OnComplete(() => scenarioCanvasGroup.alpha = 0);
	}

	public void ScrollToScenarioPanel() {
		scenarioCanvasGroup.alpha = 1;
		ScenarioPanel.transform.DOMoveY(panelStartY, transitionDuration).SetEase(transitionEase);
		GamePanel.transform.DOMoveY(panelStartY + panelOffsetY, transitionDuration)
			.SetEase(transitionEase)
				.OnComplete(() => gameCanvasGroup.alpha = 0);
	}

	public void ToggleColor() {
		CurrentColor = (CurrentColor == Leaning.Blue)? Leaning.Red : Leaning.Blue;
		ColorSwapperBase[] colorSwappers = ScenarioPanel.GetComponentsInChildren<ColorSwapperBase>();
		foreach (ColorSwapperBase colorSwapper in colorSwappers) {
			colorSwapper.SetColor(CurrentColor);
		}
	}

	public void Host() {
		string playerName = "Jerry F."; // TODO: get player name

		NetworkManager.CreateRoom(playerName, CurrentScenarioModel.Id, (int) CurrentColor);
		// Photon will automatically join the room once it's created, and then we'll start the game

		Overlay.SetActive(true); // TODO: make sure we have a way to get out of this state if there's an error
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
}
