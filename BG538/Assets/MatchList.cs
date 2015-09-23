using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System.Collections;

public class MatchList : MonoBehaviour {
	public LobbyManager lobbyManager;
	public Transform ListParent;
	public GameObject EntryPrefab;
	public GameObject ProgressIndicator;
	public GameObject EmptyIndicator;

	void Start() {
		EmptyIndicator.SetActive(false);

		StartCoroutine(TimedRefresh());
	}

	[ContextMenu("Refresh")]
	public void Refresh() {
		ProgressIndicator.SetActive(true);
		lobbyManager.MatchMaker.ListMatches(0, 30, "", OnMatchList);
	}

	public void OnMatchList(ListMatchResponse response) {
		ProgressIndicator.SetActive(false);

		// Erase existing entries
		foreach (Transform t in ListParent) {
			Destroy(t.gameObject);
		}

		EmptyIndicator.SetActive(response.matches.Count == 0);

		foreach (MatchDesc match in response.matches) {
			GameObject go = Instantiate(EntryPrefab) as GameObject;
			go.transform.SetParent(ListParent, false);
			go.GetComponent<MatchmakerEntry>().Set(match);
		}
	}

	IEnumerator TimedRefresh() {
		while (true) {
			if (isActiveAndEnabled) {
				Refresh();
				yield return new WaitForSeconds(5f);
			}
		}
	}
}
