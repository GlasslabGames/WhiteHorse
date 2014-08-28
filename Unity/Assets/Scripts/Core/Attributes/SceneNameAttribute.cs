using UnityEngine;

public class SceneNameAttribute : PropertyAttribute {
  public enum Flags {
    NONE = 0,
    ALLOW_NONE = 1 << 0,
    HIDE_CURRENT = 1 << 1,
  }

  protected Flags m_flags = Flags.NONE;

  public bool AllowNone {
    get { return (m_flags & Flags.ALLOW_NONE) != 0; }
  }

  public bool HideCurrent {
    get { return (m_flags & Flags.HIDE_CURRENT) != 0; }
  }


  public SceneNameAttribute() {}
  public SceneNameAttribute(Flags flags) 
  {
      m_flags = flags;
  }
}
