using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class PlaySoundOnButton : MonoBehaviour {
	public string soundName = "Button";

	void Start () {
		Button button = GetComponent<Button>();
		button.onClick.AddListener(() => SoundController.Play(soundName));
	}
}
