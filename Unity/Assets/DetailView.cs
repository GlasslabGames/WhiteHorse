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
	public UILabel m_playerPercent;
	public UILabel m_opponentPercent;
	public UILabel m_unitsCount;
	public UILabel m_opponentUnitsCount;
	public UILabel m_playerIncrement;
	public UILabel m_opponentIncrement;

	public GLButton m_addUnitButton;
	public GLButton m_removeUnitButton;

	public UITexture m_headerBg;
	public UITexture m_leftArrow;
	public UITexture m_rightArrow;

	public OpinionMeter m_voteMeter;

	private State m_currentState;

	private State m_highlightedState;

  public State CurrentState
  {
    get { return m_currentState; }
  }

	void Start() {
		ClearState ();
	}
	/*
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
	*/

	public void SetState(State state, bool showIndicator) {
		gameObject.SetActive (true);
		Debug.Log ("SetState");

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

		string redPercent = Mathf.Round (state.RedSupportPercent * 100).ToString () + "%";
		string bluePercent = Mathf.Round(state.BlueSupportPercent * 100).ToString () + "%";
		if (m_playerPercent != null) {
			m_playerPercent.text = (GameObjectAccessor.Instance.Player.IsRed) ? redPercent : bluePercent;
		}
		if (m_opponentPercent != null) {
			m_opponentPercent.text = (GameObjectAccessor.Instance.Player.IsBlue) ? redPercent : bluePercent;
		}

		if (m_voteMeter != null) {
			float percent = (GameObjectAccessor.Instance.Player.IsRed)? state.RedSupportPercent : state.BlueSupportPercent;
			m_voteMeter.Set(percent, false);
		}

		if (m_row2 != null) {
			m_row2.SetActive (state.InPlay);
		}
		if (m_row2Inactive != null) {
			m_row2Inactive.SetActive (!state.InPlay);
		}

		if (state.InPlay) {
			if (m_unitsCount != null) {
				m_unitsCount.text = state.PlayerCampaignWorkers.ToString () + " x";
			}
			if (m_opponentUnitsCount != null) {
				m_opponentUnitsCount.text = state.OpponentCampaignWorkers.ToString () + " x";
			}
		}

		Color color = GameObjectAccessor.Instance.GameColorSettings.undiscoveredState; // gray
		if (state.IsBlue) {
			color = GameObjectAccessor.Instance.GameColorSettings.blueDarker;
		} else if (state.IsRed) {
			color = GameObjectAccessor.Instance.GameColorSettings.redDarker;
		}

		if (m_headerBg != null) {
			m_headerBg.color = color;
		}
		if (m_leftArrow != null) {
			m_leftArrow.color = color;
		}
		if (m_rightArrow != null) {
			m_rightArrow.color = color;
		}

		// TODO: Percent increment
		// TODO: Buttons

		// place near state
		Vector3 pos = transform.position;
		Vector3 statePos = state.Center;
		Debug.Log (statePos);
		pos.y = Mathf.Clamp (statePos.y, -5.5f, 2.8f);
		Debug.Log (pos.y);
		// If it's too far on the right, show the popup on the left. Else it's always on the right.
		if (statePos.x < 4) {
			pos.x = statePos.x + 1.25f;
			m_leftArrow.gameObject.SetActive (true);
			m_rightArrow.gameObject.SetActive (false);
		} else {
			pos.x = statePos.x - 5.75f;
			m_leftArrow.gameObject.SetActive (false);
			m_rightArrow.gameObject.SetActive (true);
		}
		transform.position = Utility.ConvertFromGameToUiPosition(pos);

		if (showIndicator) {
			// reset the previous scaled state
			if (m_highlightedState != null) m_highlightedState.Highlight(false);

			state.Highlight(true);
			m_highlightedState = state;
		}
	}

	public void OnClickBackground() {
		ClearState ();
	}

	void ClearState() {
		Debug.Log ("ClearState");
		gameObject.SetActive (false);

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
