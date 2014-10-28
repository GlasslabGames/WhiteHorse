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
  private List< State > m_states = new List< State >();
  private List< State > m_statesInPlay = new List< State >();
  private List< State > m_statesNotInPlay = new List< State >();

  private TurnState m_currentTurnState;

  private bool m_playerTurnCompleted;
  private bool m_opponentTurnCompleted;

  private int m_weeksLeft;
  public int m_totalElectionWeeks;

  public Timer m_harvestTimer;

  public int m_currentAnte;

  private int m_playerVotes;
  private int m_opponentVotes;

  public int m_defaultScenarioId;

  public TurnState CurrentTurnState
  {
    get { return m_currentTurnState; }
  }


  public void Awake()
  {
    ScenarioModel m_scenario = ScenarioModel.GetModel(m_defaultScenarioId);

    foreach (State state in GameObjectAccessor.Instance.StatesContainer.GetComponentsInChildren<State>()) {
      if (m_scenario != null) {
        state.InPlay = !m_scenario.PresetStates.Contains(state.Model.Id);
        float value = 0;
        if (m_scenario.StateLeanings.Count >= state.Model.Id) {
          int leaningId = m_scenario.StateLeanings[state.Model.Id - 1]; // state IDs go from 1 to 51, so subtract 1 to access 0 - 50 in array
          InitialLeaningModel initialLeaning = InitialLeaningModel.GetModel( leaningId );
          value = initialLeaning.Value;
        }
        float r = UnityEngine.Random.Range( -m_scenario.Randomness, m_scenario.Randomness );
        Debug.Log ("Initial leaning value: "+value+" Random: "+r);
        state.SetInitialPopularVote(value + r);
      } else {
        state.InPlay = true;
      }
      m_states.Add( state );
      if ( state.InPlay ) m_statesInPlay.Add( state );
      else m_statesNotInPlay.Add( state );
    }

    Debug.Log( "States in play: " + m_statesInPlay.Count );
    Debug.Log( "States not in play: " + m_statesNotInPlay.Count );

    m_weeksLeft = m_totalElectionWeeks;

  }

  public void Start()
  {
    GoToState( TurnState.Placement );

    UpdateElectoralVotes(true);
  }


  public void NextHarvestAction()
  {
    foreach( State state in m_statesInPlay )
    {
      bool hasCompleted = true;

      //if( state.m_dirty )
      {
        hasCompleted = state.UpdateState();
      }

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
		if (GameObjectAccessor.Instance.WeekCounter != null) GameObjectAccessor.Instance.WeekCounter.text = ((week+1 > m_totalElectionWeeks)? m_totalElectionWeeks : week+1).ToString();
    if( m_weeksLeft < 5 )  m_currentAnte = 50;
    else if( m_weeksLeft < 9 ) m_currentAnte = 30;

    GameObjectAccessor.Instance.Budget.GainAmount( m_currentAnte );
    // also give the AI money
    GameObjectAccessor.Instance.OpponentAi.Budget.GainAmount( m_currentAnte );

    if( m_weeksLeft == 0 )
    {
      GoToState( TurnState.ElectionDay );
      GameObjectAccessor.Instance.GameOverScreen.SetActive( true );

			// Rather than renaming the GameOverRedVotes, etc, just know that RedVotes is on the left (the player) and BlueVotes is on the right (the opponent)
      GameObjectAccessor.Instance.GameOverRedVotes.text = GameObjectAccessor.Instance.PlayerVotesLabel.text;
			GameObjectAccessor.Instance.GameOverRedVotes.color = AutoSetColor.GetColor(true, AutoSetColor.ColorChoice.LIGHT);

			GameObjectAccessor.Instance.GameOverBlueVotes.text = GameObjectAccessor.Instance.OpponentVotesLabel.text;
			GameObjectAccessor.Instance.GameOverBlueVotes.color = AutoSetColor.GetColor(false, AutoSetColor.ColorChoice.LIGHT);

      if( m_playerVotes > m_opponentVotes )
      {
        // victory sound
        GameObject.Instantiate( GameObjectAccessor.Instance.VictorySound );
      }
      else
      {
        // defeat sound
        GameObject.Instantiate( GameObjectAccessor.Instance.DefeatSound );
      }
		}
		else
    {
      m_playerTurnCompleted = false;
      m_opponentTurnCompleted = false;
      GameObjectAccessor.Instance.Player.ToggleCampaignWorker( true );
      GoToState( TurnState.Placement );

      foreach( State state in m_statesInPlay )
      {
        state.m_receivedOpponentInfo = false;
      }

			// indicate that we can end the turn
			GameObjectAccessor.Instance.EndTurnButton.mainTexture = GameObjectAccessor.Instance.Textures.EndTurnButton;
    }
  }


  public void GoToState( TurnState nextState )
  {
    m_currentTurnState = nextState;
    if (nextState == TurnState.Placement) {
      if (GameObjectAccessor.Instance.UseAI) GameObjectAccessor.Instance.OpponentAi.DoTurn();
    }
  }

  public void CheckForHarvest()
  {
    if( m_playerTurnCompleted && ( GameObjectAccessor.Instance.UseAI || m_opponentTurnCompleted ) )
    {
      // indicate that we're showing the harvest
      GameObjectAccessor.Instance.EndTurnButton.mainTexture = GameObjectAccessor.Instance.Textures.ResultsButton;

      foreach( State state in m_statesInPlay )
      {
        state.PrepareToUpdate();
      }

      GoToState( TurnState.Harvest );
      m_harvestTimer.StartTimer( NextHarvestAction );
    }
  }


  public void UpdateElectoralVotes(bool atBeginning = false)
  {
    int totalRedVotes = 0;
    int totalBlueVotes = 0;

		float totalOpinion = 0;
		float totalPopulation = 0;

    foreach( State state in m_states )
    {
      if( state.IsRed )
      {
        totalRedVotes += state.Model.ElectoralCount;
      }
      else if( state.IsBlue )
      {
        totalBlueVotes += state.Model.ElectoralCount;
      }
			totalOpinion += state.PopularVote * state.Model.Population;
      totalPopulation += state.Model.Population;

      //state.m_dirty = false;
    }
	
		m_playerVotes = (GameObjectAccessor.Instance.Player.IsBlue) ? totalBlueVotes : totalRedVotes;
		m_opponentVotes = (GameObjectAccessor.Instance.Player.IsBlue) ? totalRedVotes : totalBlueVotes;

    GameObjectAccessor.Instance.PlayerVoteCount.Set(m_playerVotes, !atBeginning);
    GameObjectAccessor.Instance.OpponentVoteCount.Set (m_opponentVotes, !atBeginning);

		if (GameObjectAccessor.Instance.ElectoralVoteMeter != null)
      GameObjectAccessor.Instance.ElectoralVoteMeter.Refresh(m_playerVotes, m_opponentVotes);

		Debug.Log("Average opinion: "+totalOpinion / totalPopulation);

		if (GameObjectAccessor.Instance.PopularVoteMeter != null)
			GameObjectAccessor.Instance.PopularVoteMeter.Refresh( totalOpinion / totalPopulation );

  }


  public void CompletePlayerTurn()
  {
    if( m_currentTurnState == TurnState.Placement )
    {
      Debug.Log( "player turn completed!" );

      m_playerTurnCompleted = true;

			// indicate that we're waiting for the opponent
			GameObjectAccessor.Instance.EndTurnButton.mainTexture = GameObjectAccessor.Instance.Textures.SubmittedButton;

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