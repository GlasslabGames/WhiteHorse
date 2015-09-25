using UnityEngine;
using UnityEngine.UI;

public class RandomMatchmaker : Photon.PunBehaviour
{
    private PhotonView myPhotonView;
	public Text InfoLabel;

    // Use this for initialization
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(GameSettings.InstanceOrCreate.Version);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("JoinRandom");
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnPhotonRandomJoinFailed()
    {
        PhotonNetwork.CreateRoom(null);
    }

    public override void OnJoinedRoom()
    {
        /*GameObject monster = PhotonNetwork.Instantiate("monsterprefab", Vector3.zero, Quaternion.identity, 0);
        monster.GetComponent<myThirdPersonController>().isControllable = true;
        myPhotonView = monster.GetComponent<PhotonView>();*/
    }

	void Update() {
		InfoLabel.text = PhotonNetwork.connectionStateDetailed.ToString();
	}
	/*
    public void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());

        if (PhotonNetwork.connectionStateDetailed == PeerState.Joined)
        {
            bool shoutMarco = GameLogic.playerWhoIsIt == PhotonNetwork.player.ID;

            if (shoutMarco && GUILayout.Button("Marco!"))
            {
                myPhotonView.RPC("Marco", PhotonTargets.All);
            }
            if (!shoutMarco && GUILayout.Button("Polo!"))
            {
                myPhotonView.RPC("Polo", PhotonTargets.All);
            }
        }
    }
    */
}
