using UnityEngine;
using System;
using System.Collections.Generic;


public enum TimerType
{
  OneShot,
  Continuous
}

public class Timer : MonoBehaviour
{
  public float m_duration;
  private float m_currentTime;

  private bool m_active;

  public TimerType m_type;
  private Action m_callback;


  public float CurrentTime
  {
    get { return m_currentTime; }
  }

  public float PercentageComplete
  {
    get {
      if( m_duration > 0 )
      {
        return m_currentTime / m_duration;
      }
      else
      {
        return 0;
      }
    }
  }

  public bool Active
  {
    get { return m_active; }
  }


  public void TimerCompleteCallback() {}

  public void StartTimer( Action callback = null )
  {
    m_currentTime = 0.0f;
    m_active = true;

    m_callback = callback == null ? TimerCompleteCallback : callback;
  }

  public void StopTimer()
  {
    m_active = false;
  }

  public void Restart()
  {
    StopTimer();
    StartTimer( m_callback );
  }

  public void Update()
  {
    if( m_active )
    {
      m_currentTime += Time.deltaTime;
      if( m_currentTime >= m_duration )
      {
        m_callback();

        if( m_type == TimerType.OneShot )
        {
          m_currentTime = m_duration;
          m_active = false;
        }
        else if( m_type == TimerType.Continuous )
        {
          m_currentTime = 0.0f;
        }
      }
    }
  }
}