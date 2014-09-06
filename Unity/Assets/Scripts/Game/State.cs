using UnityEngine;
using System;
using System.Collections.Generic;


public enum Leaning
{
  Neutral,
  Red,
  Blue
}

public class State : MonoBehaviour
{
  private Color blueStateColor = new Color( 79.0f / 255.0f, 131.0f / 255.0f, 255.0f / 255.0f );
  private Color redStateColor = new Color( 255.0f / 255.0f, 79.0f / 255.0f, 79.0f / 255.0f );
  private Color undiscoveredStateColor = new Color( 79.0f / 255.0f, 79.0f / 255.0f, 79.0f / 255.0f );
  private Color neutralStateColor = new Color( 156.0f / 255.0f, 103.0f / 255.0f, 152.0f / 255.0f );

  private Vector3 m_workerOffset = new Vector3( 0.5f, 0, 0 );
  private Vector3 m_workerAdjacencyOffset = new Vector3( 0, -0.4f, 0 );
  private Vector3 m_workerCountOffset = new Vector3( 1, 0, 0 );
  private Vector3 m_popularVoteOffset = new Vector3( 0, 0.5f, 0 );

  public Leaning m_stateLeaning;

  public int m_electoralCount;
  public float m_populationInMillions;
  private int m_popularVote;  // blue negative, red positive

  public string m_name;
  public string m_abbreviation;

  public bool m_hidden;
  public bool m_inPlay;

  private List< GameObject > m_playerSupporterList;
  private int m_playerBasisCount;

  private List< GameObject > m_opponentSupporterList;
  private int m_opponentBasisCount;

  private int m_playerSupportersAddedThisTurn;
  private int m_opponentSupportersAddedThisTurn;

  private int m_currentPlayerSupporterIteration;
  private int m_currentOpponentSupporterIteration;
  private GameObject m_playerFloatingText;
  private GameObject m_opponentFloatingText;


  public void Start()
  {
    m_playerSupporterList = new List< GameObject >();
    m_opponentSupporterList = new List< GameObject >();

    m_playerBasisCount = 0;
    m_opponentBasisCount = 0;

    m_playerSupportersAddedThisTurn = 0;

    m_popularVote = 0;

    UpdateColor();
  }


  public bool SendOpponentPlayerSupporters()
  {
    if( m_playerSupportersAddedThisTurn == 0 ) { return PlayerIncrement(); }

    networkView.RPC( "OpponentCreateSupporters", RPCMode.Others, m_playerSupportersAddedThisTurn );

    // TEMP
    m_opponentSupportersAddedThisTurn = 2;
    // TEMP

    m_currentPlayerSupporterIteration = 0;
    m_currentOpponentSupporterIteration = 0;

    m_playerFloatingText = GameObject.Instantiate( GameObjectAccessor.Instance.PulseTextPrefab, Utility.ConvertFromGameToUiPosition( -m_workerCountOffset + gameObject.transform.position ), Quaternion.identity ) as GameObject;
    m_playerFloatingText.GetComponent< FloatingText >().Display( "" );

    m_opponentFloatingText = GameObject.Instantiate( GameObjectAccessor.Instance.PulseTextPrefab, Utility.ConvertFromGameToUiPosition( m_workerCountOffset + gameObject.transform.position ), Quaternion.identity ) as GameObject;
    m_opponentFloatingText.GetComponent< FloatingText >().Display( "" );

    m_playerSupportersAddedThisTurn = 0;
    return false;
  }
  public bool PlayerIncrement()
  {
    if( m_currentPlayerSupporterIteration >= m_playerSupporterList.Count ) { return OpponentIncrement(); }

    CampaignWorker worker = m_playerSupporterList[ m_currentPlayerSupporterIteration ].GetComponent< CampaignWorker >();
    m_playerBasisCount += worker.m_currentLevel;

    worker.gameObject.SendMessage( "BounceOut" );
    m_playerFloatingText.SendMessage( "Display", "" + m_playerBasisCount );
    m_playerFloatingText.SendMessage( "BounceOut" );

    m_currentPlayerSupporterIteration++;

    return false;
  }
  public bool OpponentIncrement()
  {
    if( m_currentOpponentSupporterIteration >= ( m_opponentSupporterList.Count + m_opponentSupportersAddedThisTurn ) ) { return true; }

    if( m_currentOpponentSupporterIteration >= m_opponentSupporterList.Count )
    {
      CreateSupporterPrefab( false );
      m_opponentSupportersAddedThisTurn--;
    }

    CampaignWorker worker = m_opponentSupporterList[ m_currentOpponentSupporterIteration ].GetComponent< CampaignWorker >();
    m_opponentBasisCount += worker.m_currentLevel;
    
    worker.gameObject.SendMessage( "BounceOut" );
    m_opponentFloatingText.SendMessage( "Display", "" + m_opponentBasisCount );
    m_opponentFloatingText.SendMessage( "BounceOut" );
    
    m_currentOpponentSupporterIteration++;

    return false;
  }

  // boolean this returns indicates has completed
  public bool UpdateState()
  {
    return SendOpponentPlayerSupporters();
    /*if( m_playerSupportersAddedThisTurn == 0 )
    {
      return false;
    }

    networkView.RPC ( "OpponentPlaceSupporter", RPCMode.Others );
    m_playerSupportersAddedThisTurn--;

    if( m_playerSupportersAddedThisTurn == 0 )
    {
      // display the floating text
      GameObject floatingText = GameObject.Instantiate( GameObjectAccessor.Instance.FloatingTextPrefab, Utility.ConvertFromGameToUiPosition( gameObject.transform.position ), Quaternion.identity ) as GameObject;
      floatingText.GetComponent< FloatingText >().Display( "+1" );
    }

    return true;*/
  }

  public void UpdateColor()
  {
    if( m_inPlay && m_hidden )
    {
      gameObject.GetComponentInChildren<SpriteRenderer>().color = undiscoveredStateColor;
      return;
    }

    switch( m_stateLeaning )
    {
    case Leaning.Blue:
      gameObject.GetComponentInChildren<SpriteRenderer>().color = blueStateColor;
      break;

    case Leaning.Red:
      gameObject.GetComponentInChildren<SpriteRenderer>().color = redStateColor;
      break;

    default:
      gameObject.GetComponentInChildren<SpriteRenderer>().color = neutralStateColor;
      break;
    }
  }

  public void PlayerPlaceSupporter()
  {
    if( !m_inPlay )  return;
    if( GameObjectAccessor.Instance.GameStateManager.CurrentTurnState != TurnState.Placement )  return;

    if( m_playerSupporterList.Count < m_populationInMillions )
    {
      m_playerSupportersAddedThisTurn++;
      CreateSupporterPrefab( true );

      GameObjectAccessor.Instance.Budget.ConsumeAmount( 30 );
    }
  }

  [RPC]
  public void OpponentCreateSupporters( int count )
  {
    m_opponentSupportersAddedThisTurn = count;
    //CreateSupporterPrefab( false );
  }

  public void CreateSupporterPrefab( bool isPlayer )
  {
    Vector3 supporterPosition = gameObject.transform.position + ( isPlayer ? -m_workerOffset + ( m_playerSupporterList.Count * m_workerAdjacencyOffset ) : m_workerOffset + ( ( m_opponentSupporterList.Count + m_opponentSupportersAddedThisTurn ) * m_workerAdjacencyOffset ) );
    supporterPosition.y += isPlayer ? 0 : 1;

    GameObject newSupporter = GameObject.Instantiate( GameObjectAccessor.Instance.SupporterPrefab, supporterPosition, Quaternion.identity ) as GameObject;
    newSupporter.GetComponent<SpriteRenderer>().color = isPlayer ? GameObjectAccessor.Instance.Player.PlayerColor : GameObjectAccessor.Instance.Player.OpponentColor;

    if( isPlayer )
    {
      m_playerSupporterList.Add( newSupporter );
    }
    else
    {
      m_opponentSupporterList.Add( newSupporter );
    }
  }
}