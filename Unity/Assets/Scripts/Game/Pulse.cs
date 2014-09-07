using UnityEngine;
using System.Collections;


public class Pulse : MonoBehaviour
{
  public Timer m_bounceTimerOut;
  public Timer m_bounceTimerIn;
  
  private Vector3 m_scaleStart;
  public Vector3 m_scaleEnd;
  
  
  public void Awake()
  {
    m_scaleStart = gameObject.transform.localScale;
  }
  
  public void BounceOut()
  {
    m_bounceTimerOut.StopTimer();
    m_bounceTimerIn.StopTimer();

    m_bounceTimerOut.StartTimer( BounceIn );
  }
  public void BounceIn()
  {
    m_bounceTimerIn.StartTimer( Complete );
  }
  public void Complete()
  {
    gameObject.transform.localScale = m_scaleStart;
  }
  
  public void Update()
  {
    if( m_bounceTimerOut.Active )
    {
      gameObject.transform.localScale = Vector3.Lerp( m_scaleStart, m_scaleEnd, m_bounceTimerOut.PercentageComplete );
    }
    
    if( m_bounceTimerIn.Active )
    {
      gameObject.transform.localScale = Vector3.Lerp( m_scaleEnd, m_scaleStart, m_bounceTimerIn.PercentageComplete );
    }
  }
}