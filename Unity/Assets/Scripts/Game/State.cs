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
  private StateModel m_model;
  public StateModel Model {
    get {
      if (m_model == null) {
        m_model = StateModel.GetModelByAbbreviation(m_abbreviation);
        if (m_model == null) Debug.LogError("Couldn't find a model for "+m_abbreviation, this);
      }
      return m_model;
    }
  }

  public static float populationPerWorker = 1.2f;

  private Vector3 m_workerOffsetX = new Vector3( -0.4f, 0, 0 );
  private Vector3 m_workerOffsetY = new Vector3( 0, 0.25f, 0 );
  private Vector3 m_workerAdjacencyOffset = new Vector3( 0.15f, 0, 0 );
  private Vector3 m_workerCountOffset = new Vector3( -0.5f, 0, 0 );
  private Vector3 m_popularVoteOffset = new Vector3( 0, 0.6f, 0 );

  private float m_popularVote;  // red negative, blue positive
  private Leaning m_previousLeaning; // remember our previous leaning so we can make a big deal about changing

  public string m_abbreviation;
  
  public bool InPlay { get; set; }

  private bool m_hidden; // we're not using this

  private List< GameObject > m_playerSupporterList = new List<GameObject>();
  private int m_playerBasisCount = 0;
  private int m_playerBasisCountIncrement = 0;

  private List< GameObject > m_opponentSupporterList = new List<GameObject>();
  private List< int > m_nextOpponentSupporterList = new List< int >();
  public List<int> NextOpponentSupporters { get { return m_nextOpponentSupporterList; } }
  private int m_opponentBasisCount = 0;
  private int m_opponentBasisCountIncrement = 0;

	// NEW
	public int NextOpponentSupporterCount = 0;

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

  public bool m_dirty = false;
  public bool m_receivedOpponentInfo = false;


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
    get { return Mathf.CeilToInt(Model.Population); }
  }
  public int UnitCap
  {
    get { return Mathf.CeilToInt( Model.Population / State.populationPerWorker ); }
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
  public int[] OpponentCampaignWorkerCounts
  {
    get { return m_opponentCampaignWorkerCounts; }
  }

  // Note: these could be adjusted to keep a margin of neutrality ( < 0.1 and > 0.1, for example)
  public bool IsRed { get { return PopularVote < 0; } }
  public bool IsBlue { get { return PopularVote > 0; } }

	private SpriteRenderer m_stateColor;
	private SpriteRenderer m_stateOutline;
	private SpriteRenderer m_stateStripes;

	private Transform m_center;
	public Vector3 Center {
		get {
			if (m_center == null) {
				m_center = transform.Find ("uiAnchor");
				if (m_center == null) m_center = transform;
			}
			return m_center.position;
		}
	}
	public Vector3 UiCenter {
		get {
			return Utility.ConvertFromGameToUiPosition(Center);
		}
	}
	
  void Awake() {
    // automatically figure out which of the child textures are which
    foreach (SpriteRenderer t in GetComponentsInChildren<SpriteRenderer>(true)) {
      if (t.name.Contains("dashed")) m_stateStripes = t;
      else if (t.name.Contains("oline")) m_stateOutline = t;
      else m_stateColor = t;
    }
    
    if (m_stateStripes == null) Debug.LogError ("No stripes on " + this, this);
    if (m_stateOutline == null) Debug.LogError ("No outline on " + this, this);
  }

  public void Start()
  {
    UpdateColor();
	
		Transform container = GameObjectAccessor.Instance.FloatingTextContainer.transform;

    m_playerFloatingText = GameObject.Instantiate( GameObjectAccessor.Instance.PulseTextPrefab, Utility.ConvertFromGameToUiPosition( m_workerOffsetY + m_workerCountOffset + Center ), Quaternion.identity ) as GameObject;
    m_playerFloatingText.GetComponent< FloatingText >().Display( "" );
		m_playerFloatingText.transform.parent = container;
    
    m_opponentFloatingText = GameObject.Instantiate( GameObjectAccessor.Instance.PulseTextPrefab, Utility.ConvertFromGameToUiPosition( -m_workerOffsetY + m_workerCountOffset + Center ), Quaternion.identity ) as GameObject;
    m_opponentFloatingText.GetComponent< FloatingText >().Display( "" );
		m_opponentFloatingText.transform.parent = container;
		
		m_popularVoteText = GameObject.Instantiate( GameObjectAccessor.Instance.PulseTextPrefab, Utility.ConvertFromGameToUiPosition( m_popularVoteOffset + Center ), Quaternion.identity ) as GameObject;
    m_popularVoteText.GetComponent< FloatingText >().Display( "" );
		m_popularVoteText.transform.parent = container;
	}

  public void SetInitialPopularVote(float v) {
    m_popularVote = v;
    //UpdateColor();
  }

  public bool SendOpponentPlayerSupporters()
  {
    if( m_playerSupportersSentToOpponent ) { return PlayerIncrement(); }

    //if( m_dirty )
    {
      networkView.RPC( "OpponentCreateSupporters", RPCMode.Others, new Vector3( PlayerCampaignWorkerCounts[0], PlayerCampaignWorkerCounts[1], PlayerCampaignWorkerCounts[2] ) );
    }

    // TEMP

    if( GameObjectAccessor.Instance.UseAI )
    {
      m_receivedOpponentInfo = true;
      /*if( m_playerSupporterList.Count > 0 )
      {
        m_nextOpponentSupporterList.Add( 1 );
        m_nextOpponentSupporterList.Add( 1 );
      }*/

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

    playerPercentage *= Model.Population;
    opponentPercentage *= Model.Population;
    
    m_popularVoteText.SendMessage( "Display", playerPercentage.ToString( "0.0" ) + "m | " + opponentPercentage.ToString( "0.0" ) + "m" );
    m_popularVoteText.SendMessage( "BounceOut" );
    
    m_popularVoteUpdatedForPlayer = true;
    
    return OpponentIncrement();
  }
  public bool OpponentIncrement()
  {
    if( !m_receivedOpponentInfo )
    {
      return false;
    }
		
		// We've iterated over all the supporters (none are left from last turn or new this turn)
		if( m_currentOpponentSupporterIteration >= Math.Max( m_opponentSupporterList.Count, NextOpponentSupporterCount ) ) {
			return UpdatePopularVoteOpponent();
		}

		if (m_opponentSupporterList.Count > 0 || NextOpponentSupporterCount > 0) {
			Debug.Log ("Iteration for "+m_abbreviation+": " + m_currentOpponentSupporterIteration + " / " + m_opponentSupporterList.Count + " -> " + NextOpponentSupporterCount);
		}

    CampaignWorker workerToChange = null;

    // Still iterating within the number of supporters they had last time
    if( m_currentOpponentSupporterIteration < m_opponentSupporterList.Count )
    {
      // Still iterating within their new number of supporters?
			if( m_currentOpponentSupporterIteration < NextOpponentSupporterCount )
			{
				Debug.Log ("Supporter "+m_currentOpponentSupporterIteration+" is still here.");
        // compare and update
        CampaignWorker worker = m_opponentSupporterList[ m_currentOpponentSupporterIteration ].GetComponent< CampaignWorker >();
        //CampaignWorker compareWorker = m_nextOpponentSupporterList[ m_currentOpponentSupporterIteration ].GetComponent< CampaignWorker >();
        /*while( worker.m_currentLevel < m_nextOpponentSupporterList[ m_currentOpponentSupporterIteration ] )
        {
          m_opponentBasisCountIncrement -= worker.GetValueForLevel();
          worker.Upgrade();
          m_opponentBasisCountIncrement += worker.GetValueForLevel();
        }*/

        // set the worker
        workerToChange = worker;
      }
      // Not within the list of new supporters - so delete it :?
      else
      {
				Debug.Log ("Supporter "+m_currentOpponentSupporterIteration+" was removed.");

				// no change
        CampaignWorker worker = m_opponentSupporterList[ m_currentOpponentSupporterIteration ].GetComponent< CampaignWorker >();

        // set the worker
        // workerToChange = worker;

				// Remove that worker
				m_opponentBasisCountIncrement -= worker.GetValueForLevel();
				m_opponentSupporterList.Remove(worker.gameObject);
				Destroy (worker.gameObject);
      }
    }
    // Not within current list, still within next list = it's new
    else if( m_currentOpponentSupporterIteration < NextOpponentSupporterCount )
    {
			Debug.Log ("Supporter "+m_currentOpponentSupporterIteration+" is new!");

			// add and potentially upgrade
      CreateSupporterPrefab( false );
      CampaignWorker worker = m_opponentSupporterList[ m_currentOpponentSupporterIteration ].GetComponent< CampaignWorker >();
      //CampaignWorker compareWorker = m_nextOpponentSupporterList[ m_currentOpponentSupporterIteration ].GetComponent< CampaignWorker >();
      /*int compareWorker = m_nextOpponentSupporterList[ m_currentOpponentSupporterIteration ];

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
      */
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
    
    playerPercentage *= Model.Population;
    opponentPercentage *= Model.Population;
    
    m_popularVoteText.SendMessage( "Display", playerPercentage.ToString( "0.0" ) + "m | " + opponentPercentage.ToString( "0.0" ) + "m" );
    m_popularVoteText.SendMessage( "BounceOut" );
    
    m_popularVoteUpdatedForOpponent = true;
    
    return UpdateStateWithPopularVote();
  }
  public bool UpdateStateWithPopularVote()
  {
    float totalBasis = TotalBasis;

    if( m_stateUpdatedWithPopularVote || totalBasis == 0 ) return true;

    Leaning newLeaning;
    if( m_playerBasisCount > m_opponentBasisCount ) newLeaning = GameObjectAccessor.Instance.Player.m_leaning;
    else if( m_playerBasisCount < m_opponentBasisCount ) newLeaning = GameObjectAccessor.Instance.Player.m_opponentLeaning;
    else newLeaning = Leaning.Neutral;

    UpdateColor( newLeaning != m_previousLeaning );
    m_previousLeaning = newLeaning;

    m_stateUpdatedWithPopularVote = true;

    return true;
  }

  public void PrepareToUpdate()
  {
    m_playerSupportersSentToOpponent = false;
    m_popularVoteUpdatedForPlayer = false;
    m_popularVoteUpdatedForOpponent = false;
    m_stateUpdatedWithPopularVote = false;

    if( GameObjectAccessor.Instance.UseAI )
    {
      //m_nextOpponentSupporterList.Clear();
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

  public void UpdateColor( bool playParticles = false )
  {
    if( m_hidden )
    {
      m_stateColor.color = GameObjectAccessor.Instance.GameColorSettings.undiscoveredState;
      m_stateOutline.color = GameObjectAccessor.Instance.GameColorSettings.outline;
			m_stateStripes.enabled = true;
      return;
    }

		float t = Mathf.Abs(m_popularVote) * 0.8f + 0.2f; // 0.2 - 1 so that we don't go all the way to purple

		if (IsBlue) {
			m_stateColor.color = Color.Lerp (GameObjectAccessor.Instance.GameColorSettings.neutralState, GameObjectAccessor.Instance.GameColorSettings.blueState, t);
			m_stateOutline.color = GameObjectAccessor.Instance.GameColorSettings.outline;
			m_stateOutline.sortingOrder = -7;
    } else if (IsRed) {
			m_stateColor.color = Color.Lerp (GameObjectAccessor.Instance.GameColorSettings.neutralState, GameObjectAccessor.Instance.GameColorSettings.redState, t);
			m_stateOutline.color = GameObjectAccessor.Instance.GameColorSettings.outline;
			m_stateOutline.sortingOrder = -7;
    } else {
      if (InPlay) {
			  m_stateColor.color = GameObjectAccessor.Instance.GameColorSettings.neutralState;
      } else {
        m_stateColor.color = GameObjectAccessor.Instance.GameColorSettings.neutralLockedState;
      }

			m_stateOutline.color = GameObjectAccessor.Instance.GameColorSettings.neutralOutline;
			m_stateOutline.sortingOrder = -6;
		}

    if( playParticles )
    {
      GameObject.Instantiate( GameObjectAccessor.Instance.FlipStateParticleSystemBlue, new Vector3( gameObject.transform.position.x, gameObject.transform.position.y, -0.5f ), Quaternion.identity );
    }

		m_stateStripes.enabled = !InPlay;
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
		
	public void PlayerPlaceSupporter(bool definitely = false)
  {
    if( !InPlay )  return;
    if( GameObjectAccessor.Instance.GameStateManager.CurrentTurnState != TurnState.Placement )  return;
    if( !GameObjectAccessor.Instance.Budget.IsAmountAvailable( GameMove.GetCost(GameActions.NEW_SUPPORTER)) ) return;
    if( !definitely && !GameObjectAccessor.Instance.Player.m_campaignWorkerSelected ) return;

		// we're not capping the supporters per state anymore
    //if( m_playerSupporterList.Count < UnitCap ) {
      m_playerSupportersAddedThisTurn++;
      CreateSupporterPrefab( true );

      GameObjectAccessor.Instance.Budget.ConsumeAmount( GameMove.GetCost(GameActions.NEW_SUPPORTER) );

      m_dirty = true;
    //}
  }

	public void PlayerRemoveSupporter()
	{
				if (!InPlay)
						return;
				if (GameObjectAccessor.Instance.GameStateManager.CurrentTurnState != TurnState.Placement)
						return;
		if (m_playerSupporterList.Count < 1)
						return;

		m_playerSupportersAddedThisTurn--; // TODO: this doesn't seem to be used anywhere anyway


		// Remove the last supporter
		GameObject supporter = m_playerSupporterList [m_playerSupporterList.Count - 1];
		m_playerSupporterList.Remove(supporter);
		m_playerBasisCountIncrement -= supporter.GetComponent< CampaignWorker >().GetValueForLevel(); // this is dumb
		m_playerCampaignWorkerCounts[ 0 ]--;
		Destroy (supporter);
		
		GameObjectAccessor.Instance.Budget.ConsumeAmount( GameMove.GetCost(GameActions.REMOVE_SUPPORTER) ); // this will probably be a negative number (add money)
		
		m_dirty = true;
	}
	
	
	[RPC]
	public void OpponentCreateSupporters( Vector3 nextOpponentsList )
  {
    m_receivedOpponentInfo = true;

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
      Debug.Log ( "Received opponent info: " + m_nextOpponentSupporterList.Count );
      /*for( int i = 0; i < m_nextOpponentSupporterList.Count; i++ )
      {
        Debug.Log ( m_nextOpponentSupporterList[i] );
      }*/
    //m_opponentCampaignWorkerCounts = counts;
    //CreateSupporterPrefab( false );
  }

	[RPC]
	public void OpponentCreateSupporters( int nextCount )
	{
		m_receivedOpponentInfo = true;

		NextOpponentSupporterCount = nextCount;
		
		Debug.Log ( "Received opponent info: " + NextOpponentSupporterCount );
	}
	
	public void CreateSupporterPrefab( bool isPlayer )
  {
    Vector3 supporterPosition = Center + m_workerOffsetX + ( isPlayer ? m_workerOffsetY + ( m_playerSupporterList.Count * m_workerAdjacencyOffset ) : -m_workerOffsetY + ( ( m_opponentSupporterList.Count ) * m_workerAdjacencyOffset ) );

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

  public void OpponentUpgrade(int upgradeLevel) {
    m_opponentCampaignWorkerCounts[ upgradeLevel - 1 ]--;
    m_opponentCampaignWorkerCounts[ upgradeLevel ]++;

    for( int i = 0; i < m_opponentSupporterList.Count; i++ )
    {
      CampaignWorker worker = m_opponentSupporterList[ i ].GetComponent< CampaignWorker >();
      if( worker.m_currentLevel == upgradeLevel )
      {
        m_opponentBasisCountIncrement -= worker.GetValueForLevel();
        worker.Upgrade();
        m_opponentBasisCountIncrement += worker.GetValueForLevel();
        
        m_dirty = true;
        break;
      }
    }
  }

  public void Upgrade1( bool bounce = true )
  {
    int cost = GameMove.GetCost( GameActions.UPGRADE1 );
    if( GameObjectAccessor.Instance.Budget.IsAmountAvailable( cost ) && m_playerCampaignWorkerCounts[ 0 ] > 0 )
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
          GameObjectAccessor.Instance.Budget.ConsumeAmount( cost );
          GameObjectAccessor.Instance.DetailView.SetState( GameObjectAccessor.Instance.DetailView.CurrentState, false );

          m_dirty = true;
          break;
        }
      }
    }
  }
  public void Upgrade2( bool bounce = true )
  {
    int cost = GameMove.GetCost( GameActions.UPGRADE2 );
    if( GameObjectAccessor.Instance.Budget.IsAmountAvailable( cost ) && m_playerCampaignWorkerCounts[ 1 ] > 0 )
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
          GameObjectAccessor.Instance.Budget.ConsumeAmount( cost );
          GameObjectAccessor.Instance.DetailView.SetState( GameObjectAccessor.Instance.DetailView.CurrentState, false );

          m_dirty = true;
          break;
        }
      }
    }
  }
}