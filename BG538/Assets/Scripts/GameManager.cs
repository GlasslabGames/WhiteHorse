using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using ExitGames.Client.Photon;

public enum TurnPhase {
	Connecting,
	BeginGame,
	BeginWeek,
	Placement,
	Waiting,
	Harvest,
	ElectionDay,
	Disconnected
}

public class GameManager : SingletonBehavior<GameManager> {
	private List<State> states = new List< State >();

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

	/*public Player LocalPlayer { get; private set; }
	public Player OpposingPlayer { get; private set; }*/

	public bool PlayerIsBlue;
	private bool playerIsFinished;
	private bool opponentIsFinished;

	public BudgetController PlayerBudget = new BudgetController();

	private ExitGames.Client.Photon.Hashtable roomSettings;

	protected override void Start() {
		base.Start ();

		foreach (State state in ObjectAccessor.Instance.StatesContainer.GetComponentsInChildren<State>()) {
			states.Add(state);
		}

		if (PhotonNetwork.room != null) {
			roomSettings = PhotonNetwork.room.customProperties; // save for later
			Leaning color = (Leaning) roomSettings["c"];
			PlayerIsBlue = (color == Leaning.Blue) ^ PhotonNetwork.isMasterClient;
			if (SignalManager.PlayerColorSet != null) SignalManager.PlayerColorSet();

			if (PhotonNetwork.room.playerCount >= PhotonNetwork.room.maxPlayers) GoToPhase(TurnPhase.BeginGame);
			else GoToPhase(TurnPhase.Connecting);
		} else {
			// For testing from the game scene directly
			PhotonNetwork.offlineMode = true;
			PhotonNetwork.JoinRoom("offline");
			GoToPhase (TurnPhase.Connecting);
		}

		//SignalManager.PlayerFinished += OnPlayerFinished;
	}
	
	protected override void OnDestroy() {
		base.OnDestroy ();

		//SignalManager.PlayerFinished -= OnPlayerFinished;
	}

	public void QuitGame() {
		Application.LoadLevel("lobby");
		PhotonNetwork.LeaveRoom();
	}

	public void StartGameWithAI() {
		if (PhotonNetwork.room != null && !PhotonNetwork.offlineMode) {
			if (PhotonNetwork.room.playerCount >= PhotonNetwork.room.maxPlayers) {
				Debug.LogError("Can't start AI when there's another player in the room!");
				return;
				// TODO
			}
			PhotonNetwork.room.open = false;
		}
		OpponentAI = new AI();
		GoToPhase (TurnPhase.BeginGame);
	}

	// Only called by the master client
	public void InitScenario() {
		int scenarioId = -1;
		if (roomSettings != null) scenarioId = (int) roomSettings["s"];
		else scenarioId = GameSettings.InstanceOrCreate.DefaultScenarioId;

		ScenarioModel scenario = ScenarioModel.GetModel(scenarioId);
		Debug.Log ("Scenario: " + scenario);
		if (scenario is ScenarioModel1) InitScenarioA(scenario as ScenarioModel1);
		else if (scenario is ScenarioModel2) InitScenarioB(scenario as ScenarioModel2);
		else Debug.LogError("Bad scenario!");
	}

	// Called on all clients with the information from the InitScenario.
	[PunRPC]
	void SetUpScenario(Dictionary<int, bool> inPlayStatus, Dictionary<int, float> votes) {
		foreach (State state in states) {
			int id = state.Model.Id;
			bool inPlay = inPlayStatus.ContainsKey(id)? inPlayStatus[id] : false;
			float vote = votes.ContainsKey(id)? votes[id] : 0;
			state.SetUp(inPlay, vote);
		}

		UIManager.Instance.StateLabels.Refresh();
		UpdateElectoralVotes(true);
	}
	
	public void InitScenarioA(ScenarioModel1 scenario) {
		Dictionary<int, bool> inPlayStatus = new Dictionary<int, bool>();
		Dictionary<int, float> votes = new Dictionary<int, float>();
		foreach (State state in states) {
			int id = state.Model.Id;
			inPlayStatus[id] = (scenario == null || !scenario.PresetStates.Contains(id));

			float vote = 0;
			if (scenario != null) {
				int stateIndex = StateModel.Models.IndexOf(state.Model);
				if (scenario.StateLeanings.Count > stateIndex) {
					int leaning = scenario.StateLeanings[stateIndex];
					vote = InitialLeaningModel.GetModel(leaning).Value;
				}
				vote += scenario.Randomness * (Random.value * 2 - 1);
			} else {
				vote = (Random.value * 2 - 1);
			}
			votes[id] = vote;
		}
		GetComponent<PhotonView>().RPC("SetUpScenario", PhotonTargets.All, inPlayStatus, votes);
	}
	
	public void InitScenarioB(ScenarioModel2 scenario) {
		Dictionary<int, bool> inPlayStatus = new Dictionary<int, bool>();
		Dictionary<int, float> votes = new Dictionary<int, float>();

		int numStatesToAdd = 0;
		int blueStatesAdded = 0;
		int redStatesAdded = 0;
		if (scenario != null) {
			numStatesToAdd = Random.Range(scenario.MinStatesInPlay, scenario.MaxStatesInPlay + 1); // since max would be excluded
		}
		List<State> maybeBlueStates = new List<State>();
		List<State> maybeRedStates = new List<State>();
		
		foreach (State state in states) {
			int id = state.Model.Id;
			if (scenario != null) {
				int percentBlue = scenario.PercentBlue[state.Model.Id - 1];
				votes[id] = (percentBlue / 50f - 1);
				
				ScenarioModel2.InPlayStatus status = (ScenarioModel2.InPlayStatus)scenario.StatesInPlay[state.Model.Id - 1];
				if (status == ScenarioModel2.InPlayStatus.ALWAYS) {
					inPlayStatus[id] = true;
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
				inPlayStatus[id] = true;
			}
		}
		
		int r;
		State s;
		while ((blueStatesAdded + redStatesAdded) < numStatesToAdd || blueStatesAdded != redStatesAdded) {
			Debug.Log("Blue states: " + blueStatesAdded + " Red states: " + redStatesAdded + " NumStatesToAdd: " + numStatesToAdd);
			if (blueStatesAdded < redStatesAdded) {
				r = Random.Range(0, maybeBlueStates.Count);
				s = maybeBlueStates[r];
				maybeBlueStates.Remove(s);
				inPlayStatus[s.Model.Id] = true;
				blueStatesAdded ++;
			} else {
				r = Random.Range(0, maybeRedStates.Count);
				s = maybeRedStates[r];
				maybeRedStates.Remove(s);
				inPlayStatus[s.Model.Id] = true;
				redStatesAdded ++;
			}
		}
		GetComponent<PhotonView>().RPC("SetUpScenario", PhotonTargets.All, inPlayStatus, votes);
	}
	
	public void GoToPhase(TurnPhase nextState) {
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

		// Reset scenario - only one client does this to ensure the results of random choices match
		if (PhotonNetwork.isMasterClient) InitScenario();

		// Reset states
		foreach (State state in states) {
			state.ResetWorkers();
		}
		
		GoToPhase(TurnPhase.BeginWeek);
	}
	
	private void BeginWeek() {
		CurrentWeek ++;
		UpdateElectoralVotes();

		opponentIsFinished = false;
		playerIsFinished = false;

		SignalManager.BeginWeek(CurrentWeek);
		
		if (CurrentWeek >= GameSettings.InstanceOrCreate.TotalWeeks) {
			GoToPhase(TurnPhase.ElectionDay);
		} else {
			int index = Mathf.Min(CurrentWeek, GameSettings.InstanceOrCreate.Income.Length - 1);
			float income = GameSettings.InstanceOrCreate.Income[index];

			PlayerBudget.GainAmount(income);
			if (UsingAI) OpponentAI.Budget.GainAmount(income);

			GoToPhase(TurnPhase.Placement);
		}
	}
	
	private void BeginPlacement() {
		Debug.Log ("BeginPlacement. UsingAI: " + UsingAI);
		if (UsingAI) {
			OpponentAI.DoTurn();
			opponentIsFinished = true;
		}
	}
	
	private void BeginHarvest() {
		foreach (State state in states) {
			if (state.InPlay) state.PrepareToHarvest();
		}

		ObjectAccessor.Instance.HarvestTimer.StartTimer(NextHarvestAction);
	}
	
	private void FinishHarvest() {
		ObjectAccessor.Instance.HarvestTimer.StopTimer();
	}
	
	public void NextHarvestAction() {
		foreach (State state in states) {
			if (!state.InPlay) continue;
			if (state.NextHarvestAction(UsingAI)) return;
			// If that state had something to do, wait for the next cycle. Else keep looking for something to do.
		}
		
		Debug.Log("completed harvest");
		GoToPhase(TurnPhase.BeginWeek);
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
		GetComponent<PhotonView>().RPC("RpcSetPlayerFinished", PhotonTargets.All, PlayerIsBlue);
	}

	[PunRPC]
	void RpcSetPlayerFinished(bool isBlue) {
		if (isBlue ^ PlayerIsBlue) opponentIsFinished = true;
		else playerIsFinished = true;
		OnPlayerFinished();
	}

	public void OnPlayerFinished() {
		if (playerIsFinished) {
			if (!opponentIsFinished) GoToPhase(TurnPhase.Waiting);
			else if (CurrentTurnPhase != TurnPhase.Harvest) GoToPhase (TurnPhase.Harvest);
		}
	}

	public void PlayerPlaceWorker(State state) {
		if (state.PlayerCanPlaceWorker()) {
			state.AddWorker(true);
			PlayerBudget.ConsumeAmount(GameSettings.InstanceOrCreate.GetGameActionCost(GameAction.PlaceWorker));
		}
	}
	
	public void PlayerRemoveWorker(State state) {
		if (state.PlayerCanRemoveWorker()) {
			state.RemoveWorker(true);
			PlayerBudget.ConsumeAmount(GameSettings.InstanceOrCreate.GetGameActionCost(GameAction.RemoveWorker));
		}
	}
}
