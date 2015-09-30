using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : SingletonBehavior<UIManager> {

	public StatePopup statePopup;
	public StateLabelManager StateLabels;
	public Header header;

	public GameObject endTurnButton;
	public GameObject restartButton;
	public GameObject waitingLabel;
	public GameObject waitingIndicator;
	public GameObject connectingIndicator;
	public GameObject disconnectedIndicator;
	public Text disconnectedLabel;
	public Text weekLabel;

	protected override void Start () {
		base.Start();

		if (connectingIndicator) connectingIndicator.SetActive(false);
		if (endTurnButton) endTurnButton.gameObject.SetActive(false);
		if (waitingLabel) waitingLabel.SetActive(false);
		if (waitingIndicator) waitingIndicator.SetActive(false);
		if (disconnectedIndicator) disconnectedIndicator.SetActive(false);

		SignalManager.EnterTurnPhase += OnEnterTurnPhase;
		SignalManager.ExitTurnPhase += OnExitTurnPhase;
		SignalManager.BeginWeek += OnBeginWeek;
	}

	protected override void OnDestroy() {
		base.OnDestroy ();

		SignalManager.EnterTurnPhase -= OnEnterTurnPhase;
		SignalManager.ExitTurnPhase -= OnExitTurnPhase;
		SignalManager.BeginWeek -= OnBeginWeek;
	}

	void OnEnterTurnPhase(TurnPhase phase) {
		switch (phase) {
		case TurnPhase.Connecting:
			if (header) header.Reset();
			if (connectingIndicator) {
				connectingIndicator.SetActive(true);
				MatchmakerEntry entry = connectingIndicator.GetComponentInChildren<MatchmakerEntry>();
				if (entry) entry.Set(PhotonNetwork.room);
			}
			break;
		case TurnPhase.Placement:
			if (endTurnButton) endTurnButton.SetActive (true);
			break;
		case TurnPhase.Waiting:
			if (waitingLabel) waitingLabel.SetActive(true);
			if (waitingIndicator) waitingIndicator.SetActive(true);
			break;
		case TurnPhase.Harvest:
			statePopup.Close();
			break;
		case TurnPhase.ElectionDay:
			if (weekLabel) weekLabel.text = "THE RESULTS ARE IN...";
			if (header) header.ShowGameOver(GameManager.Instance.PlayerIsWinning, AfterResults);
			break;
		case TurnPhase.Disconnected:
			if (disconnectedIndicator) disconnectedIndicator.SetActive(true);
			if (disconnectedLabel) {
				if (NetworkManager.DisconnectionInfo == NetworkManager.DisconnectionReason.opponent) {
					disconnectedLabel.text = "Your opponent disconnected.";
				} else {
					disconnectedLabel.text = "You were disconnected.";
				}
			}
			break;
		}
	}

	void OnExitTurnPhase(TurnPhase phase) {
		switch (phase) {
		case TurnPhase.Connecting:
			if (connectingIndicator) connectingIndicator.SetActive(false);
			break;
		case TurnPhase.Placement:
			if (endTurnButton) endTurnButton.gameObject.SetActive(false);
			break;
		case TurnPhase.Waiting:
			if (waitingLabel) waitingLabel.SetActive(false);
			if (waitingIndicator) waitingIndicator.SetActive(false);
			break;
		case TurnPhase.Harvest:
			if (statePopup) statePopup.Close();
			if (State.HighlightedState) State.HighlightedState.UnHighlight();
			break;
		case TurnPhase.ElectionDay:
			HideElectionResults();
			break;
		case TurnPhase.Disconnected:
			if (disconnectedIndicator) disconnectedIndicator.SetActive(false);
			break;
		}
	}

	void OnBeginWeek(int week) {
		if (weekLabel) {
			int remainingWeeks = GameSettings.InstanceOrCreate.TotalWeeks - week;
			if (remainingWeeks <= 1) {
				weekLabel.text = "FINAL WEEK";
			} else {
				weekLabel.text = remainingWeeks + " WEEKS REMAINING";
			}
		}
	}

	void AfterResults() {
		if (restartButton) restartButton.SetActive(true);

		Color c = (GameManager.Instance.PlayerIsWinning ^ GameManager.Instance.PlayerIsBlue)? GameSettings.InstanceOrCreate.Colors.lightRed : GameSettings.InstanceOrCreate.Colors.lightBlue;
		if (ObjectAccessor.Instance.Background) ObjectAccessor.Instance.Background.color = c;

		//GameObject.Instantiate(GameManager.Instance.PlayerIsWinning? GameObjectAccessor.Instance.VictorySound : GameObjectAccessor.Instance.DefeatSound);
	}

	void HideElectionResults() {
		if (header) header.Reset();
		if (restartButton) restartButton.gameObject.SetActive(false);
		if (ObjectAccessor.Instance.Background) ObjectAccessor.Instance.Background.color = Color.white;
	}
}
