using UnityEngine;
using System.Collections;
#if FABRIC
using Fabric;
#endif

public class GlSoundManager : SingletonBehavior<GlSoundManager> {
  public bool Muted {
    get {
      return PlayerPrefs.GetInt("muted") == 1;
    }
    set {
      PlayerPrefs.SetInt("muted", value? 1 : 0);
    }
  }
#if FABRIC
  public GroupComponent m_musicGroup;
  public GroupComponent m_soundFxGroup;
#endif

  override protected void Awake() {
      base.Awake();
    if (Muted) Mute ();
    else Unmute();
  }

  public void Toggle() {
    if (Muted) Unmute();
    else Mute ();
  }

	public void Mute() {
#if FABRIC
    m_musicGroup.SetVolume(-100);
    m_soundFxGroup.SetVolume(-100);
#endif
    Muted = true;
  }

  public void Unmute() {
#if FABRIC
    m_musicGroup.SetVolume(100);
    m_soundFxGroup.SetVolume(100);
#endif
    Muted = false;
  }
}
