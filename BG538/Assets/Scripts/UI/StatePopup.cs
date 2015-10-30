using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

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
	public Transform body;

	private State currentState;
	private Vector3 initialScale;
	
	void Start() {
		Close();
		initialScale = transform.localScale;
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
		if (populationLabel) populationLabel.text = "Population " + state.population.ToString() + "M";
		
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
		pos.y = Mathf.Max(statePos.y, -5f); // magic number *~*~*
		pos.x = statePos.x;
		transform.position = pos;

		bool onLeft = statePos.x < 0; // Show popups on the left side facing to the right, and vice-versa
		leftArrow.gameObject.SetActive(onLeft);
		rightArrow.gameObject.SetActive(!onLeft);
		float bodyX = Mathf.Abs(body.localPosition.x) * (onLeft? 1 : -1);
		body.localPosition = new Vector3(bodyX, body.localPosition.y, 0);

		// Tween
		transform.localScale = Vector3.zero;
		transform.DOScale(initialScale, 0.1f);
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
			int playerWorkers = currentState.VisiblePlayerWorkerCount;
			int opponentWorkers = currentState.VisibleOpponentWorkerCount;

			if (playerUnitsCountLabel) playerUnitsCountLabel.text = playerWorkers.ToString() + " x";
			if (opponentUnitsCountLabel) opponentUnitsCountLabel.text = opponentWorkers.ToString() + " x";
		
			// Percent increment
			float increment = GameSettings.InstanceOrCreate.WorkerIncrement;
			float percentChange = 0;
			if (playerIncrementLabel) {
				percentChange = playerWorkers * increment;
				if (percentChange > 0) playerIncrementLabel.text = "+"+ Mathf.Round( percentChange * 100 ) + "%";
				else playerIncrementLabel.text = "";
			}
			if (opponentIncrementLabel) {
				percentChange = opponentWorkers * increment;
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
