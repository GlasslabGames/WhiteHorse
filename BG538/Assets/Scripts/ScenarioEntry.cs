using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScenarioEntry : PrefabEntry {
	public Text nameLabel;

	public void Set(string name) {
		base.CheckToggleGroup();

		nameLabel.text = name;
	}
}
