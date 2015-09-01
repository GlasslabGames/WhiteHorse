using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This will automatically add a label with each state's abbreviation.
// You can set the label position for a state by adding a "stateLabel" object as its child. Else it's automatic.
// This is just a first pass, it will have to be refined to work with the zoom.
public class ShowStateLabels : MonoBehaviour {
	public enum LabelOptions { ABBREVIATION, VOTES };
	public LabelOptions Content = LabelOptions.VOTES;
	public bool ShowOnlyOnActiveStates = true;

	private Object m_stateLabelPrefab;
	private Dictionary<State, UILabel> m_stateLabels;

	void Awake() {
		m_stateLabels = new Dictionary<State, UILabel>();
		m_stateLabelPrefab = Resources.Load("StateLabel");
	}

	void Start() {
		Refresh();
	}

	public void Refresh () {
		Transform newTransform;
		UILabel label;
		bool show;
		foreach (State state in GameObjectAccessor.Instance.StatesContainer.transform.GetComponentsInChildren<State>()) {
			show = !ShowOnlyOnActiveStates || state.InPlay;
			if (m_stateLabels.ContainsKey(state)) {
				m_stateLabels[state].gameObject.SetActive(show);
			} else if (show) {
				newTransform = Utility.InstantiateAsChild(m_stateLabelPrefab, transform);
				newTransform.position = state.UiCenter;
				label = newTransform.GetComponent<UILabel>();
				
				if (Content == LabelOptions.VOTES) label.text = state.Model.ElectoralCount.ToString();
				else if (Content == LabelOptions.ABBREVIATION) label.text = state.Model.Abbreviation;

				m_stateLabels[state] = label;
			}
		}
	}
}
