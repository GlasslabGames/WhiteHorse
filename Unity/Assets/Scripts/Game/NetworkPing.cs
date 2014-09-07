using UnityEngine;
using System.Collections;


public class NetworkPing : MonoBehaviour
{
  /*public Timer m_timer;
  public NetworkManager m_networkManager;


  public void Start()
  {
    if( !m_networkManager.m_actAsServer )
    {
      m_timer.StartTimer( PingServer );
    }
  }

  public void PingServer()
  {
    StartCoroutine( PerformPing() );
  }

  IEnumerator PerformPing()
  {
    WWW www = new WWW( m_networkManager.m_url );
    yield return www;
    Debug.Log( "Ping: " + www.isDone + ", " + www.text );

    m_timer.Restart();
  }*/
}