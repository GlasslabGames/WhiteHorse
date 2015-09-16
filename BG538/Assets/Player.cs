using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
 	public Leaning color;

	[SyncVar]
	public int Test;

	public Dictionary<State, int> WorkerCounts = new Dictionary<State, int>();

	void Start () {
		Debug.Log ("Start! " + isLocalPlayer);
		color = (isServer ^ isLocalPlayer)? Leaning.Blue : Leaning.Red;
		Test = (int) color;

		SpriteRenderer s = GetComponent<SpriteRenderer> ();
		if (s) s.color = (color == Leaning.Red)? Color.red : Color.blue;

		if (isLocalPlayer) GameManager.Instance.SetPlayer(this);
		GameManager.Instance.CheckPlayerCount();
	}

	void OnDestroy() {
		Debug.Log ("Destroy!", this);
		if (GameManager.Instance) GameManager.Instance.CheckPlayerCount();
	}

	public void PlaceWorker(State state) {
		if (!WorkerCounts.ContainsKey(state)) WorkerCounts.Add(state, 1);
		else WorkerCounts[state] ++;

		SetWorkers(state.Model.Abbreviation, WorkerCounts[state], color == Leaning.Blue);
	}

	public void RemoveWorker(State state) {
		if (!WorkerCounts.ContainsKey(state)) WorkerCounts.Add(state, 0);
		else WorkerCounts [state] --;

		SetWorkers(state.Model.Abbreviation, WorkerCounts[state], color == Leaning.Blue);
	}

	/***
	 * The goal of the following code is to set the number of workers of the target color in the target state on all clients.
	 * If we're connected as the server, we just call the RPC function on all clients.
	 * If we're connected as a client, we have to call the Command function on the server so it can notify all clients (including us.)
	 * Else, if we're offline, we just set the workers directly.
	 ***/
	void SetWorkers(string stateAbbreviation, int workerCount, bool isBluePlayer) {
		Debug.Log ("SetWorkers. isServer? " + isServer + " isClient? " + isClient, this);
		if (isServer) RpcSetWorkers(stateAbbreviation, workerCount, isBluePlayer);
		else if (isClient) CmdSetWorkers(stateAbbreviation, workerCount, isBluePlayer);
		else DoSetWorkers(stateAbbreviation, workerCount, isBluePlayer); // offline mode
	}

	// Called by any Player (acting as a client) on the single server, which sends it on to all clients
	[Command]
	public void CmdSetWorkers(string stateAbbreviation, int workerCount, bool isBluePlayer) {
		Debug.Log ("CmdSetWorkers. isServer? " + isServer + " isClient? " + isClient, this);
		RpcSetWorkers (stateAbbreviation, workerCount, isBluePlayer);
	}

	// Called by the server on every client to set the state workers
	[ClientRpc]
	public void RpcSetWorkers(string stateAbbreviation, int workerCount, bool isBluePlayer) {
		Debug.Log ("RpcSetWorkers. isServer? " + isServer + " isClient? " + isClient, this);
		DoSetWorkers (stateAbbreviation, workerCount, isBluePlayer);
	}

	void DoSetWorkers(string stateAbbreviation, int workerCount, bool isBluePlayer) {
		State state = GameManager.Instance.StatesByAbbreviation[stateAbbreviation];
		if (state) state.SetWorkerCount(workerCount, isBluePlayer);
	}
}
