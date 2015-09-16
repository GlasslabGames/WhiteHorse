using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
 	public Leaning color;

	public Dictionary<State, int> WorkerCounts = new Dictionary<State, int>();

	void Start () {
		Debug.Log ("Start! " + isLocalPlayer);
		color = (isServer ^ isLocalPlayer)? Leaning.Blue : Leaning.Red;

		if (isLocalPlayer) GameManager.Instance.SetPlayer(this);
		GameManager.Instance.CheckPlayerCount();
	}

	void OnDestroy() {
		Debug.Log ("Destroy!", this);
		if (GameManager.Instance) GameManager.Instance.CheckPlayerCount();
	}

	public void PlaceWorker(State state) {
		if (state.PlayerCanPlaceWorker()) {
			if (!WorkerCounts.ContainsKey(state)) WorkerCounts.Add(state, 1);
			else WorkerCounts[state] ++;

			state.AddWorker(true);
			GameManager.Instance.PlayerBudget.ConsumeAmount(GameSettings.Instance.GetGameActionCost(GameAction.PlaceWorker));

			SetWorkers(state.Model.Abbreviation, WorkerCounts[state], color == Leaning.Blue);
		}
	}

	public void RemoveWorker(State state) {
		if (state.PlayerCanRemoveWorker()) {
			if (!WorkerCounts.ContainsKey(state)) WorkerCounts.Add(state, 0);
			else WorkerCounts [state] --;

			state.RemoveWorker(true);
			GameManager.Instance.PlayerBudget.ConsumeAmount(GameSettings.Instance.GetGameActionCost(GameAction.RemoveWorker));

			SetWorkers(state.Model.Abbreviation, WorkerCounts[state], color == Leaning.Blue);
		}
	}

	/***
	 * The goal of the following code is to set the number of workers of the target color in the target state on ALL clients.
	 * If we're connected as the server, we just call the RPC function on all clients.
	 * If we're connected as a client, we have to call the Command function on the server so it can notify all clients (including us.)
	 * Else, if we're offline, we just set the workers directly.
	 ***/
	void SetWorkers(string stateAbbreviation, int workerCount, bool isBluePlayer) {
		if (isServer) RpcSetWorkers(stateAbbreviation, workerCount, isBluePlayer);
		else if (isClient) CmdSetWorkers(stateAbbreviation, workerCount, isBluePlayer);
		else DoSetWorkers(stateAbbreviation, workerCount, isBluePlayer); // offline mode
	}

	// Called by any Player (acting as a client) on the single server, which sends it on to all clients
	[Command]
	public void CmdSetWorkers(string stateAbbreviation, int workerCount, bool isBluePlayer) {
		RpcSetWorkers (stateAbbreviation, workerCount, isBluePlayer);
	}

	// Called by the server on every client to set the state workers
	[ClientRpc]
	public void RpcSetWorkers(string stateAbbreviation, int workerCount, bool isBluePlayer) {
		DoSetWorkers (stateAbbreviation, workerCount, isBluePlayer);
	}

	void DoSetWorkers(string stateAbbreviation, int workerCount, bool isBluePlayer) {
		State state = GameManager.Instance.StatesByAbbreviation[stateAbbreviation];
		if (state) state.SetWorkerCount(workerCount, isBluePlayer);
	}

	// Called when the player ends the turn to communicate that fact to the other player
	// There might be better way to do this with messages, but I think they still have to go through the server.
	public void FinishTurn() {
		bool isBlue = (color == Leaning.Blue);
		if (isServer) RpcFinishTurn(isBlue);
		else if (isClient) CmdFinishTurn(isBlue);
		else DoFinishTurn(isBlue);
	}

	[Command]
	public void CmdFinishTurn(bool isBluePlayer) {
		RpcFinishTurn(isBluePlayer);
	}

	[ClientRpc]
	public void RpcFinishTurn(bool isBluePlayer) {
		DoFinishTurn (isBluePlayer);
	}

	void DoFinishTurn(bool isBluePlayer) {
		GameManager.Instance.SetReady(isBluePlayer);
	}
}
