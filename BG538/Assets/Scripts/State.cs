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

	private StateModel model;
	public StateModel Model {
		get {
			if (model == null) {
				model = StateModel.GetModelByAbbreviation(abbreviation);
				if (model == null) {
					Debug.LogError("Couldn't find a model for " + abbreviation, this);
				}
			}
			return model;
		}
	}

	public bool InPlay { get; set; }
	public bool Hidden { get; set; }

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
			if (currentVote < 0) return Leaning.Red;
			else if (currentVote > 0) return Leaning.Blue;
			else return Leaning.Neutral;
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
	
	// WORKERS
	private int playerWorkerCount = 0;
	public int PlayerWorkerCount {
		get { return playerWorkerCount; }
	}
	private int opponentWorkerCount = 0;
	public int OpponentWorkerCount {
		get { return opponentWorkerCount; }
	}
	private int targetOpponentWorkerCount = 0;

	private int redWorkerCount {
		get {
			return (GameManager.Instance.PlayerIsBlue)? opponentWorkerCount : playerWorkerCount;
		}
	}
	private int blueWorkerCount {
		get {
			return (GameManager.Instance.PlayerIsBlue)? playerWorkerCount : opponentWorkerCount;
		}
	}

	// HARVEST
	private bool sentInfoToOpponent;
	private bool receivedInfoFromOpponent;
	private bool countedExistingWorkers;
	
	// DISPLAY
	private static Vector3 workerOffsetX = new Vector3(-0.4f, 0, 0);
	private static Vector3 workerOffsetY = new Vector3(0, 0.25f, 0);
	private static Vector3 workerAdjacencyOffset = new Vector3(0.2f, 0, 0);
	  
	private List< GameObject > playerWorkers = new List<GameObject>();
	private List< GameObject > opponentWorkers = new List<GameObject>();

	public int RoundedPopulation {
		get { return Mathf.CeilToInt(Model.Population); }
	}

	private SpriteRenderer stateColor;
	private SpriteRenderer stateOutline;
	private SpriteRenderer stateStripes;
	private Transform center;

	public Vector3 Center {
		get {
			if (center == null) {
				center = transform.Find("uiAnchor");
				if (center == null) {
					center = transform;
				}
			}
			return center.position;
		}
	}

	private bool highlighted = false;
	public static State HighlightedState = null;
    
	void Awake() {
		// automatically figure out which of the child textures are which
		foreach (SpriteRenderer t in GetComponentsInChildren<SpriteRenderer>(true)) {
			if (t.name.Contains("dashed")) {
				stateStripes = t;
			} else if (t.name.Contains("oline")) {
				stateOutline = t;
			} else {
				stateColor = t;
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
		Collider2D c = GetComponentInChildren<Collider2D>();
		if (c == null) {
			Debug.LogError("Couldn't find collider under " + this, this);
		} else {
			ClickableButton button = c.gameObject.AddComponent<ClickableButton>();
			button.OnClick += HandleClick;
		}
	}

	public void Start() {
		UpdateColor();
	}
    
	public void HandleClick () {
		Debug.Log ("Click "+Model.Name);

		// Check the current phase
		TurnPhase phase = GameManager.Instance.CurrentTurnPhase;
		if (phase == TurnPhase.Placement || phase == TurnPhase.Waiting || phase == TurnPhase.ElectionDay) {
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

		playerWorkerCount = opponentWorkerCount = targetOpponentWorkerCount = 0;
	}

	public void SetInitialPopularVote(float v) {
		// v is between -1 (red) and 1 (blue)
		currentVote = v;
		previousVote = v;
		UpdateColor();
	}
	
	public void PrepareToHarvest() {
		sentInfoToOpponent = false;
		receivedInfoFromOpponent = false;
		countedExistingWorkers = false;
	}

	// Does the next step in the harvest sequence; returns true if we had a step to do or false if we're done
	public bool NextHarvestAction(bool usingAi) {
		// First, make sure we send and get info from the opponent
		if (!sentInfoToOpponent) {
			if (usingAi) sentInfoToOpponent = true;
			else SendInfoToOpponent();
		}

		if (!receivedInfoFromOpponent) {
			if (usingAi) receivedInfoFromOpponent = true;
			else return true; // wait until we get the info
		}

		if (playerWorkerCount == 0 && opponentWorkerCount == 0 && targetOpponentWorkerCount == 0) {
			// Nothing to do here
			return false;
		}

		// Next highlight existing workers and count their contribution
		if (!countedExistingWorkers) {
			countedExistingWorkers = true;
			this.Highlight();
			
			if (playerWorkerCount > 0 || opponentWorkerCount > 0) {
				int playerWorkersLength = playerWorkers.Count; // should be the same as playerWorkerCount just in case
				for (var i = 0; i < playerWorkersLength + opponentWorkers.Count; i++) {
					GameObject worker = (i < playerWorkersLength)? playerWorkers[i] : opponentWorkers[i - playerWorkersLength];
					worker.transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 0.5f, 2);
				}

				UpdateVote();
				return true;
			}
		}

		if (targetOpponentWorkerCount > opponentWorkerCount) {
			this.Highlight();

			// Add a new opponent worker
			GameObject worker = CreateWorkerPrefab(false);
			worker.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0f), 0.5f, 2);
			UpdateVote();
			return true;
		}

		SetVote();
		return false;
	}

	public void UpdateVote() {
		Leaning prevLeaning = CurrentLeaning;
		float change = (blueWorkerCount - redWorkerCount) * GameSettings.Instance.WorkerIncrement * 2;
		// we multiply by 2 so 1% change => 0.02 difference (since the vote goes from -1 to 1)
		currentVote = previousVote + change;
		Debug.Log (Model.Abbreviation + " vote: " + currentVote + ", previously: " + previousVote);
		UpdateColor(CurrentLeaning != prevLeaning);
	}

	private void SetVote() {
		currentVote = Mathf.Clamp(currentVote, -1, 1);
		previousVote = currentVote;
	}

	public void SendInfoToOpponent() {
		GetComponent<NetworkView>().RPC("RecieveInfoFromOpponent", RPCMode.Others, playerWorkerCount);

		sentInfoToOpponent = true;
	}

	[RPC]
	public void RecieveInfoFromOpponent(int workerCount) {
		targetOpponentWorkerCount = workerCount;
		receivedInfoFromOpponent = true;
	}

	// Called by the AI
	public void IncrementOpponentWorkerCount(int amount = 1) {
		targetOpponentWorkerCount += amount;
//		Debug.Log(abbreviation + " added worker from AI. New count: " + targetOpponentWorkerCount);
	}
		
	public float GetPlayerPercentChange() {
		return playerWorkerCount * GameSettings.Instance.WorkerIncrement;
	}

	public float GetOpponentPercentChange() {
		return opponentWorkerCount * GameSettings.Instance.WorkerIncrement;
	}

	public void UpdateColor(bool playParticles = false) {
		if (Hidden) {
			stateColor.color = GameSettings.Instance.Colors.undiscoveredState;
			stateOutline.color = GameSettings.Instance.Colors.outline;
			stateStripes.enabled = true;
			return;
		}

		// State color
		if (!IsNeutral) {
			float t = 1;
			if (InPlay) {
				t = Mathf.InverseLerp(0.5f, 1f, (IsBlue) ? BlueSupportPercent : RedSupportPercent); // 0.5 -> 0, 1 -> 1
				t = Mathf.Lerp(0.2f, 1f, t); // 0 -> 0.2, 1 -> 1 (Start at 0.2 so we don't go all the way to the neutral color.)
			}
			Color c = (IsBlue)? GameSettings.Instance.Colors.medBlue : GameSettings.Instance.Colors.medRed;
			stateColor.color = Color.Lerp(GameSettings.Instance.Colors.neutralState, c, t);
		} else {
			stateColor.color = (InPlay)? GameSettings.Instance.Colors.neutralState : GameSettings.Instance.Colors.neutralLockedState;
		}

		// Outline color
		if (InPlay && !IsNeutral) {
			stateOutline.color = GameSettings.Instance.Colors.outline;
			stateOutline.sortingOrder = -6;
		} else {
			stateOutline.color = GameSettings.Instance.Colors.neutralOutline;
			stateOutline.sortingOrder = -7;
		}

		// Stripes
		stateStripes.enabled = !InPlay;

		if (playParticles) {
			//TODO GameObject.Instantiate(GameObjectAccessor.Instance.FlipStateParticleSystemBlue, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -0.5f), Quaternion.identity);
		}

		// show a different outline if we're highlighted
		if (highlighted) {
			stateOutline.color = GameSettings.Instance.Colors.highlightOutline;
			stateOutline.sortingOrder = -5;
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
		    GameManager.Instance.PlayerBudget.IsAmountAvailable(GameSettings.Instance.GetGameActionCost(GameAction.PlaceWorker)));
	}
        
	public void PlayerPlaceWorker() {    
		if (PlayerCanPlaceWorker()) {
			CreateWorkerPrefab(true);

			GameManager.Instance.PlayerBudget.ConsumeAmount(GameSettings.Instance.GetGameActionCost(GameAction.PlaceWorker));
		}
	}

	public bool PlayerCanRemoveWorker() {
		return (InPlay &&
		        GameManager.Instance.CurrentTurnPhase == TurnPhase.Placement &&
		        GameManager.Instance.PlayerBudget.IsAmountAvailable(GameSettings.Instance.GetGameActionCost(GameAction.RemoveWorker)) &&
		        playerWorkers.Count > 0);
	}

	public void PlayerRemoveWorker() {
		if (PlayerCanRemoveWorker()) {

			// Remove the last supporter
			GameObject supporter = playerWorkers[playerWorkers.Count - 1];
			playerWorkers.Remove(supporter);
			Destroy(supporter);
			playerWorkerCount --;
        
			GameManager.Instance.PlayerBudget.ConsumeAmount(GameSettings.Instance.GetGameActionCost(GameAction.RemoveWorker));
     	}
	}
    
	public GameObject CreateWorkerPrefab(bool isPlayer) {
		Vector3 supporterPosition = Center + workerOffsetX + (isPlayer? workerOffsetY + (playerWorkers.Count * workerAdjacencyOffset) : -workerOffsetY + ((opponentWorkers.Count) * workerAdjacencyOffset));
		if (!isPlayer) supporterPosition.x += workerAdjacencyOffset.x / 2f;

		GameObject newSupporter = GameObject.Instantiate(ObjectAccessor.Instance.WorkerPrefab, supporterPosition, Quaternion.identity) as GameObject;

		if (isPlayer ^ GameManager.Instance.PlayerIsBlue) {
			newSupporter.GetComponent<SpriteRenderer>().color = GameSettings.Instance.Colors.darkRed;
		} else {
			newSupporter.GetComponent<SpriteRenderer>().color = GameSettings.Instance.Colors.darkBlue;
		}

		if (isPlayer) {
			playerWorkers.Add(newSupporter);
			playerWorkerCount ++;
		} else {
			opponentWorkers.Add(newSupporter);
			opponentWorkerCount ++;
		}

		return newSupporter;
	}

}