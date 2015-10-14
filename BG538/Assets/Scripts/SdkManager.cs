#define DEBUG_SDK

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


public class SdkManager
{
  #if GAME_RELEASE
  // TODO
  #else
  public const string SDK_SERVER_URI  =   "http://stage.playfully.org";  // staging server
  #endif

  public const string SDK_CLIENT_ID   =   "B538";
  public const string SDK_GAME_NAME   =   "Battleground538";
  public const string SDK_GAME_LEVEL  =   "unassigned";

	// Based on Lizzo's work
	public enum EventCategory {
		None,
		Unit_Start,
		Unit_End,
		Player_Action,
		System_Event
	}

  // Local instance variable for the Pegasus singleton
  private static SdkManager _instance = null;
  private static readonly object padlock = new object();

  // GlassLab SDK
  private GlasslabSDK glsdk;
  public GlasslabSDK GLSDK {
    get {
      return glsdk;
    }
  }

  // Singleton instance getter
  public static SdkManager Instance {
    get {
      if( SdkManager._instance == null ) {
        lock( padlock ) {
          // Create the instance if it doesn't exist
          if( SdkManager._instance == null ) {
            SdkManager._instance = new SdkManager();
          }
          //return SdkManager._instance;
        }
      }
      return SdkManager._instance;
    }
  }

  // Singleton constructor
  private SdkManager() {
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
#if !UNITY_EDITOR
    Debug.Log( "[SdkManager] Attempting to connect..." );
    glsdk.Connect( Application.persistentDataPath, SDK_CLIENT_ID, SDK_SERVER_URI );
    SetClientProperties();
#endif

    // Listen for a new game
    //SignalManager.NewGameStarted += StartSession;
  }
  public void SetClientProperties() {
#if !UNITY_EDITOR
    Debug.Log( "[SdkManager] Attempting to set client properties..." );
    glsdk.SetName( SDK_GAME_NAME );
    glsdk.SetVersion( GLResourceManager.InstanceOrCreate.GetVersionString() );
    glsdk.SetGameLevel( SDK_GAME_LEVEL );
#endif
  }
  public void SetPlayerHandle( string handle ) {
#if !UNITY_EDITOR
    glsdk.SetPlayerHandle( handle );
#endif
  }
  public void Login( string username, string password ) {
#if !UNITY_EDITOR
    Debug.Log( "[SdkManager] Attempting login..." );
    glsdk.Login( username, password, LoginDone );
    //glsdk.Login( "andrew", "glasslab", LoginDone );
    //glsdk.Login( "jstudent", "jstudent", LoginDone );
#endif
  }
  public void Logout() {
#if !UNITY_EDITOR
    Debug.Log( "[SdkManager] Attempting logout..." );
    glsdk.Logout( LogoutDone );
#endif
  }
  public void StartSession() {
#if !UNITY_EDITOR
    Debug.Log( "[SdkManager] Attempting to start the session..." );
    glsdk.StartSession( StartSessionDone );
#endif
  }
  public void EndSession() {
#if !UNITY_EDITOR
    Debug.Log( "[SdkManager] Attempting to end the session..." );
    glsdk.EndSession( EndSessionDone );
#endif
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

	/**
	 * Shortcuts for telemetry events with debug logs
	 * Note that not all types have shortcuts yet, just the most commonly used ones (for now)
	 */
	public void AddTelemEventValue(string key, string value) {
		#if DEBUG_SDK
		Debug.Log("_ "+key+": "+value);
		#endif
		#if (UNITY_IOS && !UNITY_EDITOR) || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		GLSDK.AddTelemEventValue(key, value);
		#endif
	}
	public void AddTelemEventValue(string key, int value) {
		#if DEBUG_SDK
		Debug.Log("__ "+key+": "+value);
		#endif
		#if (UNITY_IOS && !UNITY_EDITOR) || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		GLSDK.AddTelemEventValue(key, value);
		#endif
	}
	public void AddTelemEventValue(string key, float value) {
		#if DEBUG_SDK
		Debug.Log("__ "+key+": "+value);
		#endif
		#if (UNITY_IOS && !UNITY_EDITOR) || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		GLSDK.AddTelemEventValue(key, value);
		#endif
	}
	public void AddTelemEventValue(string key, bool value) {
		#if DEBUG_SDK
		Debug.Log("__ "+key+": "+value);
		#endif
		#if (UNITY_IOS && !UNITY_EDITOR) || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		GLSDK.AddTelemEventValue(key, value);
		#endif
	}
	// Shortcuts for enums:
	public void AddTelemEventValue(string key, State.Controller value) {
		AddTelemEventValue(key, Enum.GetName(typeof(State.Controller), value));
	}

	public void SaveTelemEvent(string name, bool result, EventCategory category = EventCategory.None) {
		AddTelemEventValue("result", result);
		SaveTelemEvent(name, category);
	}

	public void SaveTelemEvent(string name, EventCategory category = EventCategory.None) {
		AddTelemEventValue("category", System.Enum.GetName(typeof(EventCategory), category));

		#if DEBUG_SDK
		Debug.Log("> TELEM EVENT: "+name);
		#endif
		#if (UNITY_IOS && !UNITY_EDITOR) || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		GLSDK.SaveTelemEvent(name);
		#endif
	}
}