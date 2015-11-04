using UnityEngine;
using System.Collections.Generic;

public class SoundController : SingletonBehavior<SoundController> 
{
	public static Dictionary<string, AudioSource> sounds = new Dictionary<string, AudioSource>();
	public static AudioSource musicSource;
	public static float initialMusicVolume;

	public static bool SoundOn {
		get {
			return AudioListener.volume > 0;
		}
	}
	public static bool MusicOn {
		get {
			return musicSource != null && musicSource.volume > 0;
		}
	}
	
	void Awake ()
	{
		base.Awake();

		if (SoundController.Instance == this) {
			DontDestroyOnLoad(gameObject);

			foreach (AudioSource s in GetComponentsInChildren<AudioSource>()) {
				sounds.Add(s.name, s);
				if (s.loop) {
					musicSource = s;
					initialMusicVolume = s.volume;
					s.ignoreListenerVolume = true; // allows us to control SFX volume without affecting music
				}
			}
		}
	}

	public static void Play(string soundName) {
		// load the sound controller from a prefab if we don't have it
		SoundController check = SoundController.InstanceOrCreate;

		if (sounds.ContainsKey(soundName)) {
			sounds[soundName].Play();

			if (sounds[soundName].priority < musicSource.priority) {
				musicSource.Pause();
			}
		} else {
			Debug.LogError("No sound named "+soundName+"!");
		}
	}

	public static void ToggleSound() {
		if (AudioListener.volume > 0) AudioListener.volume = 0;
		else AudioListener.volume = 1;
	}

	public static void ToggleMusic() {
		if (musicSource.volume > 0) musicSource.volume = 0;
		else musicSource.volume = initialMusicVolume;
	}
}
