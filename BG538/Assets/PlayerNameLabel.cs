using UnityEngine;
using UnityEngine.UI;

public class PlayerNameLabel : MonoBehaviour {
	public string StartText;

	void Start () {
		if (SdkManager.username != null && SdkManager.username.Length > 0) {
			GetComponent<Text>().text = StartText + SdkManager.username;
		} else {
			GetComponent<Text>().text = "";
		}
	}
}
