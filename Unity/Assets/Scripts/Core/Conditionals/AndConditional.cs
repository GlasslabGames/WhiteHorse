using System.Collections.Generic;

public class AndConditional : Conditional
{
  public List<Conditional> Conditionals;
  
  public AndConditional()
  {
  }
  
  override public void Init()
  {
    for (int i=Conditionals.Count-1; i>=0; i--)
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
    for (int i=Conditionals.Count-1; i>=0; i--)
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
    for (int i=Conditionals.Count-1; i>=0; i--)
    {
      Conditional conditional = Conditionals[i];
      conditional.OnChanged -= onConditionChanged;
    }
  }
}