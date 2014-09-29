using UnityEngine;
using System.Collections;
using GlassLab.Core.Conditional;

public class TestConditionalHolder : MonoBehaviour {
  public Conditional[] c;

  void Awake()
  {

    for (int i = 0; i < c.Length; i++)
    {
      if (c[i] != null)
      {
        Debug.Log(i + ": "+c[i].GetType().FullName);
      }
      else
      {
        Debug.Log(i + ": <None>");
      }
    }
  }
}
