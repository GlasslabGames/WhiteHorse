using UnityEngine;
using UnityEngine.UI;

public class PlayerNameLabel : MonoBehaviour {
	public string StartText;

	void Start () {
		if (SdkManager.username != null && SdkManager.username.Length > 0) {
			transform.parent.gameObject.SetActive(true);
			GetComponent<Text>().text = StartText + SdkManager.username;
		} else {
			transform.parent.gameObject.SetActive(false);
		}
	}
}
