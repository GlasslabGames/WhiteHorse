using UnityEngine;
using System.Collections.Generic;

public enum GameAction {
	PlaceWorker,
	RemoveWorker
}

[System.Serializable]
public struct GameActionCost {
	public GameAction move;
	public float cost;
}

public class GameSettings : SingletonBehavior<GameSettings> {
	public string Version;
	public int _totalWeeks;
	public int TotalWeeks {
		get {
			if (currentDuration > 0) return currentDuration;
			else return _totalWeeks;
		}
	}
	public float[] Income;
	public float _workerIncrement;
	public float WorkerIncrement {
		get {
			if (currentIncrement > 0) return currentIncrement;
			else return _workerIncrement;
		}
	}
	public float HarvestInterval;
	public float VoteUpdateTime;

	public int DefaultScenarioId;
	public int ScenarioId {
		get {
			if (currentScenarioId > 0) return currentScenarioId;
			else return DefaultScenarioId;
		}
	}

	public GameActionCost[] GameActionCosts;
	private Dictionary<GameAction, float> gameActionCostDict;

	public GameColorSettings Colors;

	public void Awake() {
		DontDestroyOnLoad(gameObject);
	}

	public float GetGameActionCost(GameAction m) {
		if (gameActionCostDict == null) {
			gameActionCostDict = new Dictionary<GameAction, float>();
			foreach (GameActionCost actionCost in GameActionCosts) {
				gameActionCostDict.Add(actionCost.move, actionCost.cost);
			}
		}
		return gameActionCostDict[m];
	}

	// This is how we store specific game options between the lobby and the game (for single-player)
	[HideInInspector]
	public int currentScenarioId;
	[HideInInspector]
	public Leaning currentColor;
	// tweakable values
	[HideInInspector]
	public float currentIncrement;
	[HideInInspector]
	public int currentDuration;
}
