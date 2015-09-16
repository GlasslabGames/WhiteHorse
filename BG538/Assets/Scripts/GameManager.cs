using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public enum TurnPhase {
	Connecting,
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

	private Dictionary<string, State> _statesByAbbreviation = new Dictionary<string, State>();
	public Dictionary<string, State> StatesByAbbreviation {
		get {
			if (_statesByAbbreviation.Count == 0) {
				foreach (State s in states) {
					_statesByAbbreviation.Add(s.Model.Abbreviation, s);
				}
			}
			return _statesByAbbreviation;
		}
	}

	public TurnPhase CurrentTurnPhase { get; private set; }

	private bool opponentIsReady;
	public int CurrentWeek { get; private set; }
	
	private int playerVotes;
	private int opponentVotes;
	public bool PlayerIsWinning {
		get { return playerVotes >= opponentVotes; }
	}

	public AI OpponentAI;
	public bool UsingAI {
		get { return OpponentAI != null; }
	}
	
	[HideInInspector]
	public Player LocalPlayer;

	public bool PlayerIsBlue {
		get {
			return LocalPlayer != null && LocalPlayer.color == Leaning.Blue;
		}
	}
	public BudgetController PlayerBudget = new BudgetController();

	public Timer HarvestTimer;


	protected override void Start() {
		base.Start ();

		GoToState (TurnPhase.Connecting);

		if (Object.FindObjectOfType<NetworkManager>() == null) {
			Debug.Log ("No network connection. Creating localPlayer now...");
			OpponentAI = new AI();
			GameObject go = GameObject.Instantiate(ObjectAccessor.Instance.PlayerPrefab);
			SetPlayer(go.GetComponent<Player>());
		}
	}

	public void SetPlayer(Player p) {
		Debug.Log ("Setting local player!");
		LocalPlayer = p;
		if (SignalManager.PlayerColorSet != null) SignalManager.PlayerColorSet();
		CheckPlayerCount();
	}

	public void CheckPlayerCount() {
		Debug.Log ("CheckPlayerCount: "+Object.FindObjectsOfType<Player>().Length);
		if (LocalPlayer != null && (UsingAI || Object.FindObjectsOfType<Player> ().Length >= 2)) {
			if (CurrentTurnPhase == TurnPhase.Connecting) { // we were waiting to connect, so now begin game
				GoToState (TurnPhase.BeginGame);
			}
		} else if (CurrentTurnPhase != TurnPhase.Connecting) { // we lost a player in the middle of the game
			NetworkManager nm = Object.FindObjectOfType<NetworkManager>();
			if (nm != null) nm.StopHost(); // quit the connection
		}

	}

	public void InitScenario() {
		states.Clear();
		statesInPlay.Clear();
		statesNotInPlay.Clear();
		
		if (GameSettings.Instance.DefaultScenarioType == GameSettings.ScenarioType.B) {
			InitScenarioB();
		} else {
			InitScenarioA();
		}
		
		UIManager.Instance.StateLabels.Refresh();
		UpdateElectoralVotes(true);
	}
	
	public void InitScenarioA() {
		ScenarioModel scenario = ScenarioModel.GetModel(GameSettings.Instance.DefaultScenarioId);
		Debug.Log(GameSettings.Instance.DefaultScenarioId + ": " + scenario);
		
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
	
	public void GoToState(TurnPhase nextState) {
		// Finish the current state
		switch (CurrentTurnPhase) {
		case TurnPhase.Harvest:
			FinishHarvest();
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
		PlayerBudget.Reset();

		if (UsingAI) OpponentAI.Reset();

		// Reset scenario
		InitScenario();

		// Reset states
		foreach (State state in states) {
			state.ResetWorkers();
		}
		
		GoToState(TurnPhase.BeginWeek);
	}
	
	private void BeginWeek() {
		CurrentWeek ++;
		UpdateElectoralVotes();
		opponentIsReady = false;

		SignalManager.BeginWeek(CurrentWeek);
		
		if (CurrentWeek >= GameSettings.Instance.TotalWeeks) {
			GoToState(TurnPhase.ElectionDay);
		} else {
			int index = Mathf.Min(CurrentWeek, GameSettings.Instance.Income.Length - 1);
			float income = GameSettings.Instance.Income[index];

			PlayerBudget.GainAmount(income);
			if (UsingAI) OpponentAI.Budget.GainAmount(income);

			GoToState(TurnPhase.Placement);
		}
	}
	
	private void BeginPlacement() {
		Debug.Log ("BeginPlacement. UsingAI: " + UsingAI);
		if (UsingAI) {
			OpponentAI.DoTurn();
			opponentIsReady = true;
		}
	}
	
	private void BeginHarvest() {
		foreach (State state in statesInPlay) {
			state.PrepareToHarvest();
		}
		
		HarvestTimer.StartTimer(NextHarvestAction);
	}
	
	private void FinishHarvest() {
		HarvestTimer.StopTimer();
	}
	
	public void NextHarvestAction() {
		foreach (State state in statesInPlay) {
			if (state.NextHarvestAction(UsingAI)) return;
			// If that state had something to do, wait for the next cycle. Else keep looking for something to do.
		}
		
		Debug.Log("completed harvest");
		GoToState(TurnPhase.BeginWeek);
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

		if (SignalManager.PlayerVotesChanged != null) SignalManager.PlayerVotesChanged(playerVotes, !atBeginning);
		if (SignalManager.OpponentVotesChanged != null) SignalManager.OpponentVotesChanged(opponentVotes, !atBeginning);
	}
	
	public void FinishWeek() {
		if (CurrentTurnPhase == TurnPhase.Placement) {
			LocalPlayer.FinishTurn();
		}
	}
	
	public void SetReady(bool isBlue) {
		Debug.Log("SetReady for blue? "+isBlue);

		if (isBlue ^ PlayerIsBlue) {
			opponentIsReady = true;
		} else {
			GoToState(TurnPhase.Waiting);
		}

		// Once both are ready, proceed to the harvest
		if (opponentIsReady && CurrentTurnPhase == TurnPhase.Waiting) GoToState (TurnPhase.Harvest);
	}
}
