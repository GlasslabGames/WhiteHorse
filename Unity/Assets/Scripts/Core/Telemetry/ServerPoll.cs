using UnityEngine;
using System;
using MiniJSON;
using System.Collections.Generic;


public class ServerPoll : MonoBehaviour
{
  public int SERVER_POLL_IN_SECONDS = 60;
  private float m_pollTimer;
  private bool m_active;

  public NoInternetModal NoInternetConnectionModal;

  private Action m_successCallback;


  /**
   * Activate the timer on awake.
   */
  public void Awake() {
#if CLASSROOM
    Reset( true );
#endif
  }

  /**
   * Function resets the timer and re-activates if required.
   */
  public void Reset( bool activate ) {
    m_pollTimer = 0.0f;
    m_active = activate;
  }

  /**
   * Function updates the poll timer if it is active. If the timer reaches its duration, ping the server.
   */
  public void Update() {
    if( m_active ) {
      m_pollTimer += Time.deltaTime;

      // Check the timer against the duration and attempt a connection if need be, then reset the timer
      if( m_pollTimer >= SERVER_POLL_IN_SECONDS ) {
        AttemptConnection();
      }
    }
  }

  /**
   * Attempt a connection to the server.
   */
  public void AttemptConnection( Action successCallback = null ) {
    // Set the callback to fire if it isn't null
    if( successCallback != null ) {
      m_successCallback = successCallback;
    }

    // Connect to the server
    Debug.Log( "about to call SDK Connect as poll" );
    Reset( false );
    PegasusManager.Instance.GLSDK.Connect( Application.persistentDataPath, PegasusManager.SDK_CLIENT_ID, PegasusManager.SDK_SERVER_URI, ConnectCallback );
  }
  
  /**
   * Want to display messaging for failed connection.
   */
  public void ConnectCallback( string response ) {
    Debug.Log( "SERVER: in POLL ConnectCallback(): " + response );

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
      else {
        // We are still online, poll again on the next interval
        Reset( true );

        // Fire the callback to fire if it isn't null
        if( m_successCallback != null ) {
          m_successCallback();
          m_successCallback = null;
        }
      }
    }
    
    Debug.Log( "SERVER: done with POLL ConnectCallback" );
  }

  /**
   * Function is used to display the no internet notification modal.
   * True = no internet
   * False = can't reach server
   */
  public void DisplayNoInternetModal( bool noInternet ) {
    NoInternetConnectionModal.gameObject.SetActive( true );
    NoInternetConnectionModal.Display( noInternet );
  }
}