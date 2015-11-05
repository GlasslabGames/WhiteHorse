using UnityEngine;
using System;
using MiniJSON;
using System.Collections.Generic;


public class NoInternetModal : MonoBehaviour
{
  public GameObject NoInternetConnectionMessage;
  public GameObject CantReachServerMessage;

  public ServerPoll m_serverPollScript;

  public void Display( bool noInternet ) {
	gameObject.SetActive(true);
		
    NoInternetConnectionMessage.SetActive( noInternet );
    CantReachServerMessage.SetActive( !noInternet );
  }

  public void QuitButtonPress() {
	gameObject.SetActive( false );
	SdkManager.Instance.Logout();
	if (Application.loadedLevelName != "title") Application.LoadLevel("title");
  }

  public void RetryButtonPress() {
    gameObject.SetActive( false );
    m_serverPollScript.AttemptConnection();
  }
}