/**
 * Consumable event
 */
public class GLConsumableEventArgs : System.EventArgs
{
  public bool isConsumed { get { return m_isConsumed; } }
  public void Consume() {
    m_isConsumed = true;
  }
  private bool m_isConsumed = false;
}

public delegate void OnGLEventHandler(GLConsumableEventArgs args);