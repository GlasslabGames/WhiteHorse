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

public class GameManager : SingletonBehavior<GameManager> {
	private List<State> states = new List< State >();
	private List<State> statesInPlay = new List< State >();
	private List<State> statesNotInPlay = new List< State >();

	public TurnPhase CurrentTurnPhase { get; private set; }

	private bool opponentIsWaiting;
	public int CurrentWeek { get; private set; }
	
	private int playerVotes;
	private int opponentVotes;
	public bool PlayerIsWinning {
		get { return playerVotes >= opponentVotes; }
	}

	public bool UsingAI { get; set; }
	public bool PlayerIsBlue { get; set; }
	
	public void InitScenario() {
		states.Clear();
		statesInPlay.Clear();
		statesNotInPlay.Clear();
		
		if (GameSettings.Instance.DefaultScenarioType == GameSettings.ScenarioType.B) {
			InitScenarioB();
		} else {
			InitScenarioA();
		}
		
		//TODO UIManager.Instance.StateLabelShower.Refresh();
	}
	
	public void InitScenarioA() {
		ScenarioModel scenario = ScenarioModel.GetModel(GameSettings.Instance.DefaultScenarioId);
		
		foreach (State state in ObjectAccessor.Instance.StatesContainer.GetComponentsInChildren<State>()) {
			state.InPlay = (scenario == null || !scenario.PresetStates.Contains(state.Model.Id));
			
			states.Add(state);
			if (state.InPlay) statesInPlay.Add(state);
			else statesNotInPlay.Add(state);
			
			if (scenario != null) {
				int stateIndex = StateModel.Models.IndexOf(state.Model);
				int leaning = scenario.StateLeanings[stateIndex];
				float vote = InitialLeaningModel.GetModel(leaning).Value;
				vote += scenario.Randomness * (Random.value * 2 - 1);
				state.SetInitialPopularVote(vote);
			} else {
				state.SetInitialPopularVote( Random.value * 2 - 1 );
			}
			
			state.UpdateColor();
		}
	}
	
	public void InitScenarioB() {
		ScenarioModel2 scenario = ScenarioModel2.GetModel(GameSettings.Instance.DefaultScenarioId);
		
		int numStatesToAdd = 0;
		int blueStatesAdded = 0;
		int redStatesAdded = 0;
		if (scenario != null) {
			numStatesToAdd = Random.Range(scenario.MinStatesInPlay, scenario.MaxStatesInPlay + 1); // since max would be excluded
		}
		List<State> maybeBlueStates = new List<State>();
		List<State> maybeRedStates = new List<State>();
		
		foreach (State state in ObjectAccessor.Instance.StatesContainer.GetComponentsInChildren<State>()) {
			if (scenario != null) {
				int percentBlue = scenario.PercentBlue[state.Model.Id - 1];
				state.SetInitialPopularVote(percentBlue / 50f - 1);
				
				ScenarioModel2.InPlayStatus status = (ScenarioModel2.InPlayStatus)scenario.StatesInPlay[state.Model.Id - 1];
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
			states.Add(state);
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
		
		foreach (State state in states) {
			if (state.InPlay) {
				statesInPlay.Add(state);
			} else {
				statesNotInPlay.Add(state);
			}
			state.UpdateColor();
		}
	}
	
	public void Start() {
		InitScenario();
		UpdateElectoralVotes(true);
		
		GoToState(TurnPhase.BeginGame);
	}
	
	public void GoToState(TurnPhase nextState) {
		// Finish the current state
		switch (CurrentTurnPhase) {
		case TurnPhase.Harvest:
			FinishHarvest();
			break;
		case TurnPhase.ElectionDay:
			FinishElectionDay();
			break;
		}
		SignalManager.ExitTurnPhase (CurrentTurnPhase);
		
		CurrentTurnPhase = nextState;
		Debug.Log("Enter state " + CurrentTurnPhase.ToString());
		
		// Begin the new state
		switch (CurrentTurnPhase) {
		case TurnPhase.BeginGame:
			RestartGame();
			break;
		case TurnPhase.BeginWeek:
			BeginWeek();
			break;
		case TurnPhase.Placement:
			BeginPlacement();
			break;
		case TurnPhase.Harvest:
			BeginHarvest();
			break;
		}
		SignalManager.EnterTurnPhase (CurrentTurnPhase);
	}
	
	public void RestartGame() {
		Debug.Log("Reset game!");
		
		CurrentWeek = -1;
		
		// Reset budget
		// TODO
		//GameObjectAccessor.Instance.Budget.ResetPool();
		//GameObjectAccessor.Instance.OpponentAi.Budget.ResetPool();
		
		// Reset scenario
		InitScenario();
		UpdateElectoralVotes(true);
		
		// Reset states
		foreach (State state in states) {
			state.ResetWorkers();
		}
		
		GoToState(TurnPhase.BeginWeek);
	}
	
	private void BeginWeek() {
		CurrentWeek ++;
		UpdateWeeksRemaining(GameSettings.Instance.TotalWeeks - CurrentWeek);
		UpdateElectoralVotes();
		
		if (CurrentWeek >= GameSettings.Instance.TotalWeeks) {
			GoToState(TurnPhase.ElectionDay);
		} else {
			int index = Mathf.Min(CurrentWeek, GameSettings.Instance.Income.Length - 1);
			float income = GameSettings.Instance.Income[index];

			// TODO
			//GameObjectAccessor.Instance.Budget.GainAmount(income);
			//GameObjectAccessor.Instance.OpponentAi.Budget.GainAmount(income);
			
			GoToState(TurnPhase.Placement);
		}
	}
	
	private void BeginPlacement() {
		if (UsingAI) {
			//TODO GameObjectAccessor.Instance.OpponentAi.DoTurn();
			opponentIsWaiting = true;
		}
	}
	
	private void BeginHarvest() {
		foreach (State state in statesInPlay) {
			state.PrepareToHarvest();
		}
		
		//TODO m_harvestTimer.StartTimer(NextHarvestAction);
	}
	
	private void FinishHarvest() {
		//TODO m_harvestTimer.StopTimer();
	}
	
	private void FinishElectionDay() {
		/* TODO
		GameObjectAccessor.Instance.HeaderBg.Reset();
		GameObjectAccessor.Instance.ResultText.gameObject.SetActive(false);
		GameObjectAccessor.Instance.RestartButton.gameObject.SetActive(false);
		GameObjectAccessor.Instance.HeaderInset.gameObject.SetActive(false);
		GameObjectAccessor.Instance.Background.color = Color.white;
		*/
	}
	
	public void NextHarvestAction() {
		foreach (State state in statesInPlay) {
			if (state.NextHarvestAction(UsingAI)) return;
			// If that state had something to do, wait for the next cycle. Else keep looking for something to do.
		}
		
		Debug.Log("completed harvest");
		GoToState(TurnPhase.BeginWeek);
	}
	
	private void UpdateWeeksRemaining(int week) {
		/* TODO
		if (GameObjectAccessor.Instance.WeekCounter != null) {
			if (week <= 1) {
				GameObjectAccessor.Instance.WeekCounter.text = "FINAL WEEK";
			} else {
				GameObjectAccessor.Instance.WeekCounter.text = week + " WEEKS REMAINING";
			}
		}
		*/
	}
	
	public void UpdateElectoralVotes(bool atBeginning = false) {
		int totalRedVotes = 0;
		int totalBlueVotes = 0;
		
		foreach (State state in states) {
			if (state.IsRed) {
				totalRedVotes += state.Model.ElectoralCount;
			} else if (state.IsBlue) {
				totalBlueVotes += state.Model.ElectoralCount;
			}
		}
		
		playerVotes = (PlayerIsBlue)? totalBlueVotes : totalRedVotes;
		opponentVotes = (PlayerIsBlue)? totalRedVotes : totalBlueVotes;

		//TODO GameObjectAccessor.Instance.PlayerVoteCount.Set(m_playerVotes, !atBeginning);
		//TODO GameObjectAccessor.Instance.OpponentVoteCount.Set(m_opponentVotes, !atBeginning);
	}
	
	public void FinishWeek() {
		if (CurrentTurnPhase == TurnPhase.Placement) {

			if (opponentIsWaiting) {
				GoToState(TurnPhase.Harvest);
			} else {
				GoToState(TurnPhase.Waiting);
			}
			
			if (!UsingAI) {
				//TODO networkView.RPC("OpponentFinishWeek", RPCMode.Others);
			}
		}
	}
	
	[RPC]
	public void OpponentFinishWeek() {
		Debug.Log("opponent turn completed!");
		
		opponentIsWaiting = true;
		if (CurrentTurnPhase == TurnPhase.Waiting) {
			GoToState(TurnPhase.Harvest);
		}
	}
}
