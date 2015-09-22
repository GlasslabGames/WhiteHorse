using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScenarioEntry : PrefabEntry {
	public Text nameLabel;
	private ScenarioModel model;

	public delegate void ScenarioEvent(ScenarioModel scenario);
	public ScenarioEvent OnSelected;

	public void Set(ScenarioModel scenario) {
		base.CheckToggleGroup();

		model = scenario;
		nameLabel.text = scenario.Name;
	}

	public void OnToggle(bool value) {
		if (value && OnSelected != null) OnSelected(model);
	}
}
