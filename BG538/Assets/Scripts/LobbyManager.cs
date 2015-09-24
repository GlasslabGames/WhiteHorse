using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System.Collections.Generic;
using DG.Tweening;

public class LobbyManager : MonoBehaviour {
	public GameObject GamePanel;
	public GameObject ScenarioPanel;
	public GameObject WaitingModal;
	public GameObject Overlay;
	private MatchList matchList;
	
	public Text ScenarioDescription;
	public Image ScenarioImage;

	public Leaning CurrentColor;
	public ScenarioModel CurrentScenarioModel;
	public GameObject ScenarioEntryPrefab;

	private NetworkLobbyManager _networkManager;
	public NetworkLobbyManager NetworkManager {
		get {
			if (!_networkManager) _networkManager = FindObjectOfType<NetworkLobbyManager>();
			return _networkManager;
		}
	}
	public NetworkMatch MatchMaker {
		get {
			if (!NetworkManager.matchMaker) NetworkManager.StartMatchMaker();
			return NetworkManager.matchMaker;
		}
	}

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
		ScenarioPanel.SetActive(false);

		Vector3 pos = ScenarioPanel.transform.position;
		pos.y = panelStartY - panelOffsetY;
		ScenarioPanel.transform.position = pos;

		WaitingModal.SetActive(false);
		Overlay.SetActive(false);

		matchList = GamePanel.GetComponentInChildren<MatchList>();
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
		GamePanel.SetActive(true);
		GamePanel.transform.DOMoveY(panelStartY, transitionDuration).SetEase(transitionEase);
		ScenarioPanel.transform.DOMoveY(panelStartY - panelOffsetY, transitionDuration).SetEase(transitionEase).OnComplete(() => ScenarioPanel.SetActive(false));
	}

	public void ScrollToScenarioPanel() {
		ScenarioPanel.SetActive(true);
		ScenarioPanel.transform.DOMoveY(panelStartY, transitionDuration).SetEase(transitionEase);
		GamePanel.transform.DOMoveY(panelStartY + panelOffsetY, transitionDuration).SetEase(transitionEase).OnComplete(() => GamePanel.SetActive(false));
	}

	public void ToggleColor() {
		CurrentColor = (CurrentColor == Leaning.Blue)? Leaning.Red : Leaning.Blue;
		ColorSwapperBase[] colorSwappers = ScenarioPanel.GetComponentsInChildren<ColorSwapperBase>();
		foreach (ColorSwapperBase colorSwapper in colorSwappers) {
			colorSwapper.SetColor(CurrentColor);
		}
	}

	public void Play() {
		string playerName = "Jerry F."; // TODO: get player name
		/*
		// This is how we would pass additional info if matchAttributes worked, but they don't.
		// Maybe we can revisit with a later version of Unity
		CreateMatchRequest request = new CreateMatchRequest();
		request.advertise = true;
		request.name = "Jerry F.";
		request.size = 2;
		request.password = "";

		Dictionary<string, long> attributes = new Dictionary<string, long>();
		attributes.Add("scenarioId", CurrentScenarioModel.Id);
		attributes.Add("color", (long) CurrentColor);
		request.matchAttributes = attributes;

		MatchMaker.CreateMatch(request, OnCreateMatch);
		*/
		string info = playerName + MatchInfoDivider + CurrentScenarioModel.Id + MatchInfoDivider + ((int) CurrentColor);
		MatchMaker.CreateMatch(info, 2, true, "", OnCreateMatch);

		ScrollToGamePanel();
		WaitingModal.SetActive(true);
	}

	void OnCreateMatch(CreateMatchResponse response) {
		hostedMatchId = (uint) response.networkId;
		if (matchList != null) matchList.Refresh();
	}

	public void CancelMatch() {
		Debug.Log (hostedMatchId);
		MatchMaker.DestroyMatch((UnityEngine.Networking.Types.NetworkID) hostedMatchId, OnMatchDestroyed);
		WaitingModal.SetActive(false);
	}

	void OnMatchDestroyed(BasicResponse response) {
		Debug.Log ("** Match destroyed: "+response.extendedInfo);
		if (matchList != null) matchList.Refresh();
	}

	public void StartAIGame() {
		CancelMatch();
		GameManager.StartAIGame = true;
		GameManager.ChosenScenario = CurrentScenarioModel;
		GameManager.ChosenLeaning = CurrentColor;
		Overlay.SetActive(true);
		Application.LoadLevel("game");
	}
}
