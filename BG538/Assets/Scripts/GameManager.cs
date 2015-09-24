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
	// For AI games
	public static bool StartAIGame = false;
	public static ScenarioModel ChosenScenario = null;
	public static Leaning ChosenLeaning = Leaning.Neutral;

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

	public Player LocalPlayer { get; private set; }
	public Player OpposingPlayer { get; private set; }

	public bool PlayerIsBlue {
		get {
			return LocalPlayer != null && LocalPlayer.color == Leaning.Blue;
		}
	}

	public BudgetController PlayerBudget = new BudgetController();
	

	protected override void Start() {
		base.Start ();

		GoToState (TurnPhase.Connecting);

		if (Object.FindObjectOfType<NetworkManager>() == null || GameManager.StartAIGame) {
			OpponentAI = new AI();
			GameObject go = GameObject.Instantiate(ObjectAccessor.Instance.PlayerPrefab);
			SetPlayer(go.GetComponent<Player>(), true);
		}

		SignalManager.PlayerFinished += OnPlayerFinished;
	}
	
	protected override void OnDestroy() {
		base.OnDestroy ();

		SignalManager.PlayerFinished -= OnPlayerFinished;
	}

	public void SetPlayer(Player p, bool isLocalPlayer) {
		if (isLocalPlayer) {
			LocalPlayer = p;
			if (GameManager.ChosenLeaning != Leaning.Neutral) LocalPlayer.color = GameManager.ChosenLeaning; // get the color we chose in the menu 
			if (SignalManager.PlayerColorSet != null) SignalManager.PlayerColorSet();
		} else {
			OpposingPlayer = p;
		}
		CheckPlayerCount();
	}

	public void CheckPlayerCount() {
		if (LocalPlayer != null && (UsingAI || OpposingPlayer != null)) {
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

		ScenarioModel scenario = GameManager.ChosenScenario;
		if (scenario == null) scenario = ScenarioModel.GetModel(GameSettings.InstanceOrCreate.DefaultScenarioId);
		Debug.Log ("Scenario: " + scenario);
		if (scenario is ScenarioModel1) InitScenarioA(scenario as ScenarioModel1);
		else if (scenario is ScenarioModel2) InitScenarioB(scenario as ScenarioModel2);
		else Debug.LogError("Bad scenario!");
		
		UIManager.Instance.StateLabels.Refresh();
		UpdateElectoralVotes(true);
	}
	
	public void InitScenarioA(ScenarioModel1 scenario) {
		foreach (State state in ObjectAccessor.Instance.StatesContainer.GetComponentsInChildren<State>()) {
			state.InPlay = (scenario == null || !scenario.PresetStates.Contains(state.Model.Id));
			
			states.Add(state);
			if (state.InPlay) statesInPlay.Add(state);
			else statesNotInPlay.Add(state);
			
			if (scenario != null) {
				int stateIndex = StateModel.Models.IndexOf(state.Model);
				float vote = 0;
				if (scenario.StateLeanings.Count > stateIndex) {
					int leaning = scenario.StateLeanings[stateIndex];
					vote = InitialLeaningModel.GetModel(leaning).Value;
				}
				vote += scenario.Randomness * (Random.value * 2 - 1);
				state.SetInitialPopularVote(vote);
			} else {
				state.SetInitialPopularVote( Random.value * 2 - 1 );
			}

			state.UpdateColor();
		}
	}
	
	public void InitScenarioB(ScenarioModel2 scenario) {
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

		Debug.Log ("BeginWeek, setting Ready to false");
		LocalPlayer.SetFinished(false);
		if (OpposingPlayer) OpposingPlayer.SetFinished(false);

		SignalManager.BeginWeek(CurrentWeek);
		
		if (CurrentWeek >= GameSettings.InstanceOrCreate.TotalWeeks) {
			GoToState(TurnPhase.ElectionDay);
		} else {
			int index = Mathf.Min(CurrentWeek, GameSettings.InstanceOrCreate.Income.Length - 1);
			float income = GameSettings.InstanceOrCreate.Income[index];

			PlayerBudget.GainAmount(income);
			if (UsingAI) OpponentAI.Budget.GainAmount(income);

			GoToState(TurnPhase.Placement);
		}
	}
	
	private void BeginPlacement() {
		Debug.Log ("BeginPlacement. UsingAI: " + UsingAI);
		if (UsingAI) OpponentAI.DoTurn();
	}
	
	private void BeginHarvest() {
		foreach (State state in statesInPlay) {
			state.PrepareToHarvest();
		}

		ObjectAccessor.Instance.HarvestTimer.StartTimer(NextHarvestAction);
	}
	
	private void FinishHarvest() {
		ObjectAccessor.Instance.HarvestTimer.StopTimer();
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
			LocalPlayer.SetFinished(true);
		}
	}

	public void OnPlayerFinished(Leaning playerColor) {
		if (LocalPlayer.Finished && (UsingAI || OpposingPlayer.Finished)) {
			GoToState (TurnPhase.Harvest); // Ready!
		} else if (LocalPlayer.Finished) {
			GoToState(TurnPhase.Waiting);
		}
	}
}
