using UnityEngine;
using System.Collections;

namespace GlassLab.Core.Conditional
{
  public class ConditionalTester : MonoBehaviour
  {
    public Conditional Conditional;

    public bool IsSatisfied()
    {
      return Conditional.IsSatisfied;
    }

    void Awake()
    {
      Debug.Log(IsSatisfied());
    }
  }
}