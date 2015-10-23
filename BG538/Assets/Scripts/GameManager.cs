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

	private Dictionary<string, object> gameSettings;

	protected override void Start() {
		base.Start ();

		foreach (State state in ObjectAccessor.Instance.StatesContainer.GetComponentsInChildren<State>()) {
			states.Add(state);
		}

		gameSettings = GameSettings.InstanceOrCreate.CurrentOptions;

		Debug.Log (">> "+GameSettings.InstanceOrCreate.CurrentOptions["scenarioId"]); // TODO

		if (gameSettings.ContainsKey("color")) {
			PlayerIsBlue = ((Leaning) gameSettings["color"] == Leaning.Blue) ^ !PhotonNetwork.isMasterClient;
		}
		if (SignalManager.PlayerColorSet != null) SignalManager.PlayerColorSet();

		if (PhotonNetwork.connected) {
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
		if (gameSettings != null && gameSettings.ContainsKey("scenarioId")) scenarioId = (int) gameSettings["scenarioId"];
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

		// Make sure we get the initial votes. I don't know why it doesn't work correctly without this hack. // FIXME
		Invoke("InitialVoteUpdate", 1f);
	}

	void InitialVoteUpdate() {
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
					if (votes[id] > 0) { // is blue
						blueStatesAdded ++;
					} else {
						redStatesAdded ++;
					}
				} else if (status == ScenarioModel2.InPlayStatus.MAYBE) {
					if (votes[id] > 0) { // is blue
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
			Debug.Log("Blue states: " + blueStatesAdded + " Red states: " + redStatesAdded + " NumStatesToAdd: " + numStatesToAdd
			          + " Maybe blue states: "+maybeBlueStates.Count + " MaybeRedStates: " + maybeRedStates.Count);
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
				totalRedVotes += state.Model.ElectoralCount;
			} else if (state.IsBlue) {
				totalBlueVotes += state.Model.ElectoralCount;
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
			totalVotes += state.Model.Population;
			playerVotes += state.PlayerSupportPercent * state.Model.Population;
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

	void AddGameTelemetry() {
		SdkManager.Instance.AddTelemEventValue("game_id", PhotonNetwork.room.name);
		
		// TODO: opponent_user_id
		
		string scenarioName = "";
		if (gameSettings != null && gameSettings.ContainsKey("scenarioId")) { // should have been stored when we entered the room
			int scenarioId = (int)gameSettings["scenarioId"];
			ScenarioModel scenario = ScenarioModel.GetModel(scenarioId);
			if (scenario != null) scenarioName = scenario.Name;
		}
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
