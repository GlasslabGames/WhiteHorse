using System.Collections.Generic;
using UnityEngine;


public class ActivityStatsManager : SingletonBehavior<ActivityStatsManager>
{
  [PersistAttribute]
  private string m_currentActivity; // note: don't think there's a reason to persist this since we always start in Exploration

  private Stack<string> m_prevActivities = new Stack<string>(); // keep a stack of activities so we can go back to the previous one

  [PersistAttribute]
  private Dictionary<string, int> m_activityAttempts;

  private static string DEFAULT_ACTIVITY = "Exploration";

  /**
   * Initialize the dictionary on initialize.
   */
  protected override void Awake()
  {
    base.Awake();

    if( m_currentActivity == null || m_currentActivity == "" ) {
      m_currentActivity = DEFAULT_ACTIVITY;
    }

    if( m_activityAttempts == null ) {
      m_activityAttempts = new Dictionary<string, int>();
      ResetActivityAttempts();
    }
  }

  /**
   * Helper function to get the current activity.
   */
  public string GetCurrentActivity() {
    if( m_currentActivity == null || m_currentActivity == "" ) {
      return "NONE";
    }
    return m_currentActivity;
  }

  /**
   * Helper function for setting the current activity.
   */
  public void SetCurrentActivity( string activity ) {
    // Only increment the attempt counter for this activity if it is new
    if( m_currentActivity != activity ) {
      Debug.Log ("*** Set activity: "+activity);

      if (m_currentActivity != null) {
        m_prevActivities.Push(m_currentActivity); // remember which activity we came from
      }

      m_currentActivity = activity;
      
      // Update the activity attempts counter
      UpdateAttemptForActivity( m_currentActivity );
    }
  }

  public void FinishActivity( ) {
    Debug.Log ("*** Finish activity: "+m_currentActivity);

    m_currentActivity = null;
    // If we had a previous activity, go back to that
    SetCurrentActivity ((m_prevActivities.Count > 0)? m_prevActivities.Pop() : DEFAULT_ACTIVITY);
  }

  /**
   * Helper function to reset attempts dictionary.
   */
  public void ResetActivityAttempts() {
    m_activityAttempts[ "Exploration" ] = 1;
    m_activityAttempts[ "BotSelect" ] = 0;
    m_activityAttempts[ "CoreEquip" ] = 0;
    m_activityAttempts[ "CoreConstruction" ] = 0;
    m_activityAttempts[ "Battle" ] = 0;
  }

  /**
   * Helper function to update the attempt value for an activity.
   */
  public void UpdateAttemptForActivity( string activity ) {
    if( m_activityAttempts.ContainsKey( activity ) ) {
      m_activityAttempts[ activity ]++;
    }
  }

  /**
   * Helper function to get the attempt counter for the desired activity. If
   * there is no entry, -1 will be returned.
   */
  public int GetAttemptForCurrentActivity() {
    //Debug.Log( m_currentActivity , this);
    if( m_activityAttempts != null && m_activityAttempts.ContainsKey( GetCurrentActivity() ) ) {
      return m_activityAttempts[ m_currentActivity ];
    }
    return -1;
  }
}