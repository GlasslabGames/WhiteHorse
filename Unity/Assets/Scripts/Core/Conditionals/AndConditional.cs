using System;
using System.Collections.Generic;
using UnityEngine;

public class AndConditional : Conditional
{
  public Conditional[] Conditionals;
  
  public AndConditional()
  {
  }
  
  override public void Init()
  {
    for (int i=Conditionals.Length-1; i>=0; i--)
    {
      Conditional conditional = Conditionals[i];
      conditional.OnChanged += onConditionChanged;
      conditional.Init();
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
      if (!conditional.IsSatisfied)
      {
        return false;
      }
    }

    return true;
  }
  
  ~AndConditional()
  {
    for (int i=Conditionals.Length-1; i>=0; i--)
    {
      Conditional conditional = Conditionals[i];
      conditional.OnChanged -= onConditionChanged;
    }
  }
}