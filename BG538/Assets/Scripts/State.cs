using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using DG.Tweening;

public enum Leaning {
	Neutral,
	Red,
	Blue
}

public class State : MonoBehaviour {
	public string abbreviation;

	private StateModel _model;
	public StateModel Model {
		get {
			if (_model == null) {
				_model = StateModel.GetModelByAbbreviation(abbreviation);
				if (_model == null) {
					Debug.LogError("Couldn't find a model for " + abbreviation, this);
				}
			}
			return _model;
		}
	}
	public float population;
	public int electoralVotes;

	private PhotonView _networkView;
	public PhotonView NetworkView {
		get {
			if (_networkView == null) _networkView = GetComponent<PhotonView>();
			return _networkView;
		}
	}

	private bool _inPlay;
	public bool InPlay {
		get {
			return _inPlay && !Hidden;
		}
		set {
			_inPlay = value;
		}
	}
	public bool Hidden {
		get {
			return population <= 0 || electoralVotes <= 0;
		}
	}

	// VOTE
	
	private float currentVote;  // between -1 (red) and 1 (blue)
	private float previousVote;
	public float PopularVote {
		get { return currentVote; }
	}

	public float RedSupportPercent {
		get { return Mathf.Clamp01((currentVote - 1) / -2); }
	}
	public float BlueSupportPercent {
		get { return Mathf.Clamp01((currentVote + 1) / 2); }
	}

	public float PlayerSupportPercent {
		get {
			return (GameManager.Instance.PlayerIsBlue)? BlueSupportPercent : RedSupportPercent;
		}
	}
	public float OpponentSupportPercent {
		get {
			return (GameManager.Instance.PlayerIsBlue)? RedSupportPercent : BlueSupportPercent;
		}
	}

	public Leaning CurrentLeaning {
		get {
			return GetLeaningForVote(currentVote);
		}
	}
	public bool IsBlue {
		get { return CurrentLeaning == Leaning.Blue; }
	}
	public bool IsRed {
		get { return CurrentLeaning == Leaning.Red; }
	}
	public bool IsNeutral {
		get { return CurrentLeaning == Leaning.Neutral; }
	}

	public enum Controller {
		Player,
		Opponent,
		Neutral
	}

	public int RedWorkerCount { get; private set; }
	public int BlueWorkerCount { get; private set; }

	public int PlayerWorkerCount {
		get {
			return (GameManager.Instance.PlayerIsBlue)? BlueWorkerCount : RedWorkerCount;
		}
	}
	public int OpponentWorkerCount {
		get {
			return (GameManager.Instance.PlayerIsBlue)? RedWorkerCount : BlueWorkerCount;
		}
	}

	// HARVEST
	private bool countedExistingWorkers;
	
	// DISPLAY
	private static Vector3 workerOffsetX = new Vector3(-0.4f, 0, 0);
	private static Vector3 workerOffsetY = new Vector3(0, 0.25f, 0);
	private static Vector3 workerAdjacencyOffset = new Vector3(0.2f, 0, 0);
	  
	private List< GameObject > playerWorkers = new List<GameObject>();
	private List< GameObject > opponentWorkers = new List<GameObject>();

	private List<SpriteRenderer> stateColor = new List<SpriteRenderer>();
	private List<SpriteRenderer> stateOutline = new List<SpriteRenderer>();
	private SpriteRenderer stateStripes;
	private List<SpriteRenderer> stateLabel = new List<SpriteRenderer>();

	private Transform center;
	public Vector3 Center {
		get {
			return transform.Find("uiAnchor").position; // TODO
			if (center == null) center = transform.Find("uiAnchor");
			if (center == null) center = transform;
			return center.position;
		}
	}

	private Transform uiCenter;
	public Vector3 UICenter {
		get {
			if (uiCenter == null) uiCenter = transform.Find(abbreviation + " label");
			if (uiCenter == null) uiCenter = transform.Find("uiAnchor");
			if (uiCenter == null) uiCenter = transform;
			return uiCenter.position;
		}
	}

	private bool highlighted = false;
	public static State HighlightedState = null;
    
	void Awake() {
		// automatically figure out which of the child textures are which
		foreach (SpriteRenderer t in GetComponentsInChildren<SpriteRenderer>(true)) {
			if (t.name.Contains("label") || t.name.Contains("connector")) {
				stateLabel.Add(t);
			}

			if (t.name.Contains("dashed")) {
				stateStripes = t;
			} else if (t.name.Contains("oline")) {
				stateOutline.Add(t);
			} else if (!t.name.Contains("connector")) {
				stateColor.Add(t);
			}
		}
    
		if (stateStripes == null) {
			Debug.LogError("No stripes on " + this.name, this);
		}
		if (stateOutline == null) {
			Debug.LogError("No outline on " + this.name, this);
		}
		if (stateColor == null) {
			Debug.LogError("No color on " + this.name, this);
		}

		// automatically add a button to the child with the collider so that we can get events from it
		Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
		for (var i = 0; i < colliders.Length; i++) {
			ClickableButton button = colliders[i].gameObject.AddComponent<ClickableButton>();
			button.OnClick += HandleClick;
		}
	}

	public void Start() {
		UpdateColor();
	}

	public void SetYear(YearModel yearModel) {
		population = 0;
		electoralVotes = 0;

    	if (yearModel != null) {
			int stateIndex = StateModel.Models.IndexOf(Model);
      		if (yearModel.Populations.Count > stateIndex) {
				population = yearModel.Populations[stateIndex];
			}
			if (yearModel.ElectoralCounts.Count > stateIndex) {
				electoralVotes = yearModel.ElectoralCounts[stateIndex];
			}
    	}

		UpdateColor();
  	}
    
	public void HandleClick () {
		//Debug.Log ("Click "+Model.Name);

		if (Hidden) { // don't allow clicking on hidden states, but do close the state popup if it's open
			if (UIManager.Instance != null && UIManager.Instance.statePopup != null) {
				UIManager.Instance.statePopup.Close();
			}
			return;
		}

		// Check the current phase
		TurnPhase phase = GameManager.Instance.CurrentTurnPhase;
		if (phase == TurnPhase.Placement || phase == TurnPhase.Waiting || phase == TurnPhase.GameEnd) {
			AddTelemetry();
			SdkManager.Instance.SaveTelemEvent("inspect_state", SdkManager.EventCategory.Player_Action);

			UIManager.Instance.statePopup.Show(this);
		}
	}

	public void ResetWorkers() {
		foreach (GameObject worker in playerWorkers) {
			GameObject.Destroy(worker);
		}
		foreach (GameObject worker in opponentWorkers) {
			GameObject.Destroy(worker);
		}

		playerWorkers.Clear();
		opponentWorkers.Clear();
		RedWorkerCount = BlueWorkerCount = 0;
	}

	public void SetUp(bool inPlay, float vote) {
		InPlay = inPlay;
		currentVote = vote;
		previousVote = vote;
		UpdateColor();
	}
	/*
	void SetInitialPopularVote(float v) {
		currentVote = v; // set this immediately for use in initialization
		previousVote = v;
		//NetworkView.RPC("RpcSetInitialSettings", PhotonTargets.All, currentVote, InPlay);
	}

	[PunRPC]
	void RpcSetInitialSettings(float v, bool inPlay) {
		// v is between -1 (red) and 1 (blue)
		currentVote = v;
		previousVote = v;
		InPlay = inPlay;
		UpdateColor();
	}
	*/
	
	public void PrepareToHarvest() {
		countedExistingWorkers = false;
	}

	// Does the next step in the harvest sequence; returns true if we had a step to do or false if we're done
	public bool NextHarvestAction(bool usingAi) {

		if (RedWorkerCount == 0 && BlueWorkerCount == 0 && playerWorkers.Count == 0 && opponentWorkers.Count == 0) {
			// Empty state, nothing to do here
			return false;
		}

		// Highlight existing workers and count their contribution
		if (!countedExistingWorkers) {
			countedExistingWorkers = true;
			this.Highlight();

			// Bounce existing worker sprites
			if (playerWorkers.Count > 0 || opponentWorkers.Count > 0) {
				for (var i = 0; i < playerWorkers.Count + opponentWorkers.Count; i++) {
					GameObject worker = (i < playerWorkers.Count)? playerWorkers[i] : opponentWorkers[i - playerWorkers.Count];
					if (!worker) continue; // not sure why this is happening though
					worker.transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 0.5f, 2);
				}

				UpdateVote();
				return true;
			}
		}

		// Add or remove opponent workers to match the target count
		while (opponentWorkers.Count < OpponentWorkerCount) {
			this.Highlight();
			
			// Add a new opponent worker
			GameObject worker = CreateWorker(false);
			worker.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0f), 0.5f, 2);
			UpdateVote();
			return true;
		}

		while (opponentWorkers.Count > OpponentWorkerCount) {
			DestroyWorker(false);
			UpdateVote();
			return true;
		}

		SetFinalVote();
		return false;
	}

	public float CalculateVote() {
		int currentBlueWorkerCount = (GameManager.Instance.PlayerIsBlue) ? playerWorkers.Count : opponentWorkers.Count;
		int currentRedWorkerCount = (GameManager.Instance.PlayerIsBlue) ? opponentWorkers.Count : playerWorkers.Count;
		
		float change = (currentBlueWorkerCount - currentRedWorkerCount) * GameSettings.InstanceOrCreate.WorkerIncrement * 2;
		// we multiply by 2 so 1% change => 0.02 difference (since the vote goes from -1 to 1)
		
		return previousVote + change;
	}

	// Updates the vote based on the number of workers in the state, which changes throughout the harvest process.
	public void UpdateVote() {
		Leaning prevLeaning = CurrentLeaning;
		currentVote = CalculateVote();

		bool colorChanged = (CurrentLeaning != prevLeaning);
		if (colorChanged) {
			bool success = DidControllerGetBetter(GetController(prevLeaning), GetController());

			AddTelemetry(false);
			SdkManager.Instance.AddTelemEventValue("prev_controller", GetController(prevLeaning));
			SdkManager.Instance.SaveTelemEvent("state_control_changed", success, SdkManager.EventCategory.System_Event);
		}
		UpdateColor(colorChanged);
	}

	private void SetFinalVote() {
		currentVote = Mathf.Clamp(currentVote, -1, 1);
		previousVote = currentVote;
	}

	public float GetPlayerPercentChange() {
		return playerWorkers.Count * GameSettings.InstanceOrCreate.WorkerIncrement;
	}

	public float GetOpponentPercentChange() {
		return opponentWorkers.Count * GameSettings.InstanceOrCreate.WorkerIncrement;
	}

	public void UpdateColor(bool playParticles = false) {
		if (Hidden) {
			for (var i = 0; i < stateColor.Count; i++) {
				stateColor[i].color = GameSettings.InstanceOrCreate.Colors.undiscoveredState;
			}
			for (var j = 0; j < stateOutline.Count; j++) {
				stateOutline[j].color = GameSettings.InstanceOrCreate.Colors.outline;
			}
			for (var k = 0; k < stateLabel.Count; k++) {
				stateLabel[k].enabled = false;
			}
			stateStripes.enabled = true;
			return;
		}

		// State color
		Color c;
		if (!IsNeutral) {
			float t = 1;
			if (InPlay) {
				t = Mathf.InverseLerp(0.5f, 1f, (IsBlue) ? BlueSupportPercent : RedSupportPercent); // 0.5 -> 0, 1 -> 1
				t = Mathf.Lerp(0.2f, 1f, t); // 0 -> 0.2, 1 -> 1 (Start at 0.2 so we don't go all the way to the neutral color.)
			}
			c = (IsBlue)? GameSettings.InstanceOrCreate.Colors.medBlue : GameSettings.InstanceOrCreate.Colors.medRed;
			c = Color.Lerp(GameSettings.InstanceOrCreate.Colors.neutralState, c, t);
		} else {
			c = (InPlay)? GameSettings.InstanceOrCreate.Colors.neutralState : GameSettings.InstanceOrCreate.Colors.neutralLockedState;
		}
		for (var i = 0; i < stateColor.Count; i++) {
			stateColor[i].color = c;
		}

		// Outline color
		for (var j = 0; j < stateOutline.Count; j++) {
			if (highlighted) {
				stateOutline[j].color = GameSettings.InstanceOrCreate.Colors.highlightOutline;
				stateOutline[j].sortingOrder = -5;
			} else if (InPlay && !IsNeutral) {
				stateOutline[j].color = GameSettings.InstanceOrCreate.Colors.outline;
				stateOutline[j].sortingOrder = -6;
			} else {
				stateOutline[j].color = GameSettings.InstanceOrCreate.Colors.neutralOutline;
				stateOutline[j].sortingOrder = -7;
			}
		}
	
		// Stripes
		stateStripes.enabled = !InPlay;

		// Label
		for (var k = 0; k < stateLabel.Count; k++) {
			stateLabel[k].enabled = InPlay;
		}

		if (playParticles) {
			//TODO GameObject.Instantiate(GameObjectAccessor.Instance.FlipStateParticleSystemBlue, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -0.5f), Quaternion.identity);
		}
	}
    
	public void Highlight() {
		if (State.HighlightedState != null) State.HighlightedState.UnHighlight();

		highlighted = true;
		UpdateColor();
		State.HighlightedState = this;
	}

	public void UnHighlight() {
		highlighted = false;
		UpdateColor();
		if (State.HighlightedState == this) {
			State.HighlightedState = null;
		}
	}

	public bool PlayerCanPlaceWorker() {
		return (InPlay &&
			GameManager.Instance.CurrentTurnPhase == TurnPhase.Placement &&
		    GameManager.Instance.PlayerBudget.IsAmountAvailable(GameSettings.InstanceOrCreate.GetGameActionCost(GameAction.PlaceWorker)));
	}

	public bool PlayerCanRemoveWorker() {
		return (InPlay &&
		        GameManager.Instance.CurrentTurnPhase == TurnPhase.Placement &&
		        GameManager.Instance.PlayerBudget.IsAmountAvailable(GameSettings.InstanceOrCreate.GetGameActionCost(GameAction.RemoveWorker)) &&
		        playerWorkers.Count > 0);
	}

	public void AddWorker(bool isPlayer) {
		// Store the previously predicted controller for telemetry purposes
		Controller prevPredictedController = GetPredictedController();

		SetWorkerCount( (isPlayer? PlayerWorkerCount : OpponentWorkerCount) + 1, isPlayer);

		if (isPlayer) {
			CreateWorker(true);

			// We only send the telemetry when the player adds a worker (not if it's from the opponent/AI)
			bool success = DidControllerGetBetter(prevPredictedController, GetPredictedController());
			AddTelemetry();
			SdkManager.Instance.AddTelemEventValue("prev_predicted_controller", System.Enum.GetName(typeof(Controller), prevPredictedController));
			SdkManager.Instance.SaveTelemEvent("place_worker", success, SdkManager.EventCategory.Player_Action);
		}
	}

	public void RemoveWorker(bool isPlayer) {		
		// Store the previously predicted controller for telemetry purposes
		Controller prevPredictedController = GetPredictedController();

		SetWorkerCount( (isPlayer? PlayerWorkerCount : OpponentWorkerCount) - 1, isPlayer);
		if (isPlayer) {
			DestroyWorker(true);

			// We only send the telemetry when the player adds a worker (not if it's from the opponent/AI)
			bool failure = DidControllerGetWorse(prevPredictedController, GetPredictedController());
			AddTelemetry();
			SdkManager.Instance.AddTelemEventValue("prev_predicted_controller", System.Enum.GetName(typeof(Controller), prevPredictedController));
			SdkManager.Instance.SaveTelemEvent("remove_worker", !failure, SdkManager.EventCategory.Player_Action);
		}
	}
	
	private void SetWorkerCount(int workerCount, bool isPlayer) {
		bool isBlue = isPlayer ^ !GameManager.Instance.PlayerIsBlue;
		NetworkView.RPC("RpcSetWorkerCount", PhotonTargets.All, workerCount, isBlue);
	}
	
	[PunRPC]
	private void RpcSetWorkerCount(int workerCount, bool isBlue) {
		Debug.Log ("Setting worker count on " + name + " to " + workerCount + " for blue? " + isBlue);
		if (isBlue) BlueWorkerCount = workerCount;
		else RedWorkerCount = workerCount;
	}
    
	private GameObject CreateWorker(bool isPlayer) {
		Vector3 supporterPosition = UICenter + workerOffsetX + (isPlayer? workerOffsetY + (playerWorkers.Count * workerAdjacencyOffset) : -workerOffsetY + ((opponentWorkers.Count) * workerAdjacencyOffset));
		if (!isPlayer) supporterPosition.x += workerAdjacencyOffset.x / 2f;

		GameObject newWorker = GameObject.Instantiate(ObjectAccessor.Instance.WorkerPrefab, supporterPosition, Quaternion.identity) as GameObject;

		if (isPlayer ^ GameManager.Instance.PlayerIsBlue) {
			newWorker.GetComponent<SpriteRenderer>().color = GameSettings.InstanceOrCreate.Colors.darkRed;
		} else {
			newWorker.GetComponent<SpriteRenderer>().color = GameSettings.InstanceOrCreate.Colors.darkBlue;
		}

		if (isPlayer) playerWorkers.Add(newWorker);
		else opponentWorkers.Add(newWorker);

		return newWorker;
	}

	private void DestroyWorker(bool isPlayer) {
		// Remove the last worker
		List<GameObject> workerList = (isPlayer) ? playerWorkers : opponentWorkers;
		if (workerList.Count == 0) {
			Debug.LogError(this.name + " tried to remove a worker from an empty list!", this);
			return;
		}

		GameObject worker = workerList[workerList.Count - 1];
		if (workerList.Remove(worker)) Destroy(worker);
	}

	public static Leaning GetLeaningForVote(float vote) {
		if (vote < 0) return Leaning.Red;
		else if (vote > 0) return Leaning.Blue;
		else return Leaning.Neutral;
	}

	public Leaning GetPredictedLeaning() {
		var predictedVote = CalculateVote();
		return GetLeaningForVote(predictedVote);
	}

	public Controller GetController() {
		return GetController(CurrentLeaning);
	}

	// Prediction of what the state's color will be
	public Controller GetPredictedController() {
		return GetController(GetPredictedLeaning());
	}

	public static Controller GetController(float vote) {
		return GetController(GetLeaningForVote(vote));
	}

	public static Controller GetController(Leaning leaning) {
		if (leaning == Leaning.Blue) return (GameManager.Instance.PlayerIsBlue)? Controller.Player : Controller.Opponent;
		else if (leaning == Leaning.Red) return (GameManager.Instance.PlayerIsBlue)? Controller.Opponent : Controller.Player;
		else return Controller.Neutral;
	}

	public static bool DidControllerGetBetter(Controller prevController, Controller newController) {
		return (prevController == Controller.Opponent && newController != Controller.Opponent)
			|| (prevController != Controller.Player && newController == Controller.Player);
		// Success = no longer under opponent control, or newly under player controller
	}
	
	public static bool DidControllerGetWorse(Controller prevController, Controller newController) {
		return (prevController == Controller.Player && newController != Controller.Player)
			|| (prevController != Controller.Opponent && newController == Controller.Opponent);
		// Failure = no longer under player control, or newly under opponent control
	}

	void AddTelemetry(bool includePredictedController = true) {
		SdkManager.Instance.AddTelemEventValue("state", Model.Abbreviation);
		SdkManager.Instance.AddTelemEventValue("player_popular_opinion", PlayerSupportPercent);
		SdkManager.Instance.AddTelemEventValue("player_workers", playerWorkers.Count);
		SdkManager.Instance.AddTelemEventValue("opponent_workers", opponentWorkers.Count);
		SdkManager.Instance.AddTelemEventValue("controller", System.Enum.GetName(typeof(Controller), GetController()));

		if (includePredictedController) SdkManager.Instance.AddTelemEventValue("predicted_controller", System.Enum.GetName(typeof(Controller), GetPredictedController()));
	}

}