using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ConnectingPopup : MonoBehaviour {
	public float shortHeight;
	public float tallHeight;
	public Text keywordText;

	void OnEnable() {
		keywordText.gameObject.SetActive (NetworkManager.Instance.GroupKeyword.Length > 0);
		RectTransform rt = transform.GetChild(0) as RectTransform;
		rt.sizeDelta = new Vector2 (rt.sizeDelta.x, (keywordText.gameObject.activeSelf) ? tallHeight : shortHeight);

		if (keywordText.gameObject.activeSelf) {
			keywordText.text = "Class keyword: <color=red>" + NetworkManager.Instance.GroupKeyword + "</color>";
		}
	}
}
