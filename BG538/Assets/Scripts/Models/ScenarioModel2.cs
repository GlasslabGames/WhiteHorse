using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScenarioModel2 : ScenarioModel<ScenarioModel2> {
	public int MinStatesInPlay;
	public int MaxStatesInPlay;
  public List<int> StatesInPlay;
  public List<int> PercentBlue;

	// This matches the InPlayStatus category in the JSON
	public enum InPlayStatus { ALWAYS = 1, MAYBE = 2, NEVER = 3 }
}
