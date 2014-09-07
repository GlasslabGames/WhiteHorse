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
  public static Color blueStateColor = new Color( 26.0f / 255.0f, 94.0f / 255.0f, 255.0f / 255.0f );
  public static Color blueStateColorInactive = new Color( 135.0f / 255.0f, 160.0f / 255.0f, 219.0f / 255.0f );

  public static Color redStateColor = new Color( 255.0f / 255.0f, 0.0f / 255.0f, 0.0f / 255.0f );
  public static Color redStateColorInactive = new Color( 255.0f / 255.0f, 134.0f / 255.0f, 134.0f / 255.0f );

  public static Color undiscoveredStateColor = new Color( 79.0f / 255.0f, 79.0f / 255.0f, 79.0f / 255.0f );
  public static Color neutralStateColor = new Color( 165.0f / 255.0f, 32.0f / 255.0f, 155.0f / 255.0f );

  private Vector3 m_workerOffset = new Vector3( 0.25f, 0, 0 );
  private Vector3 m_workerAdjacencyOffset = new Vector3( 0, -0.2f, 0 );
  private Vector3 m_workerCountOffset = new Vector3( 0.5f, 0, 0 );
  private Vector3 m_popularVoteOffset = new Vector3( 0, 0.3f, 0 );

  public Leaning m_stateLeaning;

  public int m_electoralCount;
  public float m_populationInMillions;
  private float m_popularVote;  // blue negative, red positive

  public string m_name;
  public string m_abbreviation;

  public bool m_hidden;
  public bool m_inPlay;

  private List< GameObject > m_playerSupporterList;
  private int m_playerBasisCount;
  private int m_playerBasisCountIncrement = 0;

  private List< GameObject > m_opponentSupporterList;
  private int m_opponentBasisCount;
  private int m_opponentBasisCountIncrement = 0;

  private int[] m_playerCampaignWorkerCounts = new int[ 3 ];

  private int m_playerSupportersAddedThisTurn;
  private int m_opponentSupportersAddedThisTurn;

  private bool m_playerSupportersSentToOpponent;
  private int m_currentPlayerSupporterIteration;
  private int m_currentOpponentSupporterIteration;
  private bool m_popularVoteUpdatedForPlayer;
  private bool m_popularVoteUpdatedForOpponent;
  private bool m_stateUpdatedWithPopularVote;
  private GameObject m_playerFloatingText;
  private GameObject m_opponentFloatingText;
  private GameObject m_popularVoteText;


  public int PlayerBasisCount
  {
    get { return m_playerBasisCount; }
  }
  public int PlayerBasisCountIncrement
  {
    get { return m_playerBasisCountIncrement; }
  }
  public int OpponentBasisCount
  {
    get { return m_opponentBasisCount; }
  }
  public int OpponentBasisCountIncrement
  {
    get { return m_opponentBasisCountIncrement; }
  }
  public int PlayerSupporters
  {
    get { return m_playerSupporterList.Count; }
  }
  public int OpponentSupporters
  {
    get { return m_opponentSupporterList.Count; }
  }
  public int RoundedPopulation
  {
    get { return (int)m_populationInMillions; }
  }
  public int TotalBasis
  {
    get { return m_playerBasisCount + m_opponentBasisCount; }
  }
  public float PopularVote
  {
    get { return m_popularVote; }
  }
  public int[] PlayerCampaignWorkerCounts
  {
    get { return m_playerCampaignWorkerCounts; }
  }


  public void Start()
  {
    m_playerSupporterList = new List< GameObject >();
    m_opponentSupporterList = new List< GameObject >();

    m_playerBasisCount = 0;
    m_opponentBasisCount = 0;

    m_playerSupportersAddedThisTurn = 0;

    m_popularVote = 0;

    UpdateColor();

    m_playerFloatingText = GameObject.Instantiate( GameObjectAccessor.Instance.PulseTextPrefab, Utility.ConvertFromGameToUiPosition( -m_workerCountOffset + gameObject.transform.position ), Quaternion.identity ) as GameObject;
    m_playerFloatingText.GetComponent< FloatingText >().Display( "" );
    
    m_opponentFloatingText = GameObject.Instantiate( GameObjectAccessor.Instance.PulseTextPrefab, Utility.ConvertFromGameToUiPosition( m_workerCountOffset + gameObject.transform.position ), Quaternion.identity ) as GameObject;
    m_opponentFloatingText.GetComponent< FloatingText >().Display( "" );

    m_popularVoteText = GameObject.Instantiate( GameObjectAccessor.Instance.PulseTextPrefab, Utility.ConvertFromGameToUiPosition( m_popularVoteOffset + gameObject.transform.position ), Quaternion.identity ) as GameObject;
    m_popularVoteText.GetComponent< FloatingText >().Display( "" );
  }


  public bool SendOpponentPlayerSupporters()
  {
    if( m_playerSupportersSentToOpponent ) { return PlayerIncrement(); }

    networkView.RPC( "OpponentCreateSupporters", RPCMode.Others, m_playerSupportersAddedThisTurn );

    // TEMP
    if( GameObjectAccessor.Instance.UseAI )
    {
      m_opponentSupportersAddedThisTurn = m_playerSupporterList.Count == 0 ? 0 : 2;
    }
    // TEMP

    m_currentPlayerSupporterIteration = 0;
    m_currentOpponentSupporterIteration = 0;

    m_playerSupportersAddedThisTurn = 0;

    m_playerSupportersSentToOpponent = true;

    return false;
  }
  public bool PlayerIncrement()
  {
    if( m_currentPlayerSupporterIteration >= m_playerSupporterList.Count ) { return UpdatePopularVotePlayer(); }

    CampaignWorker worker = m_playerSupporterList[ m_currentPlayerSupporterIteration ].GetComponent< CampaignWorker >();
    m_playerBasisCount += worker.m_currentLevel;

    worker.gameObject.SendMessage( "BounceOut" );
    m_playerFloatingText.SendMessage( "Display", "" + m_playerBasisCount );
    m_playerFloatingText.SendMessage( "BounceOut" );

    m_currentPlayerSupporterIteration++;

    return false;
  }
  public bool UpdatePopularVotePlayer()
  {
    float totalBasis = TotalBasis;
    
    if( m_popularVoteUpdatedForPlayer || totalBasis == 0 )  return OpponentIncrement();
    
    float playerPercentage = ( m_playerBasisCount / totalBasis );
    float opponentPercentage = ( m_opponentBasisCount / totalBasis );

    if( GameObjectAccessor.Instance.Player.m_leaning == Leaning.Red )
    {
      m_popularVote = -playerPercentage + opponentPercentage;
    }
    else
    {
      m_popularVote = playerPercentage - opponentPercentage;
    }

    playerPercentage *= m_populationInMillions;
    opponentPercentage *= m_populationInMillions;
    
    m_popularVoteText.SendMessage( "Display", playerPercentage.ToString( "0.0" ) + "m | " + opponentPercentage.ToString( "0.0" ) + "m" );
    m_popularVoteText.SendMessage( "BounceOut" );
    
    m_popularVoteUpdatedForPlayer = true;
    
    return false;
  }
  public bool OpponentIncrement()
  {
    if( m_currentOpponentSupporterIteration >= ( m_opponentSupporterList.Count + m_opponentSupportersAddedThisTurn ) ) { return UpdatePopularVoteOpponent(); }

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
  public bool UpdatePopularVoteOpponent()
  {
    float totalBasis = TotalBasis;
    
    if( m_popularVoteUpdatedForOpponent || totalBasis == 0 )  return UpdateStateWithPopularVote();
    
    float playerPercentage = ( m_playerBasisCount / totalBasis );
    float opponentPercentage = ( m_opponentBasisCount / totalBasis );

    if( GameObjectAccessor.Instance.Player.m_leaning == Leaning.Red )
    {
      m_popularVote = -playerPercentage + opponentPercentage;
    }
    else
    {
      m_popularVote = playerPercentage - opponentPercentage;
    }
    
    playerPercentage *= m_populationInMillions;
    opponentPercentage *= m_populationInMillions;
    
    m_popularVoteText.SendMessage( "Display", playerPercentage.ToString( "0.0" ) + "m | " + opponentPercentage.ToString( "0.0" ) + "m" );
    m_popularVoteText.SendMessage( "BounceOut" );
    
    m_popularVoteUpdatedForOpponent = true;
    
    return false;
  }
  public bool UpdateStateWithPopularVote()
  {
    float totalBasis = TotalBasis;

    if( m_stateUpdatedWithPopularVote || totalBasis == 0 ) return true;

    if( m_playerBasisCount > m_opponentBasisCount ) m_stateLeaning = GameObjectAccessor.Instance.Player.m_leaning;
    else if( m_playerBasisCount < m_opponentBasisCount ) m_stateLeaning = GameObjectAccessor.Instance.Player.m_opponentLeaning;
    else m_stateLeaning = Leaning.Neutral;

    UpdateColor();

    m_stateUpdatedWithPopularVote = true;

    return false;
  }

  public void PrepareToUpdate()
  {
    m_playerSupportersSentToOpponent = false;
    m_popularVoteUpdatedForPlayer = false;
    m_popularVoteUpdatedForOpponent = false;
    m_stateUpdatedWithPopularVote = false;
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
    if( m_hidden )
    {
      gameObject.GetComponentInChildren<SpriteRenderer>().color = undiscoveredStateColor;
      return;
    }

    switch( m_stateLeaning )
    {
    case Leaning.Blue:
      gameObject.GetComponentInChildren<SpriteRenderer>().color = m_inPlay ? blueStateColor : blueStateColorInactive;
      break;

    case Leaning.Red:
      gameObject.GetComponentInChildren<SpriteRenderer>().color = m_inPlay ? redStateColor : redStateColorInactive;
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
    if( !GameObjectAccessor.Instance.Budget.IsAmountAvailable( 10 ) ) return;
    if( !GameObjectAccessor.Instance.Player.m_campaignWorkerSelected ) return;

    if( m_playerSupporterList.Count < m_populationInMillions )
    {
      m_playerSupportersAddedThisTurn++;
      CreateSupporterPrefab( true );

      GameObjectAccessor.Instance.Budget.ConsumeAmount( 10 );
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
    Vector3 supporterPosition = gameObject.transform.position + ( isPlayer ? -m_workerOffset + ( m_playerSupporterList.Count * m_workerAdjacencyOffset ) : m_workerOffset + ( ( m_opponentSupporterList.Count ) * m_workerAdjacencyOffset ) );

    GameObject newSupporter = GameObject.Instantiate( GameObjectAccessor.Instance.SupporterPrefab, supporterPosition, Quaternion.identity ) as GameObject;
    newSupporter.GetComponent<SpriteRenderer>().color = isPlayer ? GameObjectAccessor.Instance.Player.PlayerColor : GameObjectAccessor.Instance.Player.OpponentColor;

    if( isPlayer )
    {
      m_playerSupporterList.Add( newSupporter );
      m_playerBasisCountIncrement += newSupporter.GetComponent< CampaignWorker >().GetValueForLevel();
      m_playerCampaignWorkerCounts[ 0 ]++;
    }
    else
    {
      m_opponentSupporterList.Add( newSupporter );
      m_opponentBasisCountIncrement += newSupporter.GetComponent< CampaignWorker >().GetValueForLevel();
    }
  }
}