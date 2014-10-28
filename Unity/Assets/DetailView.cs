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
	public UILabel m_redPercent;
	public UILabel m_bluePercent;
	public UILabel m_redPoints;
	public UILabel m_bluePoints;
	public UILabel m_unitsCount;
	public UILabel m_unit1Count;
	public UILabel m_unit2Count;
	public UILabel m_unit3Count;
	public UITexture m_newUnit;

	private State m_currentState;

	private State m_highlightedState;

  public State CurrentState
  {
    get { return m_currentState; }
  }

	void Start() {
		ClearState ();
	}

	// Update is called once per frame
	void Update () {
    //Debug.Log ( Input.mousePosition );
		if (Input.mousePosition.y < ( Screen.height * 0.15f ) || (Input.mousePosition.x > ( Screen.width * 0.85f ) && Input.mousePosition.y < ( Screen.height * 0.25f ) ) ) return; // kinda hacky way to ignore clicks on the bottom ui

		//if (Input.GetMouseButtonDown (0))
			//Debug.Log (Input.mousePosition);

		// if they just released the mouse, select the state they were over
		if (Input.GetMouseButtonUp (0) && m_currentState != null) {
			SetState(m_currentState, true);
		}

		// if the mouse is down, find the state it's over
		else if (Input.GetMouseButton (0)) {
			Vector3 clickPosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			RaycastHit2D[] hits = Physics2D.LinecastAll (clickPosition, clickPosition);
			m_currentState = null;

			foreach (RaycastHit2D hit in hits) {
				if (hit.collider.transform.parent != null) {
					m_currentState = hit.collider.transform.parent.GetComponent<State> ();
					if (m_currentState != null) {
						SetState(m_currentState, false);
						break;
					}
				}
			}

			if (m_currentState == null) ClearState();
		}
	}

	public void SetState(State state, bool showIndicator) {
		if (m_row1 != null)
			m_row1.SetActive (true);
		if (m_row2 != null)
			m_row2.SetActive (state.InPlay);
		if (m_row2Inactive != null)
			m_row2Inactive.SetActive (!state.InPlay);

		if (m_name != null)
			m_name.text = state.Model.Name;
		if (m_abbreviation != null)
			m_abbreviation.text = state.m_abbreviation;
		if (m_population != null)
			m_population.text = "Population " + state.Model.Population.ToString () + "M";
		if (m_votes != null)
			m_votes.text = "Electoral College Votes " + state.Model.ElectoralCount.ToString ();

		Leaning color = GameObjectAccessor.Instance.Player.m_leaning;
		if (m_redPoints != null) {
			int points = (color == Leaning.Red) ? state.PlayerBasisCountIncrement : state.OpponentBasisCountIncrement;
			m_redPoints.text = points.ToString () + "pts";
		}
		if (m_bluePoints != null) {
			int points = (color == Leaning.Blue) ? state.PlayerBasisCountIncrement : state.OpponentBasisCountIncrement;
			m_bluePoints.text = points.ToString () + "pts";
		}

		float bluePercent = state.PopularVote / 2f + 0.5f;
		float redPercent = 1 - bluePercent;
		if (m_redPercent != null)
			m_redPercent.text = Mathf.Round (redPercent * 100).ToString () + "%";
		if (m_bluePercent != null)
			m_bluePercent.text = Mathf.Round (bluePercent * 100).ToString () + "%";

		
		if (state.InPlay) {
			// assign labels for number of units
			if (m_unitsCount != null)
				m_unitsCount.text = state.PlayerCampaignWorkers.ToString () + "/" + state.UnitCap.ToString();
			int[] workerCounts = state.PlayerCampaignWorkerCounts;
			if (m_unit1Count != null)
				m_unit1Count.text = workerCounts [0].ToString () + "x";
			if (m_unit2Count != null)
				m_unit2Count.text = workerCounts [1].ToString () + "x";
			if (m_unit3Count != null)
				m_unit3Count.text = workerCounts [2].ToString () + "x";
		}

		if (showIndicator) {
			// reset the previous scaled state
			if (m_highlightedState != null) m_highlightedState.Highlight(false);

			state.Highlight(true);
			m_highlightedState = state;
		}
	}

	void ClearState() {
		if (m_row1 != null) m_row1.SetActive (false);
		if (m_row2 != null) m_row2.SetActive (false);
		if (m_row2Inactive != null) m_row2Inactive.SetActive (false);

		if (m_highlightedState != null) {
			m_highlightedState.Highlight (false);
			m_highlightedState = null;
		}
	}

  public void Upgrade1()
  {
    CurrentState.Upgrade1();
  }
  public void Upgrade2()
  {
    CurrentState.Upgrade2();
  }

	public void PlaceSupporter() {
		CurrentState.PlayerPlaceSupporter (true);
		SetState (m_currentState, false); // update the current state
	}

	public void RemoveSupporter() {
		CurrentState.PlayerRemoveSupporter ();
		SetState (m_currentState, false); // update the current state
	}
}
