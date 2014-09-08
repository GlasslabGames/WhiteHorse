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
  public static float populationPerWorker = 1.2f;

  private Vector3 m_workerOffsetX = new Vector3( -0.4f, 0, 0 );
  private Vector3 m_workerOffsetY = new Vector3( 0, 0.25f, 0 );
  private Vector3 m_workerAdjacencyOffset = new Vector3( 0.15f, 0, 0 );
  private Vector3 m_workerCountOffset = new Vector3( -0.5f, 0, 0 );
  private Vector3 m_popularVoteOffset = new Vector3( 0, 0.6f, 0 );

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
  private List< int > m_nextOpponentSupporterList;
  private int m_opponentBasisCount;
  private int m_opponentBasisCountIncrement = 0;

  private int[] m_playerCampaignWorkerCounts = new int[ 3 ];
  private int[] m_opponentCampaignWorkerCounts = new int[ 3 ];

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
  public int PlayerCampaignWorkers
  {
    get { return m_playerSupporterList.Count; }
  }
	public int OpponentCampaignWorkers
	{
    get { return m_opponentSupporterList.Count; }
  }
  public int RoundedPopulation
  {
    get { return Mathf.CeilToInt(m_populationInMillions); }
  }
  public int UnitCap
  {
    get { return Mathf.CeilToInt( m_populationInMillions / State.populationPerWorker ); }
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

	private SpriteRenderer m_stateColor;
	private SpriteRenderer m_stateOutline;
	private SpriteRenderer m_stateStripes;


  public void Start()
  {
    m_playerSupporterList = new List< GameObject >();
    m_opponentSupporterList = new List< GameObject >();
    m_nextOpponentSupporterList = new List< int >();

    m_playerBasisCount = 0;
    m_opponentBasisCount = 0;

    m_playerSupportersAddedThisTurn = 0;

    m_popularVote = 0;

		// for inactive states, set the popular vote based on the leaning
		if (!m_inPlay) {
			if (m_stateLeaning == Leaning.Red) m_popularVote = -1;
			else if (m_stateLeaning == Leaning.Blue) m_popularVote = 1;
		}

		// automatically figure out which of the child textures are which
		foreach (SpriteRenderer t in GetComponentsInChildren<SpriteRenderer>(true)) {
			if (t.name.Contains("dashed")) m_stateStripes = t;
			else if (t.name.Contains("oline")) m_stateOutline = t;
			else m_stateColor = t;
		}

		if (m_stateStripes == null) Debug.LogError ("No stripes on " + m_name, this);
		if (m_stateOutline == null) Debug.LogError ("No outline on " + m_name, this);

    UpdateColor();
	
		Transform container = GameObjectAccessor.Instance.FloatingTextContainer.transform;

    m_playerFloatingText = GameObject.Instantiate( GameObjectAccessor.Instance.PulseTextPrefab, Utility.ConvertFromGameToUiPosition( m_workerOffsetY + m_workerCountOffset + gameObject.transform.position ), Quaternion.identity ) as GameObject;
    m_playerFloatingText.GetComponent< FloatingText >().Display( "" );
		m_playerFloatingText.transform.parent = container;
    
    m_opponentFloatingText = GameObject.Instantiate( GameObjectAccessor.Instance.PulseTextPrefab, Utility.ConvertFromGameToUiPosition( -m_workerOffsetY + m_workerCountOffset + gameObject.transform.position ), Quaternion.identity ) as GameObject;
    m_opponentFloatingText.GetComponent< FloatingText >().Display( "" );
		m_opponentFloatingText.transform.parent = container;
		
		m_popularVoteText = GameObject.Instantiate( GameObjectAccessor.Instance.PulseTextPrefab, Utility.ConvertFromGameToUiPosition( m_popularVoteOffset + gameObject.transform.position ), Quaternion.identity ) as GameObject;
    m_popularVoteText.GetComponent< FloatingText >().Display( "" );
		m_popularVoteText.transform.parent = container;
	}


  public bool SendOpponentPlayerSupporters()
  {
    if( m_playerSupportersSentToOpponent ) { return PlayerIncrement(); }

    networkView.RPC( "OpponentCreateSupporters", RPCMode.Others, new Vector3( PlayerCampaignWorkerCounts[0], PlayerCampaignWorkerCounts[1], PlayerCampaignWorkerCounts[2] ) );

    // TEMP
    if( GameObjectAccessor.Instance.UseAI && m_playerSupporterList.Count > 0 )
    {
      m_nextOpponentSupporterList.Add( 1 );
      m_nextOpponentSupporterList.Add( 1 );
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
    m_playerBasisCount += worker.GetValueForLevel();

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
    if( m_currentOpponentSupporterIteration >= Math.Max( m_opponentSupporterList.Count, m_nextOpponentSupporterList.Count ) ) { return UpdatePopularVoteOpponent(); }


    CampaignWorker workerToChange = null;

    // Still within current list?
    if( m_currentOpponentSupporterIteration < m_opponentSupporterList.Count )
    {
      // Still within next list?
      if( m_currentOpponentSupporterIteration < m_nextOpponentSupporterList.Count )
      {
        // compare and update
        CampaignWorker worker = m_opponentSupporterList[ m_currentOpponentSupporterIteration ].GetComponent< CampaignWorker >();
        //CampaignWorker compareWorker = m_nextOpponentSupporterList[ m_currentOpponentSupporterIteration ].GetComponent< CampaignWorker >();
        while( worker.m_currentLevel < m_nextOpponentSupporterList[ m_currentOpponentSupporterIteration ] )
        {
          m_opponentBasisCountIncrement -= worker.GetValueForLevel();
          worker.Upgrade();
          m_opponentBasisCountIncrement += worker.GetValueForLevel();
        }

        // set the worker
        workerToChange = worker;
      }
      // Not within next list
      else
      {
        // no change
        CampaignWorker worker = m_opponentSupporterList[ m_currentOpponentSupporterIteration ].GetComponent< CampaignWorker >();

        // set the worker
        workerToChange = worker;
      }
    }
    // Not within current list, still within next list?
    else if( m_currentOpponentSupporterIteration < m_nextOpponentSupporterList.Count )
    {
      // add and potentially upgrade
      CreateSupporterPrefab( false );
      CampaignWorker worker = m_opponentSupporterList[ m_currentOpponentSupporterIteration ].GetComponent< CampaignWorker >();
      //CampaignWorker compareWorker = m_nextOpponentSupporterList[ m_currentOpponentSupporterIteration ].GetComponent< CampaignWorker >();
      int compareWorker = m_nextOpponentSupporterList[ m_currentOpponentSupporterIteration ];

      m_opponentBasisCountIncrement -= worker.GetValueForLevel();
      if( compareWorker == 2 )
      {
        worker.Upgrade();
      }
      if( compareWorker == 3 )
      {
        worker.Upgrade();
        worker.Upgrade();
      }
      m_opponentBasisCountIncrement += worker.GetValueForLevel();
            
      // set the worker
      workerToChange = worker;
    }


    // animate
    if( workerToChange != null )
    {
      m_opponentBasisCount += workerToChange.GetValueForLevel();
      workerToChange.gameObject.SendMessage( "BounceOut" );
      m_opponentFloatingText.SendMessage( "Display", "" + m_opponentBasisCount );
      m_opponentFloatingText.SendMessage( "BounceOut" );
    }


    // Update the current supporter iteration
    m_currentOpponentSupporterIteration++;


    /*if( m_currentOpponentSupporterIteration >= m_opponentSupporterList.Count )
    {
      CreateSupporterPrefab( false );
      m_opponentSupportersAddedThisTurn--;
    }

    CampaignWorker worker = m_opponentSupporterList[ m_currentOpponentSupporterIteration ].GetComponent< CampaignWorker >();
    m_opponentBasisCount += worker.GetValueForLevel();
    
    worker.gameObject.SendMessage( "BounceOut" );
    m_opponentFloatingText.SendMessage( "Display", "" + m_opponentBasisCount );
    m_opponentFloatingText.SendMessage( "BounceOut" );
    
    m_currentOpponentSupporterIteration++;*/

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

    if( GameObjectAccessor.Instance.UseAI )
    {
      m_nextOpponentSupporterList.Clear();
    }
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
      m_stateColor.color = GameObjectAccessor.Instance.GameColorSettings.undiscoveredState;
      m_stateOutline.color = GameObjectAccessor.Instance.GameColorSettings.outline;
			m_stateStripes.enabled = true;
      return;
    }

		float t = Mathf.Abs(m_popularVote) * 0.8f + 0.2f; // 0.2 - 1 so that we don't go all the way to purple

		switch (m_stateLeaning) {
		case Leaning.Blue:
			m_stateColor.color = Color.Lerp (GameObjectAccessor.Instance.GameColorSettings.neutralState, GameObjectAccessor.Instance.GameColorSettings.blueState, t);
			m_stateOutline.color = GameObjectAccessor.Instance.GameColorSettings.outline;
			m_stateOutline.sortingOrder = -7;
			break;

		case Leaning.Red:
			m_stateColor.color = Color.Lerp (GameObjectAccessor.Instance.GameColorSettings.neutralState, GameObjectAccessor.Instance.GameColorSettings.redState, t);
			m_stateOutline.color = GameObjectAccessor.Instance.GameColorSettings.outline;
			m_stateOutline.sortingOrder = -7;
			break;

		default:
			m_stateColor.color = GameObjectAccessor.Instance.GameColorSettings.neutralState;
			m_stateOutline.color = GameObjectAccessor.Instance.GameColorSettings.neutralOutline;
			m_stateOutline.sortingOrder = -6;
			break;
		}

		m_stateStripes.enabled = !m_inPlay;
	}
	
	public void Highlight(bool active) {
    if( GameObjectAccessor.Instance.GameStateManager.CurrentTurnState == TurnState.ConnectPlayers )  return;

		if (active) {
			m_stateOutline.color = GameObjectAccessor.Instance.GameColorSettings.highlightOutline;
			m_stateOutline.sortingOrder = -5;
			//transform.localScale = new Vector3(1.1f, 1.1f, 1f);
		} else {
			UpdateColor(); // reset
			//transform.localScale = Vector3.one;
		}
	}
		
	public void PlayerPlaceSupporter()
  {
    if( !m_inPlay )  return;
    if( GameObjectAccessor.Instance.GameStateManager.CurrentTurnState != TurnState.Placement )  return;
    if( !GameObjectAccessor.Instance.Budget.IsAmountAvailable( 10 ) ) return;
    if( !GameObjectAccessor.Instance.Player.m_campaignWorkerSelected ) return;

    if( m_playerSupporterList.Count < UnitCap )
    {
      m_playerSupportersAddedThisTurn++;
      CreateSupporterPrefab( true );

      GameObjectAccessor.Instance.Budget.ConsumeAmount( 10 );
    }
  }

  [RPC]
  public void OpponentCreateSupporters( Vector3 nextOpponentsList )
  {
    Debug.Log ( nextOpponentsList );
    m_nextOpponentSupporterList.Clear();
    
    for( int i = 2; i >= 0; i-- )
    {
      for( int j = 0; j < nextOpponentsList[ i ]; j++ )
      {
        m_nextOpponentSupporterList.Add( i + 1 );
      }
    }


    //m_opponentSupportersAddedThisTurn = count;
    //m_nextOpponentCampaignWorkerCounts = counts;
    //m_nextOpponentSupporterList = nextOpponentsList;
      Debug.Log ( m_nextOpponentSupporterList.Count );
      for( int i = 0; i < m_nextOpponentSupporterList.Count; i++ )
      {
        Debug.Log ( m_nextOpponentSupporterList[i] );
      }
    //m_opponentCampaignWorkerCounts = counts;
    //CreateSupporterPrefab( false );
  }

  public void CreateSupporterPrefab( bool isPlayer )
  {
    Vector3 supporterPosition = gameObject.transform.position + m_workerOffsetX + ( isPlayer ? m_workerOffsetY + ( m_playerSupporterList.Count * m_workerAdjacencyOffset ) : -m_workerOffsetY + ( ( m_opponentSupporterList.Count ) * m_workerAdjacencyOffset ) );

    GameObject newSupporter = GameObject.Instantiate( GameObjectAccessor.Instance.SupporterPrefab, supporterPosition, Quaternion.identity ) as GameObject;

    if( isPlayer )
    {
      newSupporter.GetComponent<SpriteRenderer>().color = GameObjectAccessor.Instance.Player.m_leaning == Leaning.Red ?
				GameObjectAccessor.Instance.GameColorSettings.redStateDark : GameObjectAccessor.Instance.GameColorSettings.blueStateDark;

      m_playerSupporterList.Add( newSupporter );
      m_playerBasisCountIncrement += newSupporter.GetComponent< CampaignWorker >().GetValueForLevel();
      m_playerCampaignWorkerCounts[ 0 ]++;
    }
    else
    {
      newSupporter.GetComponent<SpriteRenderer>().color = GameObjectAccessor.Instance.Player.m_opponentLeaning == Leaning.Red ?
				GameObjectAccessor.Instance.GameColorSettings.redStateDark : GameObjectAccessor.Instance.GameColorSettings.blueStateDark;

      m_opponentSupporterList.Add( newSupporter );
      m_opponentBasisCountIncrement += newSupporter.GetComponent< CampaignWorker >().GetValueForLevel();
    }
  }

  public void Upgrade1( bool bounce = true )
  {
    if( GameObjectAccessor.Instance.Budget.IsAmountAvailable( 15 ) && m_playerCampaignWorkerCounts[ 0 ] > 0 )
    {
      m_playerCampaignWorkerCounts[ 0 ]--;
      m_playerCampaignWorkerCounts[ 1 ]++;

      for( int i = 0; i < m_playerSupporterList.Count; i++ )
      {
        CampaignWorker worker = m_playerSupporterList[ i ].GetComponent< CampaignWorker >();
        if( worker.m_currentLevel == 1 )
        {
          m_playerBasisCountIncrement -= worker.GetValueForLevel();
          worker.Upgrade();
          m_playerBasisCountIncrement += worker.GetValueForLevel();

          if( bounce )  worker.gameObject.SendMessage( "BounceOut" );
          GameObjectAccessor.Instance.Budget.ConsumeAmount( 15 );
          GameObjectAccessor.Instance.DetailView.SetState( GameObjectAccessor.Instance.DetailView.CurrentState, false );
          break;
        }
      }
    }
  }
  public void Upgrade2( bool bounce = true )
  {
    if( GameObjectAccessor.Instance.Budget.IsAmountAvailable( 20 ) && m_playerCampaignWorkerCounts[ 1 ] > 0 )
    {
      m_playerCampaignWorkerCounts[ 1 ]--;
      m_playerCampaignWorkerCounts[ 2 ]++;

      for( int i = 0; i < m_playerSupporterList.Count; i++ )
      {
        CampaignWorker worker = m_playerSupporterList[ i ].GetComponent< CampaignWorker >();
        if( worker.m_currentLevel == 2 )
        {
          m_playerBasisCountIncrement -= worker.GetValueForLevel();
          worker.Upgrade();
          m_playerBasisCountIncrement += worker.GetValueForLevel();

          if( bounce )  worker.gameObject.SendMessage( "BounceOut" );
          GameObjectAccessor.Instance.Budget.ConsumeAmount( 20 );
          GameObjectAccessor.Instance.DetailView.SetState( GameObjectAccessor.Instance.DetailView.CurrentState, false );
          break;
        }
      }
    }
  }
}