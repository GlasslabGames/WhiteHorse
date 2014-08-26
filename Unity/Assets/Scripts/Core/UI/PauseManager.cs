

using System.Collections.Generic;
using UnityEngine;

/*
 * PauseManager handles the pause state of the game using a list of keys requesting a pause. If any key is in the list, the game is paused.
 */
public class PauseManager : SingletonBehavior<PauseManager>
{
  private List<object> m_pauseHideStack = new List<object>();

  public bool IsPaused {
    get {
      return m_pauseHideStack.Count > 0;
    }
  }

  public void Pause(GameObject go) {
    if (m_pauseHideStack.IndexOf(go) == -1)
    {
      m_pauseHideStack.Add(go);
      
      //Debug.Log ("Pausing with "+go.name+"\nPaused List: "+getPausedListString());
      if (SignalManager.Paused != null) {
        SignalManager.Paused(IsPaused);
      }
    }
    else
    {
      Debug.Log("[ExplorationUIManager] Already paused with "+(go != null ? go.name : "null"));
    }
  }

  public void Unpause(GameObject go) {
    if (m_pauseHideStack.Contains(go))
    {
      m_pauseHideStack.Remove(go);
      
      Debug.Log ("[ExplorationUIManager] Removing "+go.name+" from pause list.\nPaused List: "+getPausedListString());
      if (SignalManager.Paused != null) {
        SignalManager.Paused(IsPaused);
      }
    }
    else
    {
      Debug.Log("[ExplorationUIManager] Tried to unpause using "+(go != null ? go.name : "null") + ", but it wasn't in the pause list.\nPaused List: "+getPausedListString());
    }
  }

  private string getPausedListString()
  {
    string debugString = "";
    for (int i = 0; i < m_pauseHideStack.Count; i++)
    {
      debugString += m_pauseHideStack[i].ToString() + "\n";
    }

    return debugString;
  }
}