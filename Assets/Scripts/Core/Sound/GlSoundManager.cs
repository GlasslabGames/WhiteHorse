using UnityEngine;
using System.Collections;
using Fabric;

public class GlSoundManager : SingletonBehavior<GlSoundManager> {
  public bool Muted {
    get {
      return PlayerPrefs.GetInt("muted") == 1;
    }
    set {
      PlayerPrefs.SetInt("muted", value? 1 : 0);
    }
  }
  public GroupComponent m_musicGroup;
  public GroupComponent m_soundFxGroup;

  public void Awake() {
    if (Muted) Mute ();
    else Unmute();
  }

  public void Toggle() {
    if (Muted) Unmute();
    else Mute ();
  }

	public void Mute() {
    m_musicGroup.SetVolume(-100);
    m_soundFxGroup.SetVolume(-100);
    Muted = true;
  }

  public void Unmute() {
    m_musicGroup.SetVolume(100);
    m_soundFxGroup.SetVolume(100);
    Muted = false;
  }
}
