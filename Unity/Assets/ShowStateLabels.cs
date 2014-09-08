using UnityEngine;
using System.Collections;

// This will automatically add a label with each state's abbreviation.
// You can set the label position for a state by adding a "stateLabel" object as its child. Else it's automatic.
// This is just a first pass, it will have to be refined to work with the zoom.
public class ShowStateLabels : MonoBehaviour {
	public enum LabelOptions { ABBREVIATION, VOTES };
	public LabelOptions Content = LabelOptions.VOTES;
	public bool ShowOnlyOnActiveStates = true;

	void Start () {
		if (!enabled) return;

		Object statePrefab = Resources.Load("StateLabel");
		Transform newTransform;
		Transform stateTransform;
		UILabel label;
		foreach (State state in GameObjectAccessor.Instance.StatesContainer.transform.GetComponentsInChildren<State>()) {
			if (ShowOnlyOnActiveStates && !state.m_inPlay) continue;
			Debug.Log("showing label over "+state.name);
			newTransform = Utility.InstantiateAsChild(statePrefab, transform);
			label = newTransform.GetComponent<UILabel>();
			if (Content == LabelOptions.VOTES) label.text = state.m_electoralCount.ToString();
			else if (Content == LabelOptions.ABBREVIATION) label.text = state.m_abbreviation;

			stateTransform = state.transform.Find("uiAnchor");
			if (stateTransform == null) stateTransform = state.transform;
			newTransform.position = Utility.ConvertFromGameToUiPosition(stateTransform.position);
		}
	}
}
