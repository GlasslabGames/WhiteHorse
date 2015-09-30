using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;

public class NetworkManager : Photon.PunBehaviour {
	public bool ShowDebugInfo;
	public Text DebugLabel;

	static NetworkManager Instance;

	public enum DisconnectionReason {
		opponent,
		other
	}
	public static DisconnectionReason DisconnectionInfo;

	void Awake() {
		if (Instance) {
			Destroy(gameObject);
		} else {
			Instance = this;
			DontDestroyOnLoad(gameObject);
			
			PhotonNetwork.playerName = GetRandomName();
			PhotonNetwork.ConnectUsingSettings(GameSettings.InstanceOrCreate.Version);
		}
	}

	public static string GetRandomName() {
		string[] adjectives = {"Yellow", "Black", "White", "Purple", "Orange", "Green", "Awesome", "Silly",
		"Patriotic", "Valiant", "Wise", "Strong", "Careful"};
		string[] nouns = {"Cat", "Dog", "Turkey", "Eagle", "Lion", "Bear", "Pony", "Badger", "Shark", "Horse", "Elephant", "Donkey" };
		string name = adjectives[Mathf.FloorToInt(Random.value * adjectives.Length)] +
			nouns[Mathf.FloorToInt(Random.value * nouns.Length)];
		return name;
	}

	public static void CreateRoom(string name, int scenarioId, int color, float workerInfluence, int duration) { 
		RoomOptions options = new RoomOptions();
		options.maxPlayers = 2;
		options.customRoomProperties = new Hashtable() {
			{ "s", scenarioId },
			{ "n", name },
			{ "c", color },
			{ "w", workerInfluence },
			{ "d", duration }
		};
		options.customRoomPropertiesForLobby = new string[] { "s", "n", "c" };
		PhotonNetwork.CreateRoom(null, options, null);
	}

	public static void JoinRoom(string name) {
		PhotonNetwork.JoinRoom(name);
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
			GameManager.Instance.GoToPhase(TurnPhase.BeginGame);
		}
	}

	void OnDisconnected() {
		if (GameManager.Instance) {
			GameManager.Instance.GoToPhase(TurnPhase.Disconnected);
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
