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
			if (CurrentOptions != null && CurrentOptions.ContainsKey("duration") && CurrentOptions["duration"] != null) {
				return (int) CurrentOptions["duration"];
			} else return _totalWeeks;
		}
	}
	public float[] Income;
	public float _workerIncrement;
	public float WorkerIncrement {
		get {
			if (CurrentOptions != null && CurrentOptions.ContainsKey("increment") && CurrentOptions["increment"] != null) {
				return (float) CurrentOptions["increment"];
			} else return _workerIncrement;
		}
	}
	public float HarvestInterval;
	public float VoteUpdateTime;

	public int DefaultScenarioId;

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
	public Dictionary<string, object> CurrentOptions = new Dictionary<string, object>();
}
