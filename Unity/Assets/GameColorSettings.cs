using UnityEngine;
using System.Collections;

[System.Serializable]
public class GameColorSettings {

	public Color blueState;
	public Color blueStateDark;
	
	public Color redState;
	public Color redStateDark;
	
	public Color undiscoveredState;
	public Color neutralState;

	public Color outline;
	public Color neutralOutline;

	public Color highlightOutline;

	public Color blueDarker;
	public Color redDarker;
}

[System.Serializable]
public class GameTextures {
	public Texture EndTurnButton;
	public Texture SubmittedButton;
	public Texture ResultsButton;
}