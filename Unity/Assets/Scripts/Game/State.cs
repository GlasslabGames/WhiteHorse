using UnityEngine;
using System;
using System.Collections.Generic;

public enum Leaning {
	Neutral,
	Red,
	Blue
}

public class State : MonoBehaviour {
	public string m_abbreviation;

	private StateModel m_model;
	public StateModel Model {
		get {
			if (m_model == null) {
				m_model = StateModel.GetModelByAbbreviation(m_abbreviation);
				if (m_model == null) {
					Debug.LogError("Couldn't find a model for " + m_abbreviation, this);
				}
			}
			return m_model;
		}
	}

	public bool InPlay { get; set; }
	public bool Hidden { get; set; }

	// VOTE
	
	private float m_currentVote;  // between -1 (red) and 1 (blue)
	private float m_previousVote;
	public float PopularVote {
		get { return m_currentVote; }
	}

	public float RedSupportPercent {
		get { return Mathf.Clamp01((m_currentVote - 1) / -2); }
	}
	public float BlueSupportPercent {
		get { return Mathf.Clamp01((m_currentVote + 1) / 2); }
	}

	public float PlayerSupportPercent {
		get {
			if (GameObjectAccessor.Instance.Player.IsRed) return RedSupportPercent;
			else return BlueSupportPercent;
		}
	}
	public float OpponentSupportPercent {
		get {
			if (GameObjectAccessor.Instance.Player.IsBlue) return RedSupportPercent;
			else return BlueSupportPercent;
		}
	}

	public Leaning CurrentLeaning {
		get {
			if (m_currentVote < 0) return Leaning.Red;
			else if (m_currentVote > 0) return Leaning.Blue;
			else return Leaning.Neutral;
		}
	}
	public bool IsBlue {
		get { return CurrentLeaning == Leaning.Blue; }
	}
	public bool IsRed {
		get { return CurrentLeaning == Leaning.Red; }
	}
	
	// WORKERS
	private int m_playerWorkerCount = 0;
	public int PlayerWorkerCount {
		get { return m_playerWorkerCount; }
	}
	private int m_opponentWorkerCount = 0;
	public int OpponentWorkerCount {
		get { return m_opponentWorkerCount; }
	}
	private int m_targetOpponentWorkerCount = 0;

	private int m_redWorkerCount {
		get {
			return (GameObjectAccessor.Instance.Player.IsRed)? m_playerWorkerCount : m_opponentWorkerCount;
		}
	}
	private int m_blueWorkerCount {
		get {
			return (GameObjectAccessor.Instance.Player.IsBlue)? m_playerWorkerCount : m_opponentWorkerCount;
		}
	}

	// HARVEST
	public bool HarvestComplete;
	private bool m_sentInfoToOpponent;
	private bool m_receivedInfoFromOpponent;
	private bool m_countedExistingWorkers;
	
	// DISPLAY
//	public static float populationPerWorker = 1.2f;
	private static Vector3 m_workerOffsetX = new Vector3(-0.4f, 0, 0);
	private static Vector3 m_workerOffsetY = new Vector3(0, 0.25f, 0);
	private static Vector3 m_workerAdjacencyOffset = new Vector3(0.15f, 0, 0);
	private static Vector3 m_workerCountOffset = new Vector3(-0.5f, 0, 0);
	private static Vector3 m_popularVoteOffset = new Vector3(0, 0.6f, 0);
	  
	private List< GameObject > m_playerWorkers = new List<GameObject>();
	private List< GameObject > m_opponentWorkers = new List<GameObject>();
	
	private GameObject m_playerFloatingText;
	private GameObject m_opponentFloatingText;

	public int RoundedPopulation {
		get { return Mathf.CeilToInt(Model.Population); }
	}

	private SpriteRenderer m_stateColor;
	private SpriteRenderer m_stateOutline;
	private SpriteRenderer m_stateStripes;
	private Transform m_center;

	public Vector3 Center {
		get {
			if (m_center == null) {
				m_center = transform.Find("uiAnchor");
				if (m_center == null) {
					m_center = transform;
				}
			}
			return m_center.position;
		}
	}

	public Vector3 UiCenter {
		get {
			return Utility.ConvertFromGameToUiPosition(Center);
		}
	}

	private bool m_highlighted = false;
	public static State highlightedState = null;
    
	void Awake() {
		// automatically figure out which of the child textures are which
		foreach (SpriteRenderer t in GetComponentsInChildren<SpriteRenderer>(true)) {
			if (t.name.Contains("dashed")) {
				m_stateStripes = t;
			} else if (t.name.Contains("oline")) {
				m_stateOutline = t;
			} else {
				m_stateColor = t;
			}
		}
    
		if (m_stateStripes == null) {
			Debug.LogError("No stripes on " + this.name, this);
		}
		if (m_stateOutline == null) {
			Debug.LogError("No outline on " + this.name, this);
		}
		if (m_stateColor == null) {
			Debug.LogError("No color on " + this.name, this);
		}

		// automatically add a button to the child with the collider so that we can get events from it
		Collider2D c = GetComponentInChildren<Collider2D>();
		if (c == null) {
			Debug.LogError("Couldn't find collider under " + this, this);
		} else {
			GLButton button = c.gameObject.AddComponent<GLButton>() as GLButton;
			EventDelegate.Add(button.onClick, OnClick);
		}
	}

	public void Start() {
		UpdateColor();

		/* // We don't want this text anymore
    
		Transform container = GameObjectAccessor.Instance.FloatingTextContainer.transform;

		m_playerFloatingText = GameObject.Instantiate(GameObjectAccessor.Instance.PulseTextPrefab, Utility.ConvertFromGameToUiPosition(m_workerOffsetY + m_workerCountOffset + Center), Quaternion.identity) as GameObject;
		m_playerFloatingText.GetComponent< FloatingText >().Display("");
		m_playerFloatingText.transform.parent = container;
    
		m_opponentFloatingText = GameObject.Instantiate(GameObjectAccessor.Instance.PulseTextPrefab, Utility.ConvertFromGameToUiPosition(-m_workerOffsetY + m_workerCountOffset + Center), Quaternion.identity) as GameObject;
		m_opponentFloatingText.GetComponent< FloatingText >().Display("");
		m_opponentFloatingText.transform.parent = container;
		*/
	}
    
	void OnClick() {
		// Check the current phase
		TurnPhase phase = GameObjectAccessor.Instance.GameStateManager.CurrentTurnPhase;
		if (phase == TurnPhase.Placement || phase == TurnPhase.Waiting) {
			GameObjectAccessor.Instance.DetailView.SetState(this, true);
		}
	}

	public void ResetWorkers() {
		foreach (GameObject worker in m_playerWorkers) {
			GameObject.Destroy(worker);
		}
		foreach (GameObject worker in m_opponentWorkers) {
			GameObject.Destroy(worker);
		}

		m_playerWorkerCount = m_opponentWorkerCount = m_targetOpponentWorkerCount = 0;
	}

	public void SetInitialPopularVote(float v) {
		// v is between -1 (red) and 1 (blue)
		m_currentVote = v;
		m_previousVote = v;
		UpdateColor();
	}

	private void UpdatePercentText(bool forPlayer) {
		m_playerFloatingText.SendMessage("Display", Mathf.Round(PlayerSupportPercent * 100) + "%");
		m_playerFloatingText.SendMessage("BounceOut");
        
		m_opponentFloatingText.SendMessage("Display", Mathf.Round(OpponentSupportPercent * 100) + "%");
		m_opponentFloatingText.SendMessage("BounceOut");
		// todo: only bounce the player or the opponent's text. Issue is that the one that's not bounced is in the totally wrong place.
	}
	
	public void PrepareToHarvest() {
		m_sentInfoToOpponent = false;
		m_receivedInfoFromOpponent = false;
		m_countedExistingWorkers = false;
		HarvestComplete = false;
	}

	// Does the next step in the harvest sequence; returns true if we had a step to do or false if we're done
	public bool NextHarvestAction(bool usingAi) {
		if (!m_sentInfoToOpponent) {
			if (usingAi) m_sentInfoToOpponent = true;
			else SendInfoToOpponent();
		}

		if (!m_receivedInfoFromOpponent) {
			if (usingAi) m_receivedInfoFromOpponent = true;
			else return true; // wait until we get the info
		}

		if (m_playerWorkerCount == 0 && m_opponentWorkerCount == 0 && m_targetOpponentWorkerCount == 0) {
			// Nothing to do
			return false;
		}

		if (!m_countedExistingWorkers) {
			m_countedExistingWorkers = true;
			this.Highlight();
			
			if (m_playerWorkerCount > 0 || m_opponentWorkerCount > 0) {
				foreach (GameObject worker in m_playerWorkers) {
					worker.SendMessage("BounceOut");
				}
				foreach (GameObject worker in m_opponentWorkers) {
					worker.SendMessage("BounceOut");
				}
				UpdateVote();
				return true;
			}
		}

		if (m_targetOpponentWorkerCount > m_opponentWorkerCount) {
			this.Highlight();

			// Add a new opponent worker
			GameObject worker = CreateWorkerPrefab(false);
			worker.SendMessage("BounceOut");
			UpdateVote();
			return true;
		}

		SetVote();
		return false;
	}

	public void UpdateVote() {
		Leaning prevLeaning = CurrentLeaning;
		float change = (m_blueWorkerCount - m_redWorkerCount) * GameObjectAccessor.Instance.GameStateManager.WorkerIncrement * 2;
		// we multiply by 2 so 1% change => 0.02 difference (since the vote goes from -1 to 1)
		m_currentVote = m_previousVote + change;
		UpdateColor(CurrentLeaning != prevLeaning);
	}

	private void SetVote() {
		m_currentVote = Mathf.Clamp(m_currentVote, -1, 1);
		m_previousVote = m_currentVote;
	}

	public void SendInfoToOpponent() {
		networkView.RPC("RecieveInfoFromOpponent", RPCMode.Others, m_playerWorkerCount);

		m_sentInfoToOpponent = true;
	}

	[RPC]
	public void RecieveInfoFromOpponent(int workerCount) {
		m_targetOpponentWorkerCount = workerCount;
		m_receivedInfoFromOpponent = true;
	}

	// Called by the AI
	public void IncrementOpponentWorkerCount(int amount = 1) {
		m_targetOpponentWorkerCount += amount;
		Debug.Log(m_abbreviation + " added worker from AI. New count: " + m_targetOpponentWorkerCount);
	}
		
	public float GetPlayerPercentChange() {
		return m_playerWorkerCount * GameObjectAccessor.Instance.GameStateManager.WorkerIncrement;
	}

	public float GetOpponentPercentChange() {
		return m_opponentWorkerCount * GameObjectAccessor.Instance.GameStateManager.WorkerIncrement;
	}

	public void UpdateColor(bool playParticles = false) {
		if (Hidden) {
			m_stateColor.color = GameObjectAccessor.Instance.GameColorSettings.undiscoveredState;
			m_stateOutline.color = GameObjectAccessor.Instance.GameColorSettings.outline;
			m_stateStripes.enabled = true;
			return;
		}

		float t;
		if (IsBlue) {
			if (!InPlay) {
				t = 1;
			} else {
				t = Mathf.InverseLerp(0.5f, 1f, BlueSupportPercent); // 0.5 -> 0, 1 -> 1
				t = Mathf.Lerp(0.2f, 1f, t); // 0 -> 0.2, 1 -> 1 (Start at 0.2 so we don't go all the way to the neutral color.)
			}
			m_stateColor.color = Color.Lerp(GameObjectAccessor.Instance.GameColorSettings.neutralState, GameObjectAccessor.Instance.GameColorSettings.blueState, t);
			m_stateOutline.color = GameObjectAccessor.Instance.GameColorSettings.outline;
			m_stateOutline.sortingOrder = -7;
		} else if (IsRed) {
				if (!InPlay) {
					t = 1;
				} else {
					t = Mathf.InverseLerp(0.5f, 1f, RedSupportPercent); // 0.5 -> 0, 1 -> 1
					t = Mathf.Lerp(0.2f, 1f, t); // 0 -> 0.2, 1 -> 1 (Start at 0.2 so we don't go all the way to the neutral color.)
				}
				m_stateColor.color = Color.Lerp(GameObjectAccessor.Instance.GameColorSettings.neutralState, GameObjectAccessor.Instance.GameColorSettings.redState, t);
				m_stateOutline.color = GameObjectAccessor.Instance.GameColorSettings.outline;
				m_stateOutline.sortingOrder = -7;
		} else {
				if (InPlay) {
					m_stateColor.color = GameObjectAccessor.Instance.GameColorSettings.neutralState;
				} else {
					m_stateColor.color = GameObjectAccessor.Instance.GameColorSettings.neutralLockedState;
				}

				m_stateOutline.color = GameObjectAccessor.Instance.GameColorSettings.neutralOutline;
				m_stateOutline.sortingOrder = -6;
			}

		if (playParticles) {
			GameObject.Instantiate(GameObjectAccessor.Instance.FlipStateParticleSystemBlue, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -0.5f), Quaternion.identity);
		}

		// show a different outline if we're highlighted
		if (m_highlighted) {
			m_stateOutline.color = GameObjectAccessor.Instance.GameColorSettings.highlightOutline;
			m_stateOutline.sortingOrder = -5;
		}
			
		m_stateStripes.enabled = !InPlay;
	}
    
	public void Highlight() {
		if (State.highlightedState != null) State.highlightedState.UnHighlight();

		m_highlighted = true;
		UpdateColor();
		State.highlightedState = this;
	}

	public void UnHighlight() {
		m_highlighted = false;
		UpdateColor();
		if (State.highlightedState == this) {
			State.highlightedState = null;
		}
	}

	public bool PlayerCanPlaceWorker() {
		return (InPlay &&
			GameObjectAccessor.Instance.GameStateManager.CurrentTurnPhase == TurnPhase.Placement &&
			GameObjectAccessor.Instance.Budget.IsAmountAvailable(GameMove.GetCost(GameActions.PLACE_WORKER)));
		// && m_playerSupporterList.Count < UnitCap); // for now we're not capping supporters per state
	}
        
	public void PlayerPlaceWorker() {    
		if (PlayerCanPlaceWorker()) {
			CreateWorkerPrefab(true);

			GameObjectAccessor.Instance.Budget.ConsumeAmount(GameMove.GetCost(GameActions.PLACE_WORKER));
		}
	}

	public bool PlayerCanRemoveWorker() {
		return (InPlay &&
			GameObjectAccessor.Instance.GameStateManager.CurrentTurnPhase == TurnPhase.Placement &&
			m_playerWorkers.Count > 0);
	}

	public void PlayerRemoveWorker() {
		if (PlayerCanRemoveWorker()) {

			// Remove the last supporter
			GameObject supporter = m_playerWorkers[m_playerWorkers.Count - 1];
			m_playerWorkers.Remove(supporter);
			Destroy(supporter);
			m_playerWorkerCount --;
        
			GameObjectAccessor.Instance.Budget.ConsumeAmount(GameMove.GetCost(GameActions.REMOVE_WORKER)); // this will probably be a negative number (add money)
     	}
	}
    
	public GameObject CreateWorkerPrefab(bool isPlayer) {
		Vector3 supporterPosition = Center + m_workerOffsetX + (isPlayer? m_workerOffsetY + (m_playerWorkers.Count * m_workerAdjacencyOffset) : -m_workerOffsetY + ((m_opponentWorkers.Count) * m_workerAdjacencyOffset));

		GameObject newSupporter = GameObject.Instantiate(GameObjectAccessor.Instance.SupporterPrefab, supporterPosition, Quaternion.identity) as GameObject;

		if (isPlayer ^ GameObjectAccessor.Instance.Player.IsRed) {
			newSupporter.GetComponent<SpriteRenderer>().color = GameObjectAccessor.Instance.GameColorSettings.blueStateDark;
		} else {
			newSupporter.GetComponent<SpriteRenderer>().color = GameObjectAccessor.Instance.GameColorSettings.redStateDark;
		}

		if (isPlayer) {
			m_playerWorkers.Add(newSupporter);
			m_playerWorkerCount ++;
		} else {
			m_opponentWorkers.Add(newSupporter);
			m_opponentWorkerCount ++;
		}

		return newSupporter;
	}

}