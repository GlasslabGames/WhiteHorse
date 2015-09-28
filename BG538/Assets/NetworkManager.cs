using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;

public class NetworkManager : Photon.PunBehaviour {
	public bool ShowDebugInfo;
	public Text DebugLabel;

	public enum DisconnectionReason {
		opponent,
		other
	}
	public static DisconnectionReason DisconnectionInfo;

	void Start () {
		DontDestroyOnLoad(gameObject);
		PhotonNetwork.ConnectUsingSettings(GameSettings.InstanceOrCreate.Version);
	}

	public static void CreateRoom(string name, int scenarioId, int color) { 
		RoomOptions options = new RoomOptions();
		options.maxPlayers = 2;
		options.customRoomProperties = new Hashtable() { { "s", scenarioId }, { "n", name }, { "c", color } };
		options.customRoomPropertiesForLobby = new string[] { "s", "n", "c" };
		PhotonNetwork.CreateRoom(null, options, null);
	}

	public static void JoinRoom(string name) {
		PhotonNetwork.JoinRoom(name);
	}


	[ContextMenu("Test create room")]
	public void TestCreateRoom() {
		CreateRoom("Elena M.", 102, 2);
	}

	[ContextMenu("Test leave room")]
	public void TestLeaveRoom() {
		PhotonNetwork.LeaveRoom();
	}

	public override void OnJoinedRoom()
	{
		Debug.Log ("Joined room! "+PhotonNetwork.room);
		Application.LoadLevel("game");
	}
	
	public override void OnLeftRoom() {
		// Show the disconnected popup, which has a button to return to the menu
		DisconnectionInfo = DisconnectionReason.other;
		OnDisconnected();
  }

	public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer) {
		if (PhotonNetwork.room != null && PhotonNetwork.room.playerCount < PhotonNetwork.room.maxPlayers) {
			PhotonNetwork.LeaveRoom();
			DisconnectionInfo = DisconnectionReason.opponent; // TODO
		}
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer otherPlayer) {
		if (PhotonNetwork.room != null && PhotonNetwork.room.playerCount >= PhotonNetwork.room.maxPlayers
		    && GameManager.Instance) {
			GameManager.Instance.GoToState(TurnPhase.BeginGame);
		}
	}

	void OnDisconnected() {
		if (GameManager.Instance) {
			GameManager.Instance.GoToState(TurnPhase.Disconnected);
    	}
	}
    
	public override void OnPhotonCreateRoomFailed(object[] codeAndMsg)
	{
		Debug.Log ("Failed to create room. "+codeAndMsg[1].ToString());
	}
	
	public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
	{
		Debug.Log ("Failed to join room. "+codeAndMsg[1].ToString());
	}

	public void OnReceivedRoomListUpdate() {
		Debug.Log ("New room list: ");
		foreach (RoomInfo room in PhotonNetwork.GetRoomList())
		{
			Debug.Log(room.name + " " + room.playerCount + "/" + room.maxPlayers);
		}
	}
	
	void Update () {
		if (ShowDebugInfo && DebugLabel != null) {
			DebugLabel.text = PhotonNetwork.connectionStateDetailed.ToString();
			if (PhotonNetwork.inRoom) {
				DebugLabel.text += " | " + PhotonNetwork.room.ToString();
			}
		}
	}
}
