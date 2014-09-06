using UnityEngine;
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

  private int m_currentElectionWeek;
  public int m_totalElectionWeeks;

  public Timer m_harvestTimer;


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
      Debug.Log( "Found state: " + nextState.m_name + ", in play: " + nextState.m_inPlay );

      if( nextState.m_inPlay )
      {
        m_statesInPlay.Add( nextState );
      }
      else
      {
        m_statesNotInPlay.Add( nextState );
      }
    }

    Debug.Log( "States in play: " + m_statesInPlay.Count );
    Debug.Log( "States not in play: " + m_statesNotInPlay.Count );

    m_currentTurnState = TurnState.Placement;
    m_currentElectionWeek = 1;
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

    Debug.Log( "outside harvest" );

    // If we've reached this point, then there are no more harvest actions, we can transition back to Placement
    m_currentElectionWeek++;
    if( m_currentElectionWeek >= m_totalElectionWeeks )
    {
      GoToState( TurnState.ElectionDay );
    }
    else
    {
      GoToState( TurnState.Placement );
    }
  }


  public void GoToState( TurnState nextState )
  {
    m_currentTurnState = nextState;
  }

  public void CheckForHarvest()
  {
    if( m_playerTurnCompleted || m_opponentTurnCompleted )
    {
      GoToState( TurnState.Harvest );
      m_harvestTimer.StartTimer( NextHarvestAction );
    }
  }

  public void CompletePlayerTurn()
  {
    Debug.Log( "player turn completed!" );

    m_playerTurnCompleted = true;
    CheckForHarvest();

    networkView.RPC( "CompleteOpponentTurn", RPCMode.Others );
  }

  [RPC]
  public void CompleteOpponentTurn()
  {
    Debug.Log( "opponent turn completed!" );

    m_opponentTurnCompleted = true;
    CheckForHarvest();
  }
}