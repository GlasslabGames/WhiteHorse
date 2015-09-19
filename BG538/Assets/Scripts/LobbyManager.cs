using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class LobbyManager : MonoBehaviour {
	public GameObject GamePanel;
	public GameObject ScenarioPanel;
	public Leaning CurrentColor;
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
		foreach (ScenarioModel1 model1 in ScenarioModel1.Models) {
			CreateScenarioEntry(parent.transform, model1.Name, model1.Description, model1.Image);
		}
		foreach (ScenarioModel2 model2 in ScenarioModel2.Models) {
			CreateScenarioEntry(parent.transform, model2.Name, model2.Description, model2.Image);
		}
		LayoutRebuilder.MarkLayoutForRebuild(parent.transform as RectTransform);
	}

	void CreateScenarioEntry(Transform parent, string name, string description, string image) {
		GameObject entry = Object.Instantiate(ScenarioEntryPrefab) as GameObject;
		entry.transform.SetParent(parent, false);
		entry.GetComponent<ScenarioEntry>().Set(name);
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
}
