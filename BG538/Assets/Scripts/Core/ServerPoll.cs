using UnityEngine;
using System;
using MiniJSON;
using System.Collections.Generic;

// Edited from the original to use a separate timer (for clarity)
public class ServerPoll : MonoBehaviour
{
	public NoInternetModal NoInternetConnectionModal;
	private Timer timer;
	private static ServerPoll _instance;
	
  public void Awake() {
		if (_instance != null) {
			Destroy(gameObject); // we already have one, we don't need too
		} else {
			_instance = this;
			DontDestroyOnLoad(gameObject); // save this one

			#if !UNITY_EDITOR
			timer = GetComponent<Timer>();
			timer.StartTimer(onTimer);
			#endif
		}
  }

	public void onTimer() {
		Debug.Log("*** Server poll timer! ***");
		if (NoInternetConnectionModal.gameObject.activeSelf) { 	// no need to recheck while the popup is up
			return;
		} else if (Application.loadedLevelName == "title") { // no need to recheck on the title screen
			return;
		} else {
			AttemptConnection();
		}
	}

  /**
   * Attempt a connection to the server.
   */
  public void AttemptConnection( ) {

    // Connect to the server
    Debug.Log( "SERVER POLL: about to call SDK Connect" );
    SdkManager.Instance.GLSDK.Connect( Application.persistentDataPath, SdkManager.SDK_CLIENT_ID, SdkManager.SDK_SERVER_URI, ConnectCallback );
  }
  
  /**
   * Want to display messaging for failed connection.
   */
  public void ConnectCallback( string response ) {
    Debug.Log( "SERVER POLL: ConnectCallback(): " + response );

    // Likely server is down
    if( response == "" ) {
      // We are offline
      Debug.Log( "We are offline!" );
      DisplayNoInternetModal( false );
    }
    else {
      // Deserialize the response and get the status field
      Dictionary<string, object> responseAsJSON = Json.Deserialize( response ) as Dictionary<string, object>;
      if( responseAsJSON.ContainsKey( "error" ) ) {
        // We are offline
        Debug.Log( "We are offline!" );
        DisplayNoInternetModal( true );
      }
      // else we are still online
    }
    
    Debug.Log( "SERVER POLL: done with ConnectCallback" );
  }

  /**
   * Function is used to display the no internet notification modal.
   * True = no internet
   * False = can't reach server
   */
  public void DisplayNoInternetModal( bool noInternet ) {
		NoInternetConnectionModal.Display(noInternet);
  }
}