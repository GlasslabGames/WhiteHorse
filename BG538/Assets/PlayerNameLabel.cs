using UnityEngine;
using UnityEngine.UI;

public class PlayerNameLabel : MonoBehaviour {
	public string StartText;

	// Use this for initialization
	void Start () {
		GetComponent<Text>().text = StartText + PhotonNetwork.playerName;
	}
}
