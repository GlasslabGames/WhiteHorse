using UnityEngine;
using System.Collections;


public class NetworkManager : MonoBehaviour
{
  public bool m_actAsServer;
  public string m_url;
  public int m_port;
  public int m_allowedConnections;


  public void Awake()
  {
    if( m_actAsServer )
    {
      GameObjectAccessor.Instance.Player.m_leaning = Leaning.Red;
      LaunchServer();
    }
    else
    {
      GameObjectAccessor.Instance.Player.m_leaning = Leaning.Blue;
      ConnectToServer();
    }
  }

	public void LaunchServer()
  {
    //Network.incomingPassword = "glasslab2014";
    bool useNatPunchthrough = !Network.HavePublicAddress();
    NetworkConnectionError connectionResult = Network.InitializeServer( m_allowedConnections, m_port, useNatPunchthrough );
    Debug.Log( "\n\nLaunchServer() Result: " + connectionResult + "\n\n" );
  }

  public void ConnectToServer()
  {
    //10.71.8.230
    NetworkConnectionError connectionResult = Network.Connect( m_url, m_port );
    Debug.Log( "\n\nConnectToServer() Result: " + connectionResult + "\n\n" );
  }
}