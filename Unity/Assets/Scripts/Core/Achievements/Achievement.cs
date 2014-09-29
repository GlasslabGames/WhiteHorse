using System;
using System.Collections.Generic;
using GlassLab.Core.Conditional;

public delegate void AchievementEvent(Achievement a);

public class Achievement {
  public bool IsCompleted {
    get { return m_isCompleted; }
    set { m_isCompleted = value; }
  }

  private bool m_isCompleted = false;

  public event AchievementEvent OnComplete;

  public List<Conditional> Conditionals {
    get { return m_conditionals; }
  }

  // -- Filled by deserialization --
  public string Name;
  public string Group;
  public string SubGroup;
  private List<Conditional> m_conditionals = new List<Conditional>();
  // ----------------

  public Achievement()
  {
    Name = "UntitledAchievement";
  }

  public Achievement(string name = "")
  {
    Name = name;
  }

  public void DEBUG_addCondition(Conditional c)
  {
    c.OnComplete += onConditionComplete;
    m_conditionals.Add(c);
  }

  private void onConditionComplete(Conditional c)
  {
    refresh();
  }

  public void Init()
  {
    if (!m_isCompleted)
    {
      for (int i=m_conditionals.Count-1; i>=0; i--)
      {
        m_conditionals[i].OnComplete += onConditionComplete;
        m_conditionals[i].Refresh();
      }
    }
    else
    {
      m_conditionals = null; // Throw away a bunch of work! (TODO: fix)
    }
  }

  private void refresh()
  {
    for (int i=m_conditionals.Count-1; i>=0; i--)
    {
      Conditional c = m_conditionals[i];
      if (!c.IsSatisfied)
      {
        return;
      }
    }

    onComplete();
  }

  private void onComplete()
  {
    for (int i=m_conditionals.Count-1; i>=0; i--)
    {
      Conditional c = m_conditionals[i];
      c.OnComplete -= onConditionComplete;
    }
    m_conditionals = null;

    if (OnComplete != null) OnComplete(this);
    if (SignalManager.AchievementUnlocked != null) SignalManager.AchievementUnlocked(this);
  }

  ~Achievement()
  {
    if (m_conditionals != null)
    {
      for (int i=m_conditionals.Count-1; i>=0; i--)
      {
        Conditional c = m_conditionals[i];
        c.OnComplete -= onConditionComplete;
      }
    }
  }
}
