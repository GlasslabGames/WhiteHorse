using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof(Button))]
public class DisableButtonByToggleGroup : MonoBehaviour {

	private Button button;
	public ToggleGroup WatchedGroup;
	private bool togglesOn;

	void Start () {
		button = GetComponent<Button>();
		togglesOn = button.interactable; // Start with the same value as the button so we immediately switch if necessary
	}
	
	void Update () {
		if (togglesOn != WatchedGroup.AnyTogglesOn()) {
			togglesOn = WatchedGroup.AnyTogglesOn();
			button.interactable = togglesOn;
		}
	}
}
