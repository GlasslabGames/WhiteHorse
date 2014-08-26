using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System;
using System.IO;


public class PegasusManager
{
  //public const string SDK_SERVER_URI  =   "http://192.168.6.171:8001";    // Ben GlassLab network
  //public const string SDK_SERVER_URI  =   "http://192.168.1.119:8001";  // Ben home network

  //public const string SDK_SERVER_URI  =   "http://192.168.10.94:8001";  // Jerry local server

  #if GAME_RELEASE
  public const string SDK_SERVER_URI  =   "http://playfully.org";        // Mars production server
  #else
  public const string SDK_SERVER_URI  =   "http://stage.playfully.org";  // Mars staging server
  #endif

  public const string SDK_CLIENT_ID   =   "AA-1";
  public const string SDK_GAME_NAME   =   "ArgubotAcademy";
  public const string SDK_GAME_LEVEL  =   "Pre_alpha_episode_1";


  // Local instance variable for the Pegasus singleton
  private static PegasusManager _instance = null;
  private static readonly object padlock = new object();

  // GlassLab SDK
  private GlasslabSDK glsdk;
  public GlasslabSDK GLSDK {
    get {
      return glsdk;
    }
  }

  // Singleton instance getter
  public static PegasusManager Instance {
    get {
      if( PegasusManager._instance == null ) {
        lock( padlock ) {
          // Create the instance if it doesn't exist
          if( PegasusManager._instance == null ) {
            PegasusManager._instance = new PegasusManager();
          }
          //return PegasusManager._instance;
        }
      }
      return PegasusManager._instance;
    }
  }

  // Singleton constructor
  private PegasusManager() {
    // Set the GlassLab SDK
    glsdk = GlasslabSDK.Instance;
  }

  /**
  * API functions:
  *  - Connect
  *  - SetPlayerHandle
  *  - Login
  *  - Logout
  *  - StartSession
  *  - EndSession
  */
  public void Connect() {
#if !UNITY_EDITOR && CLASSROOM
    Debug.Log( "[PegasusManager] Attempting to connect..." );
    glsdk.Connect( Application.persistentDataPath, SDK_CLIENT_ID, SDK_SERVER_URI );
    SetClientProperties();
#endif

    // Listen for a new game
    SignalManager.NewGameStarted += StartSession;
  }
  public void SetClientProperties() {
#if !UNITY_EDITOR && CLASSROOM
    Debug.Log( "[PegasusManager] Attempting to set client properties..." );
    glsdk.SetName( SDK_GAME_NAME );
    glsdk.SetVersion( GLResourceManager.InstanceOrCreate.GetVersionString() );
    glsdk.SetGameLevel( SDK_GAME_LEVEL );
#endif
  }
  public void SetPlayerHandle( string handle ) {
#if !UNITY_EDITOR && CLASSROOM
    glsdk.SetPlayerHandle( handle );
#endif
  }
  public void Login( string username, string password ) {
#if !UNITY_EDITOR && CLASSROOM
    Debug.Log( "[PegasusManager] Attempting login..." );
    glsdk.Login( username, password, LoginDone );
    //glsdk.Login( "andrew", "glasslab", LoginDone );
    //glsdk.Login( "jstudent", "jstudent", LoginDone );
#endif
  }
  public void Logout() {
#if !UNITY_EDITOR && CLASSROOM
    Debug.Log( "[PegasusManager] Attempting logout..." );
    glsdk.Logout( LogoutDone );
#endif
  }
  public void StartSession() {
#if !UNITY_EDITOR && CLASSROOM
    Debug.Log( "[PegasusManager] Attempting to start the session..." );
    glsdk.StartSession( StartSessionDone );
#endif
  }
  public void EndSession() {
#if !UNITY_EDITOR && CLASSROOM
    Debug.Log( "[PegasusManager] Attempting to end the session..." );
    glsdk.EndSession( EndSessionDone );
#endif
  }

  /**
   * Helper function for appending default telemetry event info, including activity Id and current quest.
   */
  public void AppendDefaultTelemetryInfo() {
    // Add the activity Id
    glsdk.AddTelemEventValue( "activityId", ActivityStatsManager.InstanceOrCreate.GetCurrentActivity() );

    // Add the quest - indicate "interstitial" if we are between quests
    Quest quest = null;
    if (QuestManager.Instance != null) quest = QuestManager.Instance.GetCurrentActiveQuest(); // in some cases (i.e. testing the battle scene only) the QuestManager isn't present
    glsdk.AddTelemEventValue( "quest", quest == null ? "interstitial" : quest.Title );

    // Set the activity attempt
    glsdk.AddTelemEventValue( "attempt", ActivityStatsManager.InstanceOrCreate.GetAttemptForCurrentActivity() );
  }

  /**
  * GlassLab SDK callback functions
  */
  private void LoginDone( string response ) {
    Debug.Log( "Login Done!" );
  }
  private void LogoutDone( string response ) {
    Debug.Log( "Logout Done!" );
  }
  private void StartSessionDone( string response ) {
    Debug.Log( "Start Session Done!" );
  }
  private void EndSessionDone( string response ) {
    Debug.Log( "End Session Done!" );
  }
}