using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.Match;
using System.Collections.Generic;

public class MatchmakerEntry : PrefabEntry {
	public Text PlayerLabel;
	public Text ScenarioLabel;
	public Image ColorIndicator;

	public MatchDesc Match;

	public void Set(MatchDesc match) {
		Match = match;

		/*// This is what we would do if matchAttributes work, but they don't. See LobbyManager.Play
		Dictionary<string, long> attributes = match.matchAttributes;
		int scenarioId = (int) attributes["scenarioId"];
		ScenarioModel scenario = ScenarioModel.GetModel(scenarioId);
		Leaning color = (Leaning) attributes["color"];
		*/

		string[] parts = match.name.Split(LobbyManager.MatchInfoDivider);
		int scenarioId = System.Int32.Parse(parts[1]);
		ScenarioModel scenario = ScenarioModel.GetModel(scenarioId);
		int colorInt = System.Int32.Parse(parts[2]);
		Set(parts[0], scenario.Name, (Leaning) colorInt);
	}

	public void Set(string playerName, string scenarioName, Leaning color) {
		base.CheckToggleGroup();

		PlayerLabel.text = playerName;
		ScenarioLabel.text = scenarioName;
		ColorIndicator.color = ColorSwapper.GetColor(color == Leaning.Blue, ColorSwapper.ColorChoice.med);
	}
}
