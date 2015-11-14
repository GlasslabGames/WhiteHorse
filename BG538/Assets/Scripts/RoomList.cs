﻿using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System.Collections.Generic;
using System.Linq;

public class RoomList : Photon.PunBehaviour {
	public Transform ListParent;
	public GameObject EntryPrefab;
	public GameObject EmptyIndicator;
	public GameObject ConnectingIndicator;

	private Dictionary<string, MatchmakerEntry> entriesByRoom = new Dictionary<string, MatchmakerEntry>();

	void Awake() {
		SignalManager.TryingPhotonConnect += OnConnecting;
	}

	void OnDestroy() {
		SignalManager.TryingPhotonConnect -= OnConnecting;
	}

	public void OnConnecting() {
		EmptyIndicator.SetActive(false);
		ConnectingIndicator.SetActive(true);
	}

	public void OnReceivedRoomListUpdate() {
		ConnectingIndicator.SetActive(false);

		RefreshWithRooms(PhotonNetwork.GetRoomList());
	}

	public void RefreshWithRooms(RoomInfo[] rooms) {
		EmptyIndicator.SetActive(rooms.Length == 0);
		string keyword = NetworkManager.Instance.GroupKeyword;

		// We want to add new entries and then delete unused ones, without erasing and starting from scratch
		// Since that would mess up which entry is selected
		List<MatchmakerEntry> entries = new List<MatchmakerEntry>();
		MatchmakerEntry entry;
		foreach (RoomInfo room in rooms) {
			if (!room.visible || !room.open || room.playerCount >= room.maxPlayers) continue;

			// check the group keyword
			ExitGames.Client.Photon.Hashtable props = room.customProperties;
			if (props != null && props.ContainsKey("k") && props["k"] != keyword) continue;

			if (entriesByRoom.ContainsKey(room.name)) {
				entry = entriesByRoom[room.name];
				entriesByRoom.Remove(room.name);
			} else {
				GameObject go = Instantiate(EntryPrefab) as GameObject;
				go.transform.SetParent(ListParent, false);
				entry = go.GetComponent<MatchmakerEntry>();
				entry.Set(room);
			}
			entries.Add(entry);
		}

		// now everything left in entriesByMatch is old
		foreach(MatchmakerEntry oldEntry in entriesByRoom.Values) {
			Destroy(oldEntry.gameObject);
		}
		entriesByRoom.Clear();

		// and save the good entries for next time
		entriesByRoom = entries.ToDictionary(x => x.RoomName, x => x);
		Debug.Log (entriesByRoom.Values);
	}
}
