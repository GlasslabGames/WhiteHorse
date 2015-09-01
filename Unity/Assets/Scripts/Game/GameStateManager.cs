using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TurnPhase {
	BeginGame,
	BeginWeek,
	Placement,
	Waiting,
	Harvest,
	ElectionDay
}

public class GameStateManager : MonoBehaviour {
	private List< State > m_states = new List< State >();
	private List< State > m_statesInPlay = new List< State >();
	private List< State > m_statesNotInPlay = new List< State >();

	private TurnPhase m_currentTurnPhase;
	public TurnPhase CurrentTurnPhase {
		get { return m_currentTurnPhase; }
	}
	private bool m_opponentIsWaiting;

	private int m_currentWeek;
	public int TotalWeeks;
	public float[] m_income;
	public float WorkerIncrement;

	public Timer m_harvestTimer;

	private int m_playerVotes;
	private int m_opponentVotes;
	public int m_defaultScenarioId;
	
	public void Awake() { }

	public void InitScenario() {
		ScenarioModel2 m_scenario = ScenarioModel2.GetModel(m_defaultScenarioId);

		int numStatesToAdd = 0;
		int blueStatesAdded = 0;
		int redStatesAdded = 0;
		if (m_scenario != null) {
			numStatesToAdd = Random.Range(m_scenario.MinStatesInPlay, m_scenario.MaxStatesInPlay + 1); // since max would be excluded
		}
		List<State> maybeBlueStates = new List<State>();
		List<State> maybeRedStates = new List<State>();

		foreach (State state in GameObjectAccessor.Instance.StatesContainer.GetComponentsInChildren<State>()) {
			if (m_scenario != null) {
				int percentBlue = m_scenario.PercentBlue[state.Model.Id - 1];
				state.SetInitialPopularVote(percentBlue / 50f - 1);

				ScenarioModel2.InPlayStatus status = (ScenarioModel2.InPlayStatus)m_scenario.StatesInPlay[state.Model.Id - 1];
				if (status == ScenarioModel2.InPlayStatus.ALWAYS) {
					state.InPlay = true;
					if (state.IsBlue) {
						blueStatesAdded ++;
					} else {
						redStatesAdded ++;
					}
				} else if (status == ScenarioModel2.InPlayStatus.MAYBE) {
						if (state.IsBlue) {
							maybeBlueStates.Add(state);
						} else {
							maybeRedStates.Add(state);
						}
					}

			} else {
				state.InPlay = true;
			}
			m_states.Add(state);
		}

		int r;
		State s;
		List<State> list;
		while ((blueStatesAdded + redStatesAdded) < numStatesToAdd || blueStatesAdded != redStatesAdded) {
			Debug.Log("Blue states: " + blueStatesAdded + " Red states: " + redStatesAdded + " NumStatesToAdd: " + numStatesToAdd);
			if (blueStatesAdded < redStatesAdded) {
				r = Random.Range(0, maybeBlueStates.Count);
				s = maybeBlueStates[r];
				maybeBlueStates.Remove(s);
				s.InPlay = true;
				blueStatesAdded ++;
			} else {
				r = Random.Range(0, maybeRedStates.Count);
				s = maybeRedStates[r];
				maybeRedStates.Remove(s);
				s.InPlay = true;
				redStatesAdded ++;
			}
		}

		foreach (State state in m_states) {
			if (state.InPlay) {
				m_statesInPlay.Add(state);
			} else {
				m_statesNotInPlay.Add(state);
			}
			state.UpdateColor();
		}

		GameObjectAccessor.Instance.StateLabelShower.Refresh();
	}

	public void Start() {
		InitScenario();
		UpdateElectoralVotes(true);

		GoToState(TurnPhase.BeginGame);
	}

	public void GoToState(TurnPhase nextState) {
		// Finish the current state
		switch (m_currentTurnPhase) {
		case TurnPhase.Placement:
			FinishPlacement();
			break;
		case TurnPhase.Waiting:
			FinishWaiting();
			break;
		case TurnPhase.Harvest:
			FinishHarvest();
			break;
		}

		m_currentTurnPhase = nextState;
		Debug.Log("Enter state " + m_currentTurnPhase.ToString());

		// Begin the new state
		switch (m_currentTurnPhase) {
		case TurnPhase.BeginGame:
			RestartGame();
			break;
		case TurnPhase.BeginWeek:
			BeginWeek();
			break;
		case TurnPhase.Placement:
			BeginPlacement();
			break;
		case TurnPhase.Waiting:
			BeginWaiting();
			break;
		case TurnPhase.Harvest:
			BeginHarvest();
			break;
		case TurnPhase.ElectionDay:
			BeginElectionDay();
			break;
		}
	}

	private void RestartGame() {
		Debug.Log("Reset game!");
		
		m_currentWeek = -1;

		GoToState(TurnPhase.BeginWeek);
	}

	private void BeginWeek() {
		m_currentWeek ++;
		UpdateWeeksRemaining(TotalWeeks - m_currentWeek);
		UpdateElectoralVotes();

		if (m_currentWeek > TotalWeeks) {
			GoToState(TurnPhase.ElectionDay);
		} else {
			float income = m_income[Mathf.Min(m_currentWeek, m_income.Length - 1)];
			
			GameObjectAccessor.Instance.Budget.GainAmount(income);
			GameObjectAccessor.Instance.OpponentAi.Budget.GainAmount(income);

			GoToState(TurnPhase.Placement);
		}
	}

	private void BeginPlacement() {
		if (GameObjectAccessor.Instance.UseAI) {
			GameObjectAccessor.Instance.OpponentAi.DoTurn();
			m_opponentIsWaiting = true;
		}

		GameObjectAccessor.Instance.EndTurnButton.gameObject.SetActive(true);
		// TODO: Show the reset button
	}

	private void FinishPlacement() {
		GameObjectAccessor.Instance.EndTurnButton.gameObject.SetActive(false);
		// TODO: Hide reset button
	}

	private void BeginWaiting() {
		if (m_opponentIsWaiting) {
			GoToState(TurnPhase.Harvest);
		} else {
			GameObjectAccessor.Instance.WaitingText.gameObject.SetActive(true);
		}
	}

	private void FinishWaiting() {
		GameObjectAccessor.Instance.WaitingText.gameObject.SetActive(false);
	}

	private void BeginHarvest() {
		foreach (State state in m_statesInPlay) {
			state.PrepareToHarvest();
		}

		GameObjectAccessor.Instance.DetailView.ClearState();

		m_harvestTimer.StartTimer(NextHarvestAction);
	}

	private void FinishHarvest() {
		GameObjectAccessor.Instance.DetailView.ClearState(); // deselect whatever state was selected

		m_harvestTimer.StopTimer();
	}

	private void BeginElectionDay() {
		GameObjectAccessor.Instance.GameOverScreen.SetActive(true);
		
		// Rather than renaming the GameOverRedVotes, etc, just know that RedVotes is on the left (the player) and BlueVotes is on the right (the opponent)
		GameObjectAccessor.Instance.GameOverRedVotes.text = GameObjectAccessor.Instance.PlayerVotesLabel.text;
		GameObjectAccessor.Instance.GameOverRedVotes.color = AutoSetColor.GetColor(true, AutoSetColor.ColorChoice.LIGHT);
		
		GameObjectAccessor.Instance.GameOverBlueVotes.text = GameObjectAccessor.Instance.OpponentVotesLabel.text;
		GameObjectAccessor.Instance.GameOverBlueVotes.color = AutoSetColor.GetColor(false, AutoSetColor.ColorChoice.LIGHT);
		
		if (m_playerVotes > m_opponentVotes) {
			// victory sound
			GameObject.Instantiate(GameObjectAccessor.Instance.VictorySound);
		} else {
			// defeat sound
			GameObject.Instantiate(GameObjectAccessor.Instance.DefeatSound);
		}
	}

	public void NextHarvestAction() {
		foreach (State state in m_statesInPlay) {
			if (state.NextHarvestAction()) return;
			// If that state had something to do, wait for the next cycle. Else keep looking for something to do.
		}

		Debug.Log("completed harvest");
		GoToState(TurnPhase.BeginWeek);
	}

	private void UpdateWeeksRemaining(int week) {
		if (GameObjectAccessor.Instance.WeekCounter != null) {
			if (week <= 1) {
				GameObjectAccessor.Instance.WeekCounter.text = "FINAL WEEK";
			} else {
				GameObjectAccessor.Instance.WeekCounter.text = week + " WEEKS REMAINING";
			}
		}
	}

	public void UpdateElectoralVotes(bool atBeginning = false) {
		int totalRedVotes = 0;
		int totalBlueVotes = 0;

		foreach (State state in m_states) {
			if (state.IsRed) {
				totalRedVotes += state.Model.ElectoralCount;
			} else if (state.IsBlue) {
				totalBlueVotes += state.Model.ElectoralCount;
			}
		}
	
		m_playerVotes = (GameObjectAccessor.Instance.Player.IsBlue)? totalBlueVotes : totalRedVotes;
		m_opponentVotes = (GameObjectAccessor.Instance.Player.IsBlue)? totalRedVotes : totalBlueVotes;

		GameObjectAccessor.Instance.PlayerVoteCount.Set(m_playerVotes, !atBeginning);
		GameObjectAccessor.Instance.OpponentVoteCount.Set(m_opponentVotes, !atBeginning);
	}

	public void FinishWeek() {
		if (m_currentTurnPhase == TurnPhase.Placement) {
			GoToState(TurnPhase.Waiting);

			if (!GameObjectAccessor.Instance.UseAI) {
				networkView.RPC("OpponentFinishWeek", RPCMode.Others);
			}
		}
	}

	[RPC]
	public void OpponentFinishWeek() {
		Debug.Log("opponent turn completed!");

		m_opponentIsWaiting = true;
		if (m_currentTurnPhase == TurnPhase.Waiting) {
			GoToState(TurnPhase.Harvest);
		}
	}
}