using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.Match;
using System.Collections;

public class MatchmakerEntry : PrefabEntry {
	public Text PlayerLabel;
	public Text ScenarioLabel;
	public Image ColorIndicator;

	public MatchDesc Match;

	public void Set(MatchDesc match) {
		Match = match;
		Debug.Log (match + " (no attributes)");

		Set("Test", match.name, Leaning.Blue);
	}

	public void Set(string playerName, string scenarioName, Leaning color) {
		base.CheckToggleGroup();

		PlayerLabel.text = playerName;
		ScenarioLabel.text = scenarioName;
		ColorIndicator.color = ColorSwapper.GetColor(color == Leaning.Blue, ColorSwapper.ColorChoice.med);
	}
}
