using UnityEngine;

public class TweenNGUIPanel : MonoBehaviour
{
  public Vector4 From;
  public Vector4 To;
  public AnimationCurve Curve = new AnimationCurve();
  private UIPanel m_panel;
  private bool m_playForward;
  public float Duration = 1f;
  private float m_currentTime; // normalized time, range - [0, 1]

  void Awake()
  {
    m_panel = GetComponent<UIPanel>();
  }

  void Update()
  {
    float normalizedTime = Duration != 0 ? Time.deltaTime / Duration : 1;
    if (m_playForward)
    {
      m_currentTime += normalizedTime;
    }
    else
    {
      m_currentTime -= normalizedTime;
    }
    m_panel.clipRange = Vector4.Lerp(From, To, Curve.Evaluate(m_currentTime));

    if ((m_playForward && m_currentTime >= 1) || (!m_playForward && m_currentTime <= 0))
    {
      enabled = false;
    }
  }

  public void Toggle()
  {
    if (m_playForward)
    {
      PlayBackward();
    }
    else
    {
      PlayForward();
    }
  }

  public void PlayForward()
  {
    if (!enabled) enabled = true;
    m_playForward = true;
    m_currentTime = 0;
  }

  public void PlayBackward()
  {
    if (!enabled) enabled = true;
    m_playForward = false;
    m_currentTime = 1;
  }
}

