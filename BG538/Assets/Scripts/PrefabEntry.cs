using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PrefabEntry : MonoBehaviour {
	private Toggle toggle;

	protected void Start() {
		toggle = GetComponent<Toggle>();
	}

	[ContextMenu("CheckToggleGroup")]
	public void CheckToggleGroup() {
		if (!toggle.group) SetToggleGroup(Utility.FirstAncestorOfType<ToggleGroup>(transform));
	}

	public void SetToggleGroup(ToggleGroup group) {
		if (group) {
			toggle.group = group;

			if (!group.allowSwitchOff && !group.AnyTogglesOn()) toggle.isOn = true;
		}
	}
}
