using UnityEngine;
public delegate void ConditionEvent(Conditional c);

public class Conditional {
  private bool m_isSatisfied;
  public bool IsSatisfied
  {
    get { return m_isSatisfied; }
    protected set { 
      if (value != m_isSatisfied)
      {
        m_isSatisfied = value;
        if (OnChanged != null) OnChanged(this);
        
        if (m_isSatisfied)
        {
          Complete();
        }
      }
    }
  }

  public event ConditionEvent OnComplete;
  public event ConditionEvent OnChanged; // Event sent when satisfied state changed

  public virtual void Init()
  {
    Refresh();
  }

  // Override this!
  protected virtual bool CalculateIsSatisfied()
  {
    //Debug.LogError("[Conditional] CheckSatisfied has not been overridden. Defaulting to current satisfied state ("+IsSatisfied+").", this);
    return IsSatisfied;
  }

  public void Refresh()
  {
    IsSatisfied = CalculateIsSatisfied(); // Note: Setter has logic in it
  }

  private void Complete()
  {
    if (OnComplete != null) OnComplete(this);
  }
}