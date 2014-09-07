﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum TurnState
{
  ConnectPlayers,
  GameBegin,
  Placement,
  Harvest,
  ElectionDay
}

public class GameStateManager : MonoBehaviour
{
  private List< State > m_statesInPlay = new List< State >();
  private List< State > m_statesNotInPlay = new List< State >();

  private TurnState m_currentTurnState;

  private bool m_playerTurnCompleted;
  private bool m_opponentTurnCompleted;

  private int m_weeksLeft;
  public int m_totalElectionWeeks;

  public Timer m_harvestTimer;

  public int m_currentAnte;


  public TurnState CurrentTurnState
  {
    get { return m_currentTurnState; }
  }


  public void Awake()
  {
    // Get all of the existing states
    foreach( Transform child in GameObjectAccessor.Instance.StatesContainer.transform )
    {
      State nextState = child.gameObject.GetComponent< State >();
      //Debug.Log( "Found state: " + nextState.m_name + ", in play: " + nextState.m_inPlay );
			if (nextState != null) {
      if( nextState.m_inPlay )
      {
        m_statesInPlay.Add( nextState );
      }
      else
      {
        m_statesNotInPlay.Add( nextState );
      }
			}
    }

    //Debug.Log( "States in play: " + m_statesInPlay.Count );
    //Debug.Log( "States not in play: " + m_statesNotInPlay.Count );

    m_currentTurnState = TurnState.Placement;
    m_weeksLeft = m_totalElectionWeeks;
  }

  public void Start()
  {
    UpdateElectoralVotes();
  }


  public void NextHarvestAction()
  {
    foreach( State state in m_statesInPlay )
    {
      bool hasCompleted = state.UpdateState();

      if( !hasCompleted )
      {
        return;
      }
    }

    Debug.Log( "completed harvest" );
    m_harvestTimer.StopTimer();


    UpdateElectoralVotes();


    // If we've reached this point, then there are no more harvest actions, we can transition back to Placement
    m_weeksLeft--;
		int week = (m_totalElectionWeeks - m_weeksLeft);
		if (GameObjectAccessor.Instance.WeekMeter != null) GameObjectAccessor.Instance.WeekMeter.Refresh(week);
		GameObjectAccessor.Instance.WeekCounter.text = ((week+1 > m_totalElectionWeeks)? m_totalElectionWeeks : week+1).ToString();
    if( m_weeksLeft < 5 )  m_currentAnte = 50;
    else if( m_weeksLeft < 9 ) m_currentAnte = 30;

    GameObjectAccessor.Instance.Budget.GainAmount( m_currentAnte );

    if( m_weeksLeft == 0 )
    {
      GoToState( TurnState.ElectionDay );
      GameObjectAccessor.Instance.GameOverScreen.SetActive( true );
      GameObjectAccessor.Instance.GameOverRedVotes.text = GameObjectAccessor.Instance.RedVotesLabel.text;
      GameObjectAccessor.Instance.GameOverBlueVotes.text = GameObjectAccessor.Instance.BlueVotesLabel.text;
    }
    else
    {
      m_playerTurnCompleted = false;
      m_opponentTurnCompleted = false;
      GameObjectAccessor.Instance.Player.ToggleCampaignWorker( true );
      GoToState( TurnState.Placement );
    }
  }


  public void GoToState( TurnState nextState )
  {
    m_currentTurnState = nextState;
  }

  public void CheckForHarvest()
  {
    if( m_playerTurnCompleted && ( GameObjectAccessor.Instance.UseAI || m_opponentTurnCompleted ) )
    {
      foreach( State state in m_statesInPlay )
      {
        state.PrepareToUpdate();
      }

      GoToState( TurnState.Harvest );
      m_harvestTimer.StartTimer( NextHarvestAction );
    }
  }


  public void UpdateElectoralVotes()
  {
    int totalRedVotes = 0;
    int totalBlueVotes = 0;
		float totalOpinion = 0;
    foreach( State state in m_statesInPlay )
    {
      if( state.m_stateLeaning == Leaning.Red )
      {
        totalRedVotes += state.m_electoralCount;
      }
      else if( state.m_stateLeaning == Leaning.Blue )
      {
        totalBlueVotes += state.m_electoralCount;
      }
			totalOpinion += state.PopularVote;
    }
    foreach( State state in m_statesNotInPlay )
    {
      if( state.m_stateLeaning == Leaning.Red )
      {
        totalRedVotes += state.m_electoralCount;
      }
      else if( state.m_stateLeaning == Leaning.Blue )
      {
        totalBlueVotes += state.m_electoralCount;
      }
			totalOpinion += state.PopularVote;
		}
    GameObjectAccessor.Instance.RedVotesLabel.text = "" + totalRedVotes;
    GameObjectAccessor.Instance.BlueVotesLabel.text = "" + totalBlueVotes;

		if (GameObjectAccessor.Instance.ElectoralVoteMeter != null)
			GameObjectAccessor.Instance.ElectoralVoteMeter.Refresh(totalBlueVotes, totalRedVotes);
		
		if (GameObjectAccessor.Instance.PopularVoteMeter != null)
			GameObjectAccessor.Instance.PopularVoteMeter.Refresh( totalOpinion / 50f );

  }


  public void CompletePlayerTurn()
  {
    if( m_currentTurnState == TurnState.Placement )
    {
      Debug.Log( "player turn completed!" );

      m_playerTurnCompleted = true;
      CheckForHarvest();

      networkView.RPC( "CompleteOpponentTurn", RPCMode.Others );
    }
  }

  [RPC]
  public void CompleteOpponentTurn()
  {
    Debug.Log( "opponent turn completed!" );

    m_opponentTurnCompleted = true;
    CheckForHarvest();
  }
}