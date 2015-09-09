﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StatePopup : MonoBehaviour {
	public GameObject inPlayGroup;
	public GameObject notInPlayGroup;
	public Text abbreviationLabel;
	public Text nameLabel;
	public Text populationLabel;
	public Text playerPercentLabel;
	public Text opponentPercentLabel;
	public Text playerUnitsCountLabel;
	public Text opponentUnitsCountLabel;
	public Text playerIncrementLabel;
	public Text opponentIncrementLabel;
	public StatePopupButton addUnitButton;
	public StatePopupButton removeUnitButton;
	public Image headerBg;
	public Image leftArrow;
	public Image rightArrow;
	public PercentMeter voteMeter;

	private State currentState;
	
	void Start() {
		Close();
	}

	public void Show(State state = null) {
		if (!state) state = State.HighlightedState;
		if (!state) return;

		currentState = state;
		gameObject.SetActive(true);
		currentState.Highlight();
		
		// We show different content depending on whether the state is in play or not
		if (inPlayGroup) inPlayGroup.SetActive(state.InPlay);
		if (notInPlayGroup) notInPlayGroup.SetActive(!state.InPlay);
		
		// Basic state info
		if (nameLabel) nameLabel.text = state.Model.Name;
		if (abbreviationLabel) abbreviationLabel.text = state.Model.Abbreviation;
		if (populationLabel) populationLabel.text = "Population " + state.Model.Population.ToString() + "M";
		
		// Show the current support percentages
		string redPercent = Mathf.Round(state.RedSupportPercent * 100).ToString() + "%";
		string bluePercent = Mathf.Round(state.BlueSupportPercent * 100).ToString() + "%";
		if (playerPercentLabel) playerPercentLabel.text = (GameManager.Instance.PlayerIsBlue)? bluePercent : redPercent;
		if (opponentPercentLabel) opponentPercentLabel.text = (GameManager.Instance.PlayerIsBlue)? redPercent : bluePercent;
		
		if (voteMeter) {
			float percent = (GameManager.Instance.PlayerIsBlue)? state.RedSupportPercent : state.BlueSupportPercent;
			voteMeter.Set(percent, false);
		}
		
		RefreshWorkerInfo();
		
		// Set the color
		Color color = GameSettings.Instance.Colors.undiscoveredState; // gray
		if (state.IsBlue) {
			color = GameSettings.Instance.Colors.darkerBlue;
		} else if (state.IsRed) {
			color = GameSettings.Instance.Colors.darkerRed;
		}
		
		if (headerBg) headerBg.color = color;
		if (leftArrow) leftArrow.color = color;
		if (rightArrow) rightArrow.color = color;

		/* TODO
		// place near state
		Vector3 pos = transform.position;
		Vector3 statePos = state.Center;
		pos.y = Mathf.Clamp(statePos.y, -5.5f, 2.8f);
		
		// If it's too far on the right, show the popup on the left. Else it's always on the right.
		if (statePos.x < 4) {
			pos.x = statePos.x + 1.25f;
			leftArrow.gameObject.SetActive(true);
			rightArrow.gameObject.SetActive(false);
		} else {
			pos.x = statePos.x - 5.75f;
			leftArrow.gameObject.SetActive(false);
			rightArrow.gameObject.SetActive(true);
		}
		transform.position = Utility.ConvertFromGameToUiPosition(pos);
		*/
	}
	
	public void Close() {
		gameObject.SetActive(false);
		
		if (currentState) {
			currentState = null;
			currentState.UnHighlight();
		}
	}
	
	void RefreshWorkerInfo() {
		if (!currentState) return;
		
		// Show the current number of units
		if (currentState.InPlay) {
			if (playerUnitsCountLabel) playerUnitsCountLabel.text = currentState.PlayerWorkerCount.ToString() + " x";
			if (opponentUnitsCountLabel) opponentUnitsCountLabel.text = currentState.OpponentWorkerCount.ToString() + " x";
		}
		
		// Percent increment
		float percentChange = 0;
		if (playerIncrementLabel) {
			percentChange = currentState.GetPlayerPercentChange();
			if (percentChange > 0) playerIncrementLabel.text = "+"+ Mathf.Round( percentChange * 100 ) + "%";
			else playerIncrementLabel.text = "";
		}
		if (opponentIncrementLabel) {
			percentChange = currentState.GetOpponentPercentChange();
			if (percentChange > 0) opponentIncrementLabel.text = "+"+ Mathf.Round( percentChange * 100 ) + "%";
			else opponentIncrementLabel.text = "";
		}
		
		if (addUnitButton != null) {
			bool enable = currentState.PlayerCanPlaceWorker();
			addUnitButton.SetEnabled(enable, !enable); // if they can't place supporters, it's probably because of money
			addUnitButton.SetPrice(GameSettings.Instance.GetGameActionCost(GameAction.PlaceWorker));
		}
		
		if (removeUnitButton != null) {
			removeUnitButton.SetEnabled(currentState.PlayerCanRemoveWorker());
			removeUnitButton.SetPrice(GameSettings.Instance.GetGameActionCost(GameAction.RemoveWorker));
		}
	}
	
	public void PlaceWorker() {
		if (currentState) {
			currentState.PlayerPlaceWorker();
			RefreshWorkerInfo();
		}
	}
	
	public void RemoveWorker() {
		if (currentState) {
			currentState.PlayerRemoveWorker();
			RefreshWorkerInfo();
		}
	}
}
