using UnityEngine;
using System.Collections.Generic;
public class GLCurtainController : MonoBehaviour 
{
  // Curtains
  public GameObject[] Curtains;
  public List<GameObject> ColorTweenTargets;

  // Positions
  private Vector3[] m_originalDeltas;
  private float m_distance; // Range [0, 1] - 1 for open, 0 for shut.
  public float Distance
  {
    get
    {
      return m_distance;
    }
    set
    {
      m_distance = Mathf.Clamp01(value);

      Refresh();
    }
  }

  public List<GameObject> GetColorTweenTargets()
  {
    return ColorTweenTargets;
  }

  void Awake()
  {
    RefreshStartingPositions ();
  }

  public void Refresh()
  {
    for (int i=Curtains.Length-1; i >= 0; i--)
    {
			Curtains[i].transform.localPosition = transform.localPosition - (m_originalDeltas[i] * (1f-m_distance));
    }
  }

  public void RefreshStartingPositions()
  {
    m_originalDeltas = new Vector3[Curtains.Length];
    for (int i=Curtains.Length-1; i >= 0; i--)
    {
			m_originalDeltas[i] = transform.localPosition - Curtains [i].transform.localPosition;
    }

    m_distance = 0f;
  }
}
