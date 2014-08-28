using System;
using UnityEngine;

namespace GlassLab.Core.Conditional
{
  public class NotConditional : Conditional
  {
    public Conditional conditional;

    override protected void Init()
    {
      conditional.OnChanged += onConditionChanged;
      conditional.doInit();

      base.Init();
    }

    private void onConditionChanged(Conditional c)
    {
      Refresh();
    }

    override protected bool CalculateIsSatisfied()
    {
      return !conditional.IsSatisfied;
    }

    void OnDestroy()
    {
      conditional.OnChanged -= onConditionChanged;
    }
  }
}