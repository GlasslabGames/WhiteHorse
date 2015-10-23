using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ExitGames.Client.Photon;

public class NetworkManager : Photon.PunBehaviour {
	public bool ShowDebugInfo;
	public Text DebugLabel;

	public static bool MultiplayerMode { get; private set; }

	public static NetworkManager Instance;

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
		}
	}

	public static void StartMultiplayer() {
		Connect ();
		MultiplayerMode = true;
	}

	public static void EndMultiplayer() {
		PhotonNetwork.Disconnect();
		MultiplayerMode = false;
	}

	public static void Connect() {
		if (SdkManager.username != null && SdkManager.username.Length > 0) PhotonNetwork.playerName = SdkManager.username;
		else PhotonNetwork.playerName = GetRandomName();
		PhotonNetwork.ConnectUsingSettings(GameSettings.InstanceOrCreate.Version);
	}

	// For testing purposes. In the real game we would use the GLGS login
	public static string GetRandomName() {
		string[] adjectives = {"Yellow", "Black", "White", "Purple", "Orange", "Green", "Awesome", "Silly",
		"Patriotic", "Valiant", "Wise", "Strong", "Careful"};
		string[] nouns = {"Cat", "Dog", "Turkey", "Eagle", "Lion", "Bear", "Pony", "Badger", "Shark", "Horse", "Elephant", "Donkey" };
		string name = adjectives[Mathf.FloorToInt(Random.value * adjectives.Length)] +
			nouns[Mathf.FloorToInt(Random.value * nouns.Length)];
		return name;
	}

	public static void CreateRoom(Dictionary<string, object> dict) { 
		RoomOptions options = new RoomOptions();
		options.maxPlayers = 2;
		options.customRoomProperties = DictionaryToHashtable(dict);
		options.customRoomPropertiesForLobby = new string[] { "s", "n", "c" };
		PhotonNetwork.CreateRoom(null, options, null);
	}

	// shortens the key names and converts to a hashtable suitable for roomOptions
	public static Hashtable DictionaryToHashtable(Dictionary<string, object> dict) {
		Hashtable table = new Hashtable() {
			{ "s", dict["scenarioId"] },
			{ "n", dict["name"] },
			{ "c", dict["color"] },
			{ "w", dict["influence"] },
			{ "d", dict["duration"] }
		};

		return table;
	}

	// back to the readable dictionary form
	public static Dictionary<string, object> HashtableToDictionary(Hashtable table) {
		Dictionary<string, object> dict = new Dictionary<string, object> {
			{ "scenarioId", table["s"] },
			{ "name", table["n"] },
			{ "color", table["c"] },
			{ "influence", table["i"] },
			{ "duration", table["d"] }
		};

		return dict;
	}

	public static void JoinRoom(string name) {
		PhotonNetwork.JoinRoom(name);
	}

	public override void OnJoinedLobby() {

	}

	public override void OnJoinedRoom()
	{
		Debug.Log ("Joined room! "+PhotonNetwork.room);

		// Overwrite our game options with the options that come with the room
		Hashtable props = PhotonNetwork.room.customProperties;
		GameSettings.InstanceOrCreate.CurrentOptions = NetworkManager.HashtableToDictionary(props);

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

	public override void OnReceivedRoomListUpdate() {
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
