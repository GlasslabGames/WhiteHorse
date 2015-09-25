using UnityEngine;
using System.Collections;
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
	public int TotalWeeks;
	public float[] Income;
	public float WorkerIncrement;
	public float HarvestInterval;
	public float VoteUpdateTime;

	public int DefaultScenarioId;

	public GameActionCost[] GameActionCosts;
	private Dictionary<GameAction, float> gameActionCostDict;

	public GameColorSettings Colors;

	public float GetGameActionCost(GameAction m) {
		if (gameActionCostDict == null) {
			gameActionCostDict = new Dictionary<GameAction, float>();
			foreach (GameActionCost actionCost in GameActionCosts) {
				gameActionCostDict.Add(actionCost.move, actionCost.cost);
			}
		}
		return gameActionCostDict[m];
	}
}
