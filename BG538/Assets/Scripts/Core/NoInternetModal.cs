using UnityEngine;
using System;
using MiniJSON;
using System.Collections.Generic;


public class NoInternetModal : MonoBehaviour
{
  public GameObject NoInternetConnectionMessage;
  public GameObject CantReachServerMessage;

  public ServerPoll m_serverPollScript;

  public Action m_reauthenticationCallback = null;


  public void OnEnable() {
    CantReachServerMessage.SetActive( false );
    NoInternetConnectionMessage.SetActive( false );
  }

  public void Display( bool noInternet ) {
    NoInternetConnectionMessage.SetActive( true );
    return;

    if( noInternet ) {
      NoInternetConnectionMessage.SetActive( true );
    }
    else {
      CantReachServerMessage.SetActive( true );
    }
  }

  public void QuitButtonPress() {
    Application.Quit();
  }

  public void RetryButtonPress() {
    gameObject.SetActive( false );
    m_serverPollScript.AttemptConnection( m_reauthenticationCallback );
  }
}