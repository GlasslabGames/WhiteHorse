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
	/*
	private int playerWorkerCount = 0;
	public int PlayerWorkerCount {
		get { return playerWorkerCount; }
	}
	private int opponentWorkerCount = 0;
	public int OpponentWorkerCount {
		get { return opponentWorkerCount; }
	}
	private int targetOpponentWorkerCount = 0; // TODO
	*/

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

		playerWorkers.Clear();
		opponentWorkers.Clear();
		RedWorkerCount = BlueWorkerCount = 0;
	}

	public void SetInitialPopularVote(float v) {
		// v is between -1 (red) and 1 (blue)
		currentVote = v;
		previousVote = v;
		UpdateColor();
	}
	
	public void PrepareToHarvest() {
		countedExistingWorkers = false;
	}

	// Does the next step in the harvest sequence; returns true if we had a step to do or false if we're done
	public bool NextHarvestAction(bool usingAi) {

		if (RedWorkerCount == 0 && BlueWorkerCount == 0) {
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
			GameObject worker = AddWorker(false);
			worker.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0f), 0.5f, 2);
			UpdateVote();
			return true;
		}

		while (opponentWorkers.Count > OpponentWorkerCount) {
			RemoveWorker(false);
			UpdateVote();
			return true;
		}

		SetVote();
		return false;
	}

	// Updates the vote based on the number of workers in the state, which changes throughout the harvest process.
	public void UpdateVote() {
		Leaning prevLeaning = CurrentLeaning;
		int currentBlueWorkerCount = (GameManager.Instance.PlayerIsBlue) ? playerWorkers.Count : opponentWorkers.Count;
		int currentRedWorkerCount = (GameManager.Instance.PlayerIsBlue) ? opponentWorkers.Count : playerWorkers.Count;

		float change = (currentBlueWorkerCount - currentRedWorkerCount) * GameSettings.InstanceOrCreate.WorkerIncrement * 2;
		// we multiply by 2 so 1% change => 0.02 difference (since the vote goes from -1 to 1)

		currentVote = previousVote + change;
		Debug.Log (name + " vote: " + currentVote + ", previously: " + previousVote);
		UpdateColor(CurrentLeaning != prevLeaning);
	}

	private void SetVote() {
		currentVote = Mathf.Clamp(currentVote, -1, 1);
		previousVote = currentVote;
	}
	

	// Called by the AI
	public void IncrementOpponentWorkerCount(int amount = 1) {
		SetWorkerCount(OpponentWorkerCount + amount, !GameManager.Instance.PlayerIsBlue);
	}

	public void SetWorkerCount(int workerCount, bool isBlue) {
		Debug.Log ("Setting worker count on " + name + " to " + workerCount + " for blue? " + isBlue);
		if (isBlue) BlueWorkerCount = workerCount;
		else RedWorkerCount = workerCount;
	}

	public float GetPlayerPercentChange() {
		return playerWorkers.Count * GameSettings.InstanceOrCreate.WorkerIncrement;
	}

	public float GetOpponentPercentChange() {
		return opponentWorkers.Count * GameSettings.InstanceOrCreate.WorkerIncrement;
	}

	public void UpdateColor(bool playParticles = false) {
		if (Hidden) {
			stateColor.color = GameSettings.InstanceOrCreate.Colors.undiscoveredState;
			stateOutline.color = GameSettings.InstanceOrCreate.Colors.outline;
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
			Color c = (IsBlue)? GameSettings.InstanceOrCreate.Colors.medBlue : GameSettings.InstanceOrCreate.Colors.medRed;
			stateColor.color = Color.Lerp(GameSettings.InstanceOrCreate.Colors.neutralState, c, t);
		} else {
			stateColor.color = (InPlay)? GameSettings.InstanceOrCreate.Colors.neutralState : GameSettings.InstanceOrCreate.Colors.neutralLockedState;
		}

		// Outline color
		if (InPlay && !IsNeutral) {
			stateOutline.color = GameSettings.InstanceOrCreate.Colors.outline;
			stateOutline.sortingOrder = -6;
		} else {
			stateOutline.color = GameSettings.InstanceOrCreate.Colors.neutralOutline;
			stateOutline.sortingOrder = -7;
		}

		// Stripes
		stateStripes.enabled = !InPlay;

		if (playParticles) {
			//TODO GameObject.Instantiate(GameObjectAccessor.Instance.FlipStateParticleSystemBlue, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -0.5f), Quaternion.identity);
		}

		// show a different outline if we're highlighted
		if (highlighted) {
			stateOutline.color = GameSettings.InstanceOrCreate.Colors.highlightOutline;
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
		    GameManager.Instance.PlayerBudget.IsAmountAvailable(GameSettings.InstanceOrCreate.GetGameActionCost(GameAction.PlaceWorker)));
	}

	public bool PlayerCanRemoveWorker() {
		return (InPlay &&
		        GameManager.Instance.CurrentTurnPhase == TurnPhase.Placement &&
		        GameManager.Instance.PlayerBudget.IsAmountAvailable(GameSettings.InstanceOrCreate.GetGameActionCost(GameAction.RemoveWorker)) &&
		        playerWorkers.Count > 0);
	}
    
	public GameObject AddWorker(bool isPlayer) {
		Vector3 supporterPosition = Center + workerOffsetX + (isPlayer? workerOffsetY + (playerWorkers.Count * workerAdjacencyOffset) : -workerOffsetY + ((opponentWorkers.Count) * workerAdjacencyOffset));
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

	public void RemoveWorker(bool isPlayer) {
		// Remove the last worker
		List<GameObject> workerList = (isPlayer) ? playerWorkers : opponentWorkers;
		if (workerList.Count == 0) {
			Debug.LogError(this.name + " tried to remove a worker from an empty list!", this);
			return;
		}

		GameObject worker = workerList[workerList.Count - 1];
		if (workerList.Remove(worker)) Destroy(worker);
	}

}