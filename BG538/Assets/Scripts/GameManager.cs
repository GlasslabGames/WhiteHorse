using UnityEngine;
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
	GameEnd,
	Disconnected
}

public class GameManager : SingletonBehavior<GameManager> {
	private bool gameIsActive;

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

	private int prevVoteDifference;

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

	// Store the last scenario we generated so we can replay it
	Dictionary<int, bool> inPlayStatus = new Dictionary<int, bool>();	Dictionary<int, float> votes = new Dictionary<int, float>();

	protected override void Start() {
		base.Start ();

		foreach (State state in ObjectAccessor.Instance.StatesContainer.GetComponentsInChildren<State>()) {
			states.Add(state);
		}

		// Handle the case where we're playing this scene from the editor - just start in offline mode
		// Note, this will reload the game scene, so it's only used for testing purposes
		if (!PhotonNetwork.inRoom) NetworkManager.StartOfflineGame();

		PlayerIsBlue = (GameSettings.InstanceOrCreate.currentColor == Leaning.Blue) ^ !PhotonNetwork.isMasterClient;
		if (SignalManager.PlayerColorSet != null) SignalManager.PlayerColorSet();

		if (PhotonNetwork.inRoom && !PhotonNetwork.offlineMode) {
			if (PhotonNetwork.room.playerCount >= PhotonNetwork.room.maxPlayers) GoToPhase(TurnPhase.BeginGame);
			else GoToPhase(TurnPhase.Connecting);
		} else {
			StartGameWithAI();
		}

		//SignalManager.PlayerFinished += OnPlayerFinished;
	}
	
	protected override void OnDestroy() {
		base.OnDestroy ();

		//SignalManager.PlayerFinished -= OnPlayerFinished;
	}

	public void QuitGame() {
		SdkManager.Instance.SaveTelemEvent("quit_game", SdkManager.EventCategory.Player_Action);

		this.OnEndGame(false); // Send telemetry indicating that the game ended (unless we already ended the game)

		Application.LoadLevel("lobby");
		PhotonNetwork.LeaveRoom();
	}

	public void StartGameWithAI() {
		Debug.Log ("StartGameWithAI");
		if (PhotonNetwork.inRoom && !PhotonNetwork.offlineMode) {
			if (PhotonNetwork.room.playerCount >= PhotonNetwork.room.maxPlayers) {
				Debug.LogError("Can't start AI when there's another player in the room!");
				return;
			}
			PhotonNetwork.room.open = false;
		}
		OpponentAI = new AI();
		GoToPhase (TurnPhase.BeginGame);
	}

	// Only called by the master client
	public void InitScenario(ScenarioModel scenario) {
		if (scenario is HistoricalScenarioModel) InitHistoricalScenario(scenario as HistoricalScenarioModel);
		else if (scenario is RandomScenarioModel) InitRandomScenario(scenario as RandomScenarioModel);
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

		UpdateElectoralVotes(true);

		// Make sure we get the initial votes. I don't know why it doesn't work correctly without this hack. // FIXME
		Invoke("InitialVoteUpdate", 1f);
	}

	void InitialVoteUpdate() {
		UpdateElectoralVotes(true);
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
			StartGame(false);
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
		case TurnPhase.GameEnd:
			OnEndGame(true);
			break;
		case TurnPhase.Disconnected:
			SdkManager.Instance.SaveTelemEvent("disconnected", SdkManager.EventCategory.System_Event);
			OnEndGame(false);
			break;
		}
		SignalManager.EnterTurnPhase (CurrentTurnPhase);
	}

	void OnBeginGame() {
		// Send telemetry about the game
		AddGameTelemetry();
		SdkManager.Instance.SaveTelemEvent("game_started", SdkManager.EventCategory.Unit_Start);
		gameIsActive = true;
	}

	void OnEndGame(bool gameComplete) {
		if (!gameIsActive) return; // ensure there are not duplicate game_ended events

		AddGameTelemetry();
		AddWeekTelemetry(false);
		SdkManager.Instance.AddTelemEventValue("game_complete", gameComplete);

		SdkManager.Instance.SaveTelemEvent("game_ended", PlayerIsWinning, SdkManager.EventCategory.Unit_End);

		gameIsActive = false;
	}

	public void Replay() {
		GetComponent<PhotonView>().RPC("RestartGame", PhotonTargets.All);
	}

	[PunRPC]
	public void RestartGame() {
		StartGame(true);
	}

	public void StartGame(bool replay) {
		Debug.Log("Start game!");
		
		CurrentWeek = -1;
		
		// Reset budget
		PlayerBudget.Reset();

		if (UsingAI) OpponentAI.Reset();
			
		// Set the correct year
		int scenarioId = GameSettings.InstanceOrCreate.ScenarioId;
		ScenarioModel scenario = ScenarioModel.GetModel(scenarioId);
		YearModel currentYear = YearModel.GetModel(scenario.Year);
		
		// Reset states
		foreach (State state in states) {
			state.SetYear(currentYear);
			state.ResetWorkers();
		}

		// Reset scenario - only one client does this to ensure the results of random choices match
		// Note that it's important to set the year before initializing the scenario
		if (!PhotonNetwork.inRoom || PhotonNetwork.isMasterClient) {
			if (!replay) InitScenario(scenario);
			GetComponent<PhotonView>().RPC("SetUpScenario", PhotonTargets.All, inPlayStatus, votes);
		}

		UIManager.Instance.StateLabels.Refresh(); // refresh state labels with new year info

		OnBeginGame();

		GoToPhase(TurnPhase.BeginWeek);
	}
	
	private void BeginWeek() {
		CurrentWeek ++;

		opponentIsFinished = false;
		playerIsFinished = false;

		SignalManager.BeginWeek(CurrentWeek);
		
		if (CurrentWeek >= GameSettings.InstanceOrCreate.TotalWeeks) {
			GoToPhase(TurnPhase.GameEnd);
		} else {
			int index = Mathf.Min(CurrentWeek, GameSettings.InstanceOrCreate.Income.Length - 1);
			float income = GameSettings.InstanceOrCreate.Income[index];

			PlayerBudget.GainAmount(income);
			if (UsingAI) OpponentAI.Budget.GainAmount(income);

			// Telemetry
			AddWeekTelemetry(false);
			SdkManager.Instance.SaveTelemEvent("turn_started", SdkManager.EventCategory.Unit_Start);
			prevVoteDifference = playerVotes - opponentVotes; // used to check for improvement during the turn

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

		UpdateElectoralVotes();

		int voteDifference = playerVotes - opponentVotes;
		bool success = (voteDifference > prevVoteDifference);
		AddWeekTelemetry();
		SdkManager.Instance.SaveTelemEvent("turn_ended", success, SdkManager.EventCategory.Unit_End);
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
				totalRedVotes += state.electoralVotes;
			} else if (state.IsBlue) {
				totalBlueVotes += state.electoralVotes;
			}
		}

		Debug.Log ("!! Update electoral votes. States: "+states.Count+" Red votes: "+totalRedVotes);
		
		playerVotes = (PlayerIsBlue)? totalBlueVotes : totalRedVotes;
		opponentVotes = (PlayerIsBlue)? totalRedVotes : totalBlueVotes;

		if (SignalManager.PlayerVotesChanged != null) SignalManager.PlayerVotesChanged(playerVotes, !atBeginning);
		if (SignalManager.OpponentVotesChanged != null) SignalManager.OpponentVotesChanged(opponentVotes, !atBeginning);
	}

	public float GetPopularOpinion() {
		float playerVotes = 0;
		float totalVotes = 0;
		foreach (State state in states) {
			totalVotes += state.population;
			playerVotes += state.PlayerSupportPercent * state.population;
		}

		return playerVotes / totalVotes;
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

	public void InitHistoricalScenario(HistoricalScenarioModel scenario) {
		inPlayStatus.Clear();
		votes.Clear();
		
		foreach (State state in states) {
			int id = state.Model.Id;
			inPlayStatus[id] = (scenario == null || scenario.UnlockedStates.Contains(id));
			
			float vote = 0;
			if (scenario != null && (!scenario.UnlockedStatesAreNeutral || !inPlayStatus[id])) {
				int stateIndex = StateModel.Models.IndexOf(state.Model);
				if (scenario.PercentBlue.Count > stateIndex) {
					int percentBlue = scenario.PercentBlue[stateIndex];
					vote = (percentBlue / 50f - 1); // convert to number btw 1 and -1
				}
			}
			votes[id] = vote;
		}
	}

	public void InitRandomScenario(RandomScenarioModel scenario) {
		inPlayStatus.Clear();
		votes.Clear();

		State state;
		List<State> unlockedStates = new List<State>();
		List<State> lockedStates = new List<State>(states); // start with all states locked
		int statesToUnlock = (scenario != null)? scenario.NumberOfStatesToUnlock : 50;
		while (unlockedStates.Count < statesToUnlock) {
			if (lockedStates.Count <= 0) break; // oops

			int r = Random.Range(0, lockedStates.Count);
			state = lockedStates[r];
			unlockedStates.Add(state);
			lockedStates.RemoveAt(r);

			inPlayStatus[state.Model.Id] = true;
		}
	
		// Now randomly assign votes for each state in groups, so that we won't be stuck with a huge disparity
		List<State> unassignedBigStates = new List<State>();
		List<State> unassignedSmallStates = new List<State>();
		List<State> unassignedTinyStates = new List<State>();

		// We assign votes for locked states only if UnlockedStates is true; else all states.
		bool lockedStatesOnly = (scenario != null && scenario.UnlockedStatesAreNeutral);
    	List<State> statesToAssign = (lockedStatesOnly)? lockedStates : states;

		for (var i = 0; i < statesToAssign.Count; i++) {
			state = statesToAssign[i];
			if (state.electoralVotes >= 20) unassignedBigStates.Add(state);
			else if (state.electoralVotes <= 3) unassignedTinyStates.Add(state);
			else unassignedSmallStates.Add(state);
		}
		
		// Assign big states first so that we don't end up adding one at the end and throwing the balance off
		float voteTotal = 0; // track if we're leaning more red or blue
    	voteTotal = AssignRandomStateVotes(unassignedBigStates, voteTotal);
    	voteTotal = AssignRandomStateVotes(unassignedSmallStates, voteTotal);
		voteTotal = AssignRandomStateVotes(unassignedTinyStates, voteTotal);
  	}

	float AssignRandomStateVotes(List<State> states, float currentVoteTotal) {
		while (states.Count > 0) {
			int r = Random.Range(0, states.Count);
			State state = states[r];
			int id = state.Model.Id;
			states.RemoveAt(r);
			
			float vote = Random.value; // btw 0 and 1
			if (currentVoteTotal > 0) vote *= -1; // we're leaning blue, so make this vote red (negative)
			currentVoteTotal += state.electoralVotes * Mathf.Sign(vote);
			votes[id] = vote;
    	}
		return currentVoteTotal; // so we can use it for the next section
  	}

	void AddGameTelemetry() {
		string roomName = (PhotonNetwork.inRoom)? PhotonNetwork.room.name : "offline";
		SdkManager.Instance.AddTelemEventValue("game_id", roomName);
		
		// TODO: opponent_user_id
		
		int scenarioId = GameSettings.InstanceOrCreate.ScenarioId;
		ScenarioModel scenario = ScenarioModel.GetModel(scenarioId);
		string scenarioName = (scenario != null)? scenario.Name : "none";
		SdkManager.Instance.AddTelemEventValue("scenario", scenarioName);
		
		SdkManager.Instance.AddTelemEventValue("player_color", (PlayerIsBlue) ? "blue" : "red");
		SdkManager.Instance.AddTelemEventValue("using_ai", UsingAI);
	}
	
	void AddWeekTelemetry(bool includeMoney = true) {
		SdkManager.Instance.AddTelemEventValue("player_votes", playerVotes);
		SdkManager.Instance.AddTelemEventValue("opponent_votes", opponentVotes);
		SdkManager.Instance.AddTelemEventValue("player_popular_percent", GetPopularOpinion());
		SdkManager.Instance.AddTelemEventValue("turn", CurrentWeek);
		
		if (includeMoney) {
			SdkManager.Instance.AddTelemEventValue("money", PlayerBudget.Amount);
		}
	}
}
