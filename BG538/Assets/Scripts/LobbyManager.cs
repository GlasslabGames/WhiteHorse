using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class LobbyManager : MonoBehaviour {
	public GameObject GamePanel;
	public GameObject ScenarioPanel;
	
	public Text ScenarioDescription;
	public Image ScenarioImage;

	public Leaning CurrentColor;
	public ScenarioModel CurrentScenarioModel;
	public GameObject ScenarioEntryPrefab;

	// Transition
	private float panelStartY;
	private float panelOffsetY;
	private float transitionDuration = 0.5f;
	private Ease transitionEase = Ease.InOutCubic;

	public void Start() {
		FillInScenarios();

		panelStartY = GamePanel.transform.position.y;
		panelOffsetY = Screen.height * 1.25f;

		GamePanel.SetActive(true);
		ScenarioPanel.SetActive(false);

		Vector3 pos = ScenarioPanel.transform.position;
		pos.y = panelStartY - panelOffsetY;
		ScenarioPanel.transform.position = pos;
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
		GameManager.ChosenScenario = CurrentScenarioModel;
		GameManager.ChosenLeaning = CurrentColor;
		Application.LoadLevel("game"); 
	}
}
