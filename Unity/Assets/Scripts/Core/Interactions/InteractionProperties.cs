using GlassLab.Core.Conditional;

// This is a separate class just to keep the inspector organized
[System.Serializable]
public class InteractionProperties {
  public Conditional[] m_conditionals;
  public int ActivatedBy; // give the id of the InventoryItem

  public bool OnceOnly;
  public bool DestroyAfterUse;

  public enum Priorities { // leaving some space if we need to add more later
    LOW = 1, MEDIUM = 3, HIGH = 5
  }
  public Priorities Priority;

  public bool IsSatisfied()
  {
    for (int i = m_conditionals.Length; i >= 0; i--)
    {
      if (!m_conditionals[i].IsSatisfied)
      {
        return false;
      }
    }

    return true;
  }
}