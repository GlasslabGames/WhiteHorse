using UnityEngine;
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
			float percent = state.PlayerSupportPercent;
			voteMeter.Set(percent, false);
		}
		
		RefreshWorkerInfo();
		
		// Set the color
		Color color = GameSettings.InstanceOrCreate.Colors.undiscoveredState; // gray
		if (state.IsBlue) {
			color = GameSettings.InstanceOrCreate.Colors.darkerBlue;
		} else if (state.IsRed) {
			color = GameSettings.InstanceOrCreate.Colors.darkerRed;
		}
		
		if (headerBg) headerBg.color = color;
		if (leftArrow) leftArrow.color = color;
		if (rightArrow) rightArrow.color = color;

		// place near state
		Vector3 pos = transform.position; // keeps the same Z
		Vector3 statePos = state.Center;
		pos.y = Mathf.Max(statePos.y, -5f); // sorry I hardcoded this stuff

		// Show popups on the left side facing to the right, and vice-versa
		if (statePos.x < 0) {
			pos.x = statePos.x + 4.35f;
			leftArrow.gameObject.SetActive(true);
			rightArrow.gameObject.SetActive(false);
		} else {
			pos.x = statePos.x - 4.35f;
			leftArrow.gameObject.SetActive(false);
			rightArrow.gameObject.SetActive(true);
		}
		transform.position = pos;
	}
	
	public void Close() {
		gameObject.SetActive(false);
		
		if (currentState) {
			currentState.UnHighlight();
			currentState = null;
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
			addUnitButton.SetPrice(GameSettings.InstanceOrCreate.GetGameActionCost(GameAction.PlaceWorker));
		}
		
		if (removeUnitButton != null) {
			removeUnitButton.SetEnabled(currentState.PlayerCanRemoveWorker());
			removeUnitButton.SetPrice(GameSettings.InstanceOrCreate.GetGameActionCost(GameAction.RemoveWorker));
		}
	}
	
	public void PlaceWorker() {
		Debug.Log ("PlaceWorker");
		if (currentState) {
			GameManager.Instance.PlayerPlaceWorker(currentState);
			RefreshWorkerInfo();
		}
	}
	
	public void RemoveWorker() {
		Debug.Log ("RemoveWorker");
		if (currentState) {
			GameManager.Instance.PlayerRemoveWorker(currentState);
			RefreshWorkerInfo();
		}
	}
}
