using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// This will automatically add a label with each state's abbreviation or number of votes.
// You can set the label position for a state by adding a "stateLabel" object as its child. Else it's automatic.
public class StateLabelManager : SingletonBehavior<StateLabelManager> {
	public enum LabelOptions { ABBREVIATION, VOTES };
	public LabelOptions content = LabelOptions.VOTES;
	public bool showOnlyOnActiveStates = true;
	
	private Object stateLabelPrefab;
	private Dictionary<State, Text> stateLabels = new Dictionary<State, Text>();
	
	void Awake() {
		stateLabelPrefab = Resources.Load("StateLabel");
	}
	
	void Start() {
		Refresh();
	}
	
	public void Refresh () {
		Transform newTransform;
		Text label;
		bool show;
		foreach (State state in ObjectAccessor.Instance.StatesContainer.transform.GetComponentsInChildren<State>()) {
			show = !showOnlyOnActiveStates || state.InPlay;
			if (stateLabels.ContainsKey(state)) {
				stateLabels[state].gameObject.SetActive(show);
			} else if (show) {
				newTransform = Utility.InstantiateAsChild(stateLabelPrefab, transform);
				newTransform.position = state.Center;

				label = newTransform.GetComponent<Text>();
				if (content == LabelOptions.VOTES) label.text = state.Model.ElectoralCount.ToString();
				else if (content == LabelOptions.ABBREVIATION) label.text = state.Model.Abbreviation;
				
				stateLabels[state] = label;
			}
		}
	}
}
