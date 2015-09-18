using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MatchmakerEntry : PrefabEntry {
	public Text PlayerLabel;
	public Text ScenarioLabel;
	public Image ColorIndicator;

	public void Set(string playerName, string scenarioName, Leaning color) {
		base.CheckToggleGroup();

		PlayerLabel.text = playerName;
		ScenarioLabel.text = scenarioName;
		ColorIndicator.color = AutoSetColor.GetColor(color == Leaning.Blue, AutoSetColor.ColorChoice.med);
	}
}
