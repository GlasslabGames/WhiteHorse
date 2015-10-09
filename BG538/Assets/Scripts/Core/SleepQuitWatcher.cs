using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class SleepQuitWatcher : MonoBehaviour {
  public delegate void OnApplicationPauseDelegate(bool pauseStatus);
  public OnApplicationPauseDelegate OnApplicationPauseReceived;
  public delegate void OnApplicationQuitDelegate();
  public OnApplicationQuitDelegate OnApplicationQuitReceived;
  
  #if UNITY_EDITOR
  bool m_lastPaused = false;
  #endif
  
  protected void Awake()
  {
    #if UNITY_EDITOR
    EditorApplication.playmodeStateChanged += OnPlayModeStateChanged;
    #endif
  }
  
  // Callbacks for if the application pauses or exits to properly close the file (and flush the buffer to disk)
  void OnApplicationPause(bool pauseStatus) {
    if (OnApplicationPauseReceived != null) {
      OnApplicationPauseReceived (pauseStatus);
    }
  }
  
  void OnApplicationQuit() {
    if (OnApplicationQuitReceived != null) {
      OnApplicationQuitReceived ();
    }
  }
  
  #if UNITY_EDITOR
  void OnPlayModeStateChanged()
  {
    if (EditorApplication.isPaused && !m_lastPaused) {
      OnApplicationPause (true);
      m_lastPaused = true;
    } else if (!EditorApplication.isPaused && m_lastPaused) {
      OnApplicationPause (false);
      m_lastPaused = false;
    } 
    
    // This should always trigger if the application stops playing, regardless of previous tests.
    if (!EditorApplication.isPlaying) {
      OnApplicationQuit();
    }
  }
  #endif
}
