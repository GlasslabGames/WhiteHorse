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
  }
}