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
    Debug.Log ( "Prefs: " + PlayerPrefs.GetString( "NetworkType" ) );

    if( PlayerPrefs.GetString( "NetworkType" ) == "Client" )
    {
      GameObjectAccessor.Instance.Player.m_leaning = Leaning.Blue;
      GameObjectAccessor.Instance.Player.m_opponentLeaning = Leaning.Red;
      GameObjectAccessor.Instance.UseAI = false;
    }
    else if( PlayerPrefs.GetString( "NetworkType" ) == "Server" )
    {
      GameObjectAccessor.Instance.Player.m_leaning = Leaning.Red;
      GameObjectAccessor.Instance.Player.m_opponentLeaning = Leaning.Blue;
      GameObjectAccessor.Instance.UseAI = false;
    }
    else if( PlayerPrefs.GetString( "NetworkType" ) == "Offline" )
    {
      GameObjectAccessor.Instance.Player.m_leaning = Leaning.Red;
      GameObjectAccessor.Instance.Player.m_opponentLeaning = Leaning.Blue;
      GameObjectAccessor.Instance.UseAI = true;
    }

    PlayerPrefs.SetString( "NetworkType", "NONE" );


    /*Debug.Log ( "Offline" );
    Debug.Log ( Network.peerType );
    if( Network.peerType == NetworkPeerType.Client )
    {
      Debug.Log ( "Client" );

      if( GameObjectAccessor.Instance != null )
      {
        GameObjectAccessor.Instance.Player.m_leaning = Leaning.Blue;
        GameObjectAccessor.Instance.Player.m_opponentLeaning = Leaning.Red;
        GameObjectAccessor.Instance.UseAI = false;
      }
    }
    else if( Network.peerType == NetworkPeerType.Server )
    {
      Debug.Log ( "Server" );

      if( GameObjectAccessor.Instance != null )
      {
        GameObjectAccessor.Instance.Player.m_leaning = Leaning.Red;
        GameObjectAccessor.Instance.Player.m_opponentLeaning = Leaning.Blue;
        GameObjectAccessor.Instance.UseAI = false;
      }
    }
    else
    {
      Debug.Log ( "Offline" );

      if( GameObjectAccessor.Instance != null )
      {
        GameObjectAccessor.Instance.Player.m_leaning = Leaning.Red;
        GameObjectAccessor.Instance.Player.m_opponentLeaning = Leaning.Blue;
        GameObjectAccessor.Instance.UseAI = true;
      }
    }*/
    //Debug.Log ( Network.peerType );

    /*if( m_actAsServer )
    {
      GameObjectAccessor.Instance.Player.m_leaning = Leaning.Red;
      GameObjectAccessor.Instance.Player.m_opponentLeaning = Leaning.Blue;

      if( m_enableNetworking )
      {
        LaunchServer();
      }
    }
    else
    {
      GameObjectAccessor.Instance.Player.m_leaning = Leaning.Blue;
      GameObjectAccessor.Instance.Player.m_opponentLeaning = Leaning.Red;

      if( m_enableNetworking )
      {
        ConnectToServer();
      }
    }*/
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
    bool useNatPunchthrough = !Network.HavePublicAddress();
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

    Application.LoadLevel( "ben_scene" );
  }
}