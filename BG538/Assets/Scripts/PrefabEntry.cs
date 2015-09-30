using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PrefabEntry : MonoBehaviour {
	protected void Start() { }

	[ContextMenu("CheckToggleGroup")]
	public void CheckToggleGroup() {
		Toggle toggle = GetComponent<Toggle>();
		if (toggle && !toggle.group) SetToggleGroup(Utility.FirstAncestorOfType<ToggleGroup>(transform));
	}

	public void SetToggleGroup(ToggleGroup group) {
		if (group) {
			Toggle toggle = GetComponent<Toggle>();
			toggle.group = group;

			if (!group.allowSwitchOff && !group.AnyTogglesOn()) toggle.isOn = true;
		}
	}
}
