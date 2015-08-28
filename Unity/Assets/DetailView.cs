using UnityEngine;
using System.Collections;

public class DetailView : MonoBehaviour {
	public GameObject m_activeParent;
	public GameObject m_inactiveParent;
	public UILabel m_abbreviation;
	public UILabel m_name;
	public UILabel m_population;
	public UILabel m_playerPercent;
	public UILabel m_opponentPercent;
	public UILabel m_unitsCount;
	public UILabel m_opponentUnitsCount;
	public UILabel m_playerIncrement;
	public UILabel m_opponentIncrement;
	public DetailViewButton m_addUnitButton;
	public DetailViewButton m_removeUnitButton;
	public UITexture m_headerBg;
	public UITexture m_leftArrow;
	public UITexture m_rightArrow;
	public OpinionMeter m_voteMeter;
	private State m_currentState;
	private State m_highlightedState;

	public State CurrentState {
		get { return m_currentState; }
	}

	void Start() {
		ClearState();
	}

	public void SetState(State state, bool showIndicator) {
		gameObject.SetActive(true);
		m_currentState = state;
		Debug.Log("SetState " + state.name);

		// We show different content depending on whether the state is in play or not
		if (m_activeParent != null) {
			m_activeParent.SetActive(state.InPlay);
		}
		if (m_inactiveParent != null) {
			m_inactiveParent.SetActive(!state.InPlay);
		}

		// Basic state info
		if (m_name != null) {
			m_name.text = state.Model.Name;
		}
		if (m_abbreviation != null) {
			m_abbreviation.text = state.m_abbreviation;
		}
		if (m_population != null) {
			m_population.text = "Population " + state.Model.Population.ToString() + "M";
		}

		// Show the current support percentages
		string redPercent = Mathf.Round(state.RedSupportPercent * 100).ToString() + "%";
		string bluePercent = Mathf.Round(state.BlueSupportPercent * 100).ToString() + "%";
		if (m_playerPercent != null) {
			m_playerPercent.text = (GameObjectAccessor.Instance.Player.IsRed)? redPercent : bluePercent;
		}
		if (m_opponentPercent != null) {
			m_opponentPercent.text = (GameObjectAccessor.Instance.Player.IsBlue)? redPercent : bluePercent;
		}

		if (m_voteMeter != null) {
			float percent = (GameObjectAccessor.Instance.Player.IsRed)? state.RedSupportPercent : state.BlueSupportPercent;
			m_voteMeter.Set(percent, false);
		}

		RefreshSupporterInfo();

		// Set the color
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

		// place near state
		Vector3 pos = transform.position;
		Vector3 statePos = state.Center;
		pos.y = Mathf.Clamp(statePos.y, -5.5f, 2.8f);

		// If it's too far on the right, show the popup on the left. Else it's always on the right.
		if (statePos.x < 4) {
			pos.x = statePos.x + 1.25f;
			m_leftArrow.gameObject.SetActive(true);
			m_rightArrow.gameObject.SetActive(false);
		} else {
			pos.x = statePos.x - 5.75f;
			m_leftArrow.gameObject.SetActive(false);
			m_rightArrow.gameObject.SetActive(true);
		}
		transform.position = Utility.ConvertFromGameToUiPosition(pos);

		if (showIndicator) {
			// reset the previous scaled state
			if (m_highlightedState != null) {
				m_highlightedState.Highlight(false);
			}

			state.Highlight(true);
			m_highlightedState = state;
		}
	}

	public void OnClickBackground() {
		Debug.Log("Click bg");
		ClearState();
	}

	void ClearState() {
		Debug.Log("ClearState");
		gameObject.SetActive(false);

		if (m_highlightedState != null) {
			m_highlightedState.Highlight(false);
			m_highlightedState = null;
		}
	}

	void RefreshSupporterInfo() {
		if (!CurrentState) {
			return;
		}

		// Show the current number of units
		if (CurrentState.InPlay) {
			if (m_unitsCount != null) {
				m_unitsCount.text = CurrentState.PlayerCampaignWorkers.ToString() + " x";
			}
			if (m_opponentUnitsCount != null) {
				m_opponentUnitsCount.text = CurrentState.OpponentCampaignWorkers.ToString() + " x";
			}
		}

		// Percent increment
		float percentChange = 0;
		if (m_playerIncrement != null) {
			percentChange = CurrentState.GetPlayerPercentChange();
			if (percentChange > 0) m_playerIncrement.text = "+"+ Mathf.Round( percentChange * 100 ) + "%";
			else m_playerIncrement.text = "";
		}
		if (m_opponentIncrement != null) {
			percentChange = CurrentState.GetOpponentPercentChange();
			if (percentChange > 0) m_opponentIncrement.text = "+"+ Mathf.Round( percentChange * 100 ) + "%";
			else m_opponentIncrement.text = "";
		}
		
		if (m_addUnitButton != null) {
			bool enable = CurrentState.PlayerCanPlaceSupporter();
			m_addUnitButton.SetEnabled(enable, !enable); // if they can't place supporters, it's probably because of money
			m_addUnitButton.SetPrice(GameMove.GetCost(GameActions.NEW_SUPPORTER));
		}

		if (m_removeUnitButton != null) {
			m_removeUnitButton.SetEnabled(CurrentState.PlayerCanRemoveSupporter());
			m_removeUnitButton.SetPrice(GameMove.GetCost(GameActions.REMOVE_SUPPORTER));
		}
	}

	public void PlaceSupporter() {
		if (CurrentState) {
			CurrentState.PlayerPlaceSupporter();
			RefreshSupporterInfo();
		}
	}

	public void RemoveSupporter() {
		if (CurrentState) {
			CurrentState.PlayerRemoveSupporter();
			RefreshSupporterInfo();
		}
	}
}
