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
		if (PhotonNetwork.connected) PhotonNetwork.Disconnect();
		MultiplayerMode = false;
	}

	public static void Connect() {
		if (!PhotonNetwork.connected) {
			if (SdkManager.username != null && SdkManager.username.Length > 0) PhotonNetwork.playerName = SdkManager.username;
			else PhotonNetwork.playerName = GetRandomName();

			if (SignalManager.TryingPhotonConnect != null) SignalManager.TryingPhotonConnect();

			PhotonNetwork.ConnectUsingSettings(GameSettings.InstanceOrCreate.Version);
		}
	}

	// For demo purposes
	public static string GetRandomName() {
		string[] names = {"Rose", "Jerry", "Paula", "Erin", "Ben"};
		return names[Random.Range(0, names.Length)]; 
	}

	// For testing purposes. In the real game we would use the GLGS login
	public static string GetRandomName2() {
		string[] adjectives = {"Yellow", "Black", "White", "Purple", "Orange", "Green", "Awesome", "Silly",
		"Patriotic", "Valiant", "Wise", "Strong", "Careful"};
		string[] nouns = {"Cat", "Dog", "Turkey", "Eagle", "Lion", "Bear", "Pony", "Badger", "Shark", "Horse", "Elephant", "Donkey" };
		string name = adjectives[Random.Range(0, adjectives.Length)] +
			nouns[Random.Range(0, nouns.Length)];
		return name;
	}

	public static void CreateRoom(string name, GameSettings settings) { 
		RoomOptions options = new RoomOptions();
		options.maxPlayers = 2;

		Hashtable table = new Hashtable() {
			{ "s", settings.currentScenarioId },
			{ "n", name },
			{ "c", settings.currentColor },
			{ "w", settings.currentIncrement },
			{ "d", settings.currentDuration }
		};

		options.customRoomProperties = table;
		options.customRoomPropertiesForLobby = new string[] { "s", "n", "c" };
		PhotonNetwork.CreateRoom(null, options, null);
	}

	public static void JoinRoom(string name) {
		PhotonNetwork.JoinRoom(name);
	}

	public static void StartOfflineGame() {
		PhotonNetwork.offlineMode = true;
		PhotonNetwork.JoinRoom("offline");
	}

	public override void OnJoinedRoom()
	{
		Debug.Log ("Joined room! "+PhotonNetwork.room);

		// Overwrite our game options with the options that come with the room
		Hashtable props = PhotonNetwork.room.customProperties;
		GameSettings settings = GameSettings.InstanceOrCreate;
		if (props.ContainsKey("s")) settings.currentScenarioId = (int) props["s"];
		if (props.ContainsKey("c")) settings.currentColor = (Leaning) props["c"];
		if (props.ContainsKey("d")) settings.currentDuration = (int) props["d"];
		if (props.ContainsKey("w")) settings.currentIncrement = (float) props["w"];

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
		if (PhotonErrorPopup.Instance) {
			PhotonErrorPopup.Instance.ShowError("Failed to create multiplayer game! Error: ", codeAndMsg);
		}
	}
	
	public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
	{
		Debug.Log ("Failed to join room. "+codeAndMsg[1].ToString());
		if (PhotonErrorPopup.Instance) {
			PhotonErrorPopup.Instance.ShowError("Failed to join multiplayer game! Error: ", codeAndMsg);
		}
	}

	public override void OnFailedToConnectToPhoton(DisconnectCause cause) {
		Debug.LogError("Failed to connect. "+cause);
		if (PhotonErrorPopup.Instance) PhotonErrorPopup.Instance.ShowConnectionError(cause);
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
