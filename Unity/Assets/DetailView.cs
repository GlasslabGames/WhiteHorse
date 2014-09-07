using UnityEngine;
using System.Collections;

public class DetailView : MonoBehaviour {
	public GameObject m_row1;
	public GameObject m_row2;
	public GameObject m_row2Inactive;

	public UILabel m_abbreviation;
	public UILabel m_name;
	public UILabel m_population;
	public UILabel m_votes;

	void Start() {
		ClearState ();
	}

	// Update is called once per frame
	void Update () {

		// if the mouse is down, find the state it's over
		if (Input.GetMouseButton (0)) {
			Vector3 clickPosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			RaycastHit2D[] hits = Physics2D.LinecastAll (clickPosition, clickPosition);
			State state = null;

			foreach (RaycastHit2D hit in hits) {
				if (hit.collider.transform.parent != null) {
					state = hit.collider.transform.parent.GetComponent<State> ();
					if (state != null) {
						SetState(state);
						break;
					}
				}
			}

			if (state == null) ClearState();
		}
	}

	void SetState(State state) {
		if (m_row1 != null) m_row1.SetActive (true);
		if (m_row2 != null) m_row2.SetActive ( state.m_inPlay);
		if (m_row2Inactive != null) m_row2Inactive.SetActive (true);

		if (m_name != null) m_name.text = state.m_name;
		if (m_abbreviation != null) m_abbreviation.text = state.m_abbreviation;
		if (m_population != null) m_population.text = "Population "+state.m_populationInMillions.ToString() + "M";
		if (m_votes != null) m_votes.text = "Electoral College Votes "+state.m_electoralCount.ToString();

		if (state.m_inPlay) {
			// assign labels for number of units
		}
	}

	void ClearState() {
		if (m_row1 != null) m_row1.SetActive (false);
		if (m_row2 != null) m_row2.SetActive (false);
		if (m_row2Inactive != null) m_row2Inactive.SetActive (false);
	}
}
