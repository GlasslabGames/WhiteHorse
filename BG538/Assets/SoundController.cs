using UnityEngine;
using System.Collections.Generic;

public class SoundController : MonoBehaviour 
{
	public static Dictionary<string, AudioSource> sounds = new Dictionary<string, AudioSource>();
	
	void Awake ()
	{
		DontDestroyOnLoad(gameObject);

		foreach (AudioSource s in GetComponentsInChildren<AudioSource>()) {
			sounds.Add(s.name, s);
		}
	}

	public static void Play(string soundName) {
		if (sounds.ContainsKey(soundName)) {
			sounds[soundName].Play();
		} else {
			Debug.LogError("No named "+soundName+"!");
		}
	}
}
