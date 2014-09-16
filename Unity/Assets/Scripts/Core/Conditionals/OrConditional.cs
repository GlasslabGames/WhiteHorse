using System;
using System.Collections.Generic;
using UnityEngine;

namespace GlassLab.Core.Conditional
{
  public class OrConditional : Conditional
  {
    public Conditional[] Conditionals;

    override protected void Init()
    {
      for (int i = Conditionals.Length - 1; i >= 0; i--)
      {
        Conditional conditional = Conditionals[i];
        if (conditional != null)
        {
          conditional.OnChanged += onConditionChanged;
          conditional.doInit();
        }
      }

      base.Init();
    }

    private void onConditionChanged(Conditional c)
    {
      Refresh();
    }

    override protected bool CalculateIsSatisfied()
    {
      for (int i = Conditionals.Length - 1; i >= 0; i--)
      {
        Conditional conditional = Conditionals[i];
        if (conditional != null && conditional.IsSatisfied)
        {
          return true;
        }
      }

      return false;
    }

    void OnDestroy()
    {
      for (int i = Conditionals.Length - 1; i >= 0; i--)
      {
        Conditional conditional = Conditionals[i];
        if (conditional != null)
        {
          conditional.OnChanged -= onConditionChanged;
        }
      }
    }
  }
}