using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// This will automatically add a label with each state's abbreviation or number of votes.
// You can set the label position for a state by adding a "stateLabel" object as its child. Else it's automatic.
public class StateLabelManager : MonoBehaviour {
	public enum LabelOptions { ABBREVIATION, VOTES };
	public LabelOptions content = LabelOptions.VOTES;
	public bool showOnlyOnActiveStates = true;
	
	public GameObject stateLabelPrefab;
	private Dictionary<State, Text> stateLabels = new Dictionary<State, Text>();
	
	void Awake() {
	}
	
	void Start() {
		Refresh();
	}

	[ContextMenu("Refresh")]
	public void Refresh () {
		Transform newTransform;
		Text label = null;
		bool show;
		foreach (State state in ObjectAccessor.Instance.StatesContainer.transform.GetComponentsInChildren<State>()) {
			show = !showOnlyOnActiveStates || state.InPlay;
			if (stateLabels.ContainsKey(state)) {
				label = stateLabels[state];
				label.gameObject.SetActive(show);
			} else if (show) {
				newTransform = Utility.InstantiateAsChild(stateLabelPrefab, transform);
				newTransform.position = state.UICenter;

				label = newTransform.GetComponent<Text>();
				stateLabels[state] = label;
			}

			if (show) {
				if (content == LabelOptions.VOTES) label.text = state.electoralVotes.ToString();
				else if (content == LabelOptions.ABBREVIATION) label.text = state.Model.Abbreviation;
			}
		}
	}
}
