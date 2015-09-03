using UnityEngine;
using System.Collections;


public class GameNetworkManager : MonoBehaviour
{
  private bool m_enableNetworking;

  private bool m_actAsServer;
  private string m_url;
  public int m_port;
  public int m_allowedConnections;
  public UILabel m_hostLabel;


  public void Awake()
  {
    Debug.Log ( "Prefs: " + PlayerPrefs.GetString( "NetworkType" ) );

    if( PlayerPrefs.GetString( "NetworkType" ) == "Client" )
    {
      GameObjectAccessor.Instance.Player.leaning = Leaning.Blue;
      GameObjectAccessor.Instance.GameStateManager.UseAi = false;
    }
    else if( PlayerPrefs.GetString( "NetworkType" ) == "Server" )
    {
      GameObjectAccessor.Instance.Player.leaning = Leaning.Red;
	  GameObjectAccessor.Instance.GameStateManager.UseAi = false;
    }
    else // if( PlayerPrefs.GetString( "NetworkType" ) == "Offline" ) // Default to using AI if player prefs aren't set
    {
      GameObjectAccessor.Instance.Player.leaning = Leaning.Red;
	  GameObjectAccessor.Instance.GameStateManager.UseAi = true;
    }

    PlayerPrefs.SetString( "NetworkType", "NONE" );
  }
}