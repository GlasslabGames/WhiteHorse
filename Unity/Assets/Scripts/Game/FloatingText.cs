using UnityEngine;
using System.Collections;


public class FloatingText : MonoBehaviour
{
  public float m_duration;
  private float m_currentTime;

  public UILabel m_label;

  private Vector3 m_startPosition;
  private Vector3 m_endPosition;
  public Vector3 m_offset;


  public void Display( string text )
  {
    m_label.text = text;
    m_currentTime = 0;

    m_startPosition = gameObject.transform.position;
    m_endPosition = m_startPosition + m_offset;
  }

  public void Update()
  {
    m_currentTime += Time.deltaTime;

    if( m_currentTime >= m_duration )
    {
      m_currentTime = m_duration;
      Destroy( gameObject );
    }

    gameObject.transform.position = Vector3.Lerp( m_startPosition, m_endPosition, m_currentTime / m_duration );
  }
}