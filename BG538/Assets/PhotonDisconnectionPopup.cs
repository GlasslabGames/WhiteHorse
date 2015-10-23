using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ExitGames.Client.Photon;

public class PhotonDisconnectionPopup : UnityEngine.MonoBehaviour {
	public Text infoLabel;

	void Show(object[] error) {
		short errorCode = (short) error[0];
		if (errorCode == ErrorCode.MaxCcuReached) {
			infoLabel.text = "Unfortunately, the game can't accept any more players right now. Please try again later.";
		} else {
			infoLabel.text = "Couldn't connect to the game server. Error: ";
			infoLabel.text += (string) error[1];
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
