﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : SingletonBehavior<UIManager> {

	public StatePopup statePopup;
	public StateLabelManager StateLabels;
	public Header header;

	public GameObject endTurnButton;
	public GameObject restartButton;
	public GameObject waitingText;
	public GameObject waitingIndicator;

	public Text weekText;

	protected override void Start () {
		base.Start();

		SignalManager.EnterTurnPhase += OnEnterTurnPhase;
		SignalManager.ExitTurnPhase += OnExitTurnPhase;
		SignalManager.BeginWeek += OnBeginWeek;
	}

	void OnEnterTurnPhase(TurnPhase phase) {
		switch (phase) {
		case TurnPhase.Placement:
			if (endTurnButton) endTurnButton.SetActive (true);
			break;
		case TurnPhase.Waiting:
			if (waitingText) waitingText.SetActive(true);
			if (waitingIndicator) waitingIndicator.SetActive(true);
			break;
		case TurnPhase.Harvest:
			statePopup.Close();
			break;
		case TurnPhase.ElectionDay:
			if (weekText) weekText.text = "THE RESULTS ARE IN...";
			if (header) header.ShowGameOver(GameManager.Instance.PlayerIsWinning, AfterResults);
			break;
		}
	}

	void OnExitTurnPhase(TurnPhase phase) {
		switch (phase) {
		case TurnPhase.Placement:
			if (endTurnButton) endTurnButton.gameObject.SetActive(false);
			break;
		case TurnPhase.Waiting:
			if (waitingText) waitingText.SetActive(false);
			if (waitingIndicator) waitingIndicator.SetActive(false);
			break;
		case TurnPhase.Harvest:
			if (statePopup) statePopup.Close();
			if (State.HighlightedState) State.HighlightedState.UnHighlight();
			break;
		case TurnPhase.ElectionDay:
			HideElectionResults();
			break;
		}
	}

	void OnBeginWeek(int week) {
		if (weekText) {
			int remainingWeeks = GameSettings.Instance.TotalWeeks - week;
			if (remainingWeeks <= 1) {
				weekText.text = "FINAL WEEK";
			} else {
				weekText.text = remainingWeeks + " WEEKS REMAINING";
			}
		}
	}

	void AfterResults() {
		if (restartButton) restartButton.SetActive(true);

		Color c = (GameManager.Instance.PlayerIsWinning ^ GameManager.Instance.PlayerIsBlue)? GameSettings.Instance.Colors.lightRed : GameSettings.Instance.Colors.lightBlue;
		if (ObjectAccessor.Instance.Background) ObjectAccessor.Instance.Background.color = c;

		//GameObject.Instantiate(GameManager.Instance.PlayerIsWinning? GameObjectAccessor.Instance.VictorySound : GameObjectAccessor.Instance.DefeatSound);
	}

	void HideElectionResults() {
		if (header) {
			header.Reset();
		}
		if (restartButton) restartButton.gameObject.SetActive(false);
		if (ObjectAccessor.Instance.Background) ObjectAccessor.Instance.Background.color = Color.white;
	}
}
