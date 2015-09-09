using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : SingletonBehavior<UIManager> {

	public StatePopup statePopup;
	//TODO public Header header;

	public GameObject endTurnButton;
	public GameObject restartButton;
	public GameObject waitingText;
	public GameObject waitingIndicator;

	public Text weekText;
	public Text resultText;

	void Start () {
		SignalManager.EnterTurnPhase += OnEnterTurnPhase;
		SignalManager.ExitTurnPhase += OnEnterTurnPhase;
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
			//TODO if (statePopup) statePopup.Close();
			break;
		case TurnPhase.ElectionDay:
			if (weekText) weekText.text = "THE RESULTS ARE IN...";
			//TODO if (header) header.TweenImage(playerIsWinning, new EventDelegate(ShowElectionResults));
			ShowElectionResults(); // FIXME
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
			//TODO if (statePopup) statePopup.Close();
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

	void ShowElectionResults() {
		bool playerIsWinning = GameManager.Instance.PlayerIsWinning;

		if (resultText) {
			resultText.gameObject.SetActive (true);
			resultText.text = playerIsWinning ? "YOU WIN!" : "YOU LOSE!";

			/* TODO
			resultText.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
			TweenScale t = TweenScale.Begin(resultText.gameObject, 1f, Vector3.one);
			t.method = UITweener.Method.BounceIn;
			*/
		}

		//TODO if (header) header.toggleInset(true);
		if (restartButton) restartButton.SetActive(true);

		Color c = (playerIsWinning ^ GameManager.Instance.PlayerIsBlue)? GameSettings.Instance.Colors.lightRed : GameSettings.Instance.Colors.lightBlue;
		if (ObjectAccessor.Instance.Background) ObjectAccessor.Instance.Background.color = c;
		
		//TODO GameObject.Instantiate(playerIsWinning? GameObjectAccessor.Instance.VictorySound : GameObjectAccessor.Instance.DefeatSound);
	}

	void HideElectionResults() {
		/*TODO if (header) {
			header.Reset();
			header.toggleInset(false);
		}
		*/
		if (resultText) resultText.gameObject.SetActive(false);
		if (restartButton) restartButton.gameObject.SetActive(false);
		if (ObjectAccessor.Instance.Background) ObjectAccessor.Instance.Background.color = Color.white;
	}
}
