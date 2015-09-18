using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MatchmakerEntry : MonoBehaviour {
	public Text PlayerLabel;
	public Text ScenarioLabel;
	public Image ColorIndicator;

	private Toggle toggle;
	
	void Start() {
		toggle = GetComponent<Toggle>();
	}

	void Set(string playerName, string scenarioName, Leaning color) {
		PlayerLabel.text = playerName;
		ScenarioLabel.text = scenarioName;
		ColorIndicator.color = AutoSetColor.GetColor(color == Leaning.Blue, AutoSetColor.ColorChoice.med);

		if (!toggle.group) {
			ToggleGroup group = Utility.FirstAncestorOfType<ToggleGroup>(transform);
			if (group) group.RegisterToggle(toggle);
		}
	}
}
