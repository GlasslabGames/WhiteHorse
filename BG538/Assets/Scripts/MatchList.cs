using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System.Collections.Generic;
using System.Linq;

public class MatchList : MonoBehaviour {
	public LobbyManager lobbyManager;
	public Transform ListParent;
	public GameObject EntryPrefab;
	public GameObject ProgressIndicator;
	public GameObject EmptyIndicator;

	private Dictionary<ulong, MatchmakerEntry> entriesByMatch = new Dictionary<ulong, MatchmakerEntry>();

	void Start() {
		EmptyIndicator.SetActive(false);

		InvokeRepeating("Refresh", 0, 5f);
	}

	void OnDestroy() {
		CancelInvoke();
	}

	[ContextMenu("Refresh")]
	public void Refresh() {
		if (isActiveAndEnabled) {
			ProgressIndicator.SetActive(true);
			lobbyManager.MatchMaker.ListMatches(0, 30, "", OnMatchList);
		}
	}

	public void OnMatchList(ListMatchResponse response) {
		ProgressIndicator.SetActive(false);

		EmptyIndicator.SetActive(response.matches.Count == 0);

		// We want to add new entries and then delete unused ones, without erasing and starting from scratch
		// Since that would mess up which entry is selected
		List<MatchmakerEntry> entries = new List<MatchmakerEntry>();
		MatchmakerEntry entry;
		foreach (MatchDesc match in response.matches) {
			if (entriesByMatch.ContainsKey((ulong) match.networkId)) {
				entry = entriesByMatch[(ulong) match.networkId];
				entriesByMatch.Remove((ulong) match.networkId);
			} else {
				GameObject go = Instantiate(EntryPrefab) as GameObject;
				go.transform.SetParent(ListParent, false);
				entry = go.GetComponent<MatchmakerEntry>();
				entry.Set(match);
			}
			entries.Add(entry);
		}

		// now everything left in entriesByMatch is old
		foreach(MatchmakerEntry oldEntry in entriesByMatch.Values) {
			Destroy(oldEntry.gameObject);
		}
		entriesByMatch.Clear();

		// and save the good entries for next time
		entriesByMatch = entries.ToDictionary(x => (ulong) x.Match.networkId, x => x);
		Debug.Log (entriesByMatch.Values);
	}
}
