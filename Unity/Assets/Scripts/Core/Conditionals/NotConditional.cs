using System;
using UnityEngine;

public class NotConditional : Conditional
{
  public Conditional conditional;

  public NotConditional()
  {
  }

  override public void Init()
  {
    conditional.OnChanged += onConditionChanged;
    conditional.Init();

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

  ~NotConditional()
  {
    conditional.OnChanged -= onConditionChanged;
  }
}