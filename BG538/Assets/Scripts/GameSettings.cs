using UnityEngine;
using System.Collections;

public class GameSettings : SingletonBehavior<GameSettings> {
	
	public int TotalWeeks;
	public float[] Income;
	public float WorkerIncrement;

	public enum ScenarioType { A, B }
	public ScenarioType DefaultScenarioType;
	public int DefaultScenarioId;

	public GameColorSettings Colors;
}
