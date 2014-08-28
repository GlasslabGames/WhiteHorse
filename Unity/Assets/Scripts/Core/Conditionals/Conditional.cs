using UnityEngine;
using System;
using System.Linq;
public delegate void ConditionEvent(Conditional c);

[Serializable]
public class Conditional : ScriptableObject {
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

#if UNITY_EDITOR
  // This block is used only for unity editor to assist with inspector creation of Conditionals
  static Conditional()
  {
    ALL_CONDITIONAL_TYPES = typeof(Conditional).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Conditional))).ToArray<Type>();
    int numConditionals = ALL_CONDITIONAL_TYPES.Length;
    ALL_CONDITIONAL_NAMES = new String[numConditionals + 1];
    for (int i = numConditionals - 1; i>=0; i--)
    {
      ALL_CONDITIONAL_NAMES[i] = Conditional.ALL_CONDITIONAL_TYPES[i].FullName;
    }

    ALL_CONDITIONAL_NAMES[numConditionals] = "<None>";
  }
  public static readonly Type[] ALL_CONDITIONAL_TYPES;
  public static string[] ALL_CONDITIONAL_NAMES;
#endif

  public event ConditionEvent OnComplete;
  public event ConditionEvent OnChanged; // Event sent when satisfied state changed

  public virtual void Init()
  {
    Refresh();
  }

  // Override this!
  protected virtual bool CalculateIsSatisfied()
  {
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