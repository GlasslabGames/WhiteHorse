using UnityEngine;
using System.Collections;


public class NetworkManager : MonoBehaviour
{
  private bool m_enableNetworking;

  private bool m_actAsServer;
  private string m_url;
  public int m_port;
  public int m_allowedConnections;
  public UILabel m_hostLabel;

  private bool m_shouldLoad = false;


  public void Awake()
  {
    PlayerPrefs.SetString( "NetworkType", "NONE" );
  }

  public void Update()
  {
    if( Network.peerType == NetworkPeerType.Disconnected )
    {
      //Debug.Log ( "Disconnected" );
    }
    if( Network.peerType == NetworkPeerType.Connecting )
    {
      //Debug.Log ( "Connecting" );
    }
    else if( Network.peerType == NetworkPeerType.Client )
    {
      if( m_shouldLoad )
      {
        m_shouldLoad = false;
        Debug.Log ( "Client" );
        PlayerPrefs.SetString( "NetworkType", "Client" );
        HideStartScreen();
      }
    }
    else if( Network.peerType == NetworkPeerType.Server )
    {
      if( m_shouldLoad )
      {
        m_shouldLoad = false;
        Debug.Log ( "Server" );
        PlayerPrefs.SetString( "NetworkType", "Server" );
        HideStartScreen();
      }
    }
  }

  public void PlayOffline()
  {
    //GameObjectAccessor.Instance.Player.m_leaning = Leaning.Red;
    //GameObjectAccessor.Instance.Player.m_opponentLeaning = Leaning.Blue;
    m_enableNetworking = false;
    PlayerPrefs.SetString( "NetworkType", "Offline" );
    //GameObjectAccessor.Instance.UseAI = true;

    HideStartScreen();
  }
    
  public void LaunchServer()
  {
    m_enableNetworking = true;
    m_shouldLoad = true;
    //GameObjectAccessor.Instance.UseAI = false;

    //GameObjectAccessor.Instance.Player.m_leaning = Leaning.Red;
    //GameObjectAccessor.Instance.Player.m_opponentLeaning = Leaning.Blue;

    PlayerPrefs.SetString( "NetworkType", "Server" );
        
    //Network.incomingPassword = "glasslab2014";
    Debug.Log( "Launching server at" );
    bool useNatPunchthrough = false;// !Network.HavePublicAddress();
    NetworkConnectionError connectionResult = Network.InitializeServer( m_allowedConnections, m_port, useNatPunchthrough );
    Debug.Log( "\n\nLaunchServer() Result: " + connectionResult + "\n\n" );

    //HideStartScreen();
  }

  public void ConnectToServer()
  {
    m_enableNetworking = true;
    m_shouldLoad = true;
    //GameObjectAccessor.Instance.UseAI = false;

    PlayerPrefs.SetString( "NetworkType", "Client" );

    //GameObjectAccessor.Instance.Player.m_leaning = Leaning.Blue;
    //GameObjectAccessor.Instance.Player.m_opponentLeaning = Leaning.Red;
        
    //10.71.8.230
    Debug.Log ( "Connect to: " + m_hostLabel.text );
    m_url = m_hostLabel.text;
    NetworkConnectionError connectionResult = Network.Connect( m_url, m_port );
    Debug.Log( "\n\nConnectToServer() Result: " + connectionResult + "\n\n" );

    //HideStartScreen();
  }

  public void HideStartScreen()
  {
    //GameObjectAccessor.Instance.TitleScreen.SetActive( false );
    //GameObjectAccessor.Instance.GameStateManager.GoToState( TurnState.Placement );

    Application.LoadLevel( "game" );
  }
}