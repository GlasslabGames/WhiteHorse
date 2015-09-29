using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : Photon.PunBehaviour {
 	public Leaning color;

	private bool _finished;
	public bool Finished {
		get { return _finished; }
		set { SetFinished (value); }
	}

	public Dictionary<State, int> WorkerCounts = new Dictionary<State, int>();

	void Start () {
		/*Debug.Log ("Start! " + isLocalPlayer);
		color = (isServer ^ isLocalPlayer)? Leaning.Blue : Leaning.Red;

		GameManager.Instance.SetPlayer(this, isLocalPlayer);*/
	}

	void OnDestroy() {
		Debug.Log ("Destroy!", this);
		//if (GameManager.Instance) GameManager.Instance.CheckPlayerCount();
	}

	public void SetFinished(bool b) {	
		/*if (isServer) _finished = b;
		else if (isClient) CmdSetFinished(b);
		else OnFinishedChange(b);*/
	}
		
	public void CmdSetFinished(bool b) {
		_finished = b;
	}

	void OnFinishedChange(bool b) {
		_finished = b;
		if (b && SignalManager.PlayerFinished != null) SignalManager.PlayerFinished(color);
	}

	public void PlaceWorker(State state) {
		if (state.PlayerCanPlaceWorker()) {
			if (!WorkerCounts.ContainsKey(state)) WorkerCounts.Add(state, 1);
			else WorkerCounts[state] ++;

			state.AddWorker(true);
			GameManager.Instance.PlayerBudget.ConsumeAmount(GameSettings.InstanceOrCreate.GetGameActionCost(GameAction.PlaceWorker));

			SetWorkers(state.Model.Abbreviation, WorkerCounts[state], color == Leaning.Blue);
		}
	}

	public void RemoveWorker(State state) {
		if (state.PlayerCanRemoveWorker()) {
			if (!WorkerCounts.ContainsKey(state)) WorkerCounts.Add(state, 0);
			else WorkerCounts [state] --;

			state.RemoveWorker(true);
			GameManager.Instance.PlayerBudget.ConsumeAmount(GameSettings.InstanceOrCreate.GetGameActionCost(GameAction.RemoveWorker));

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
		/*if (isServer) RpcSetWorkers(stateAbbreviation, workerCount, isBluePlayer);
		else if (isClient) CmdSetWorkers(stateAbbreviation, workerCount, isBluePlayer);
		else DoSetWorkers(stateAbbreviation, workerCount, isBluePlayer); // offline mode
		*/
	}

	// Called by any Player (acting as a client) on the single server, which sends it on to all clients
	public void CmdSetWorkers(string stateAbbreviation, int workerCount, bool isBluePlayer) {
		RpcSetWorkers (stateAbbreviation, workerCount, isBluePlayer);
	}

	// Called by the server on every client to set the state workers
	public void RpcSetWorkers(string stateAbbreviation, int workerCount, bool isBluePlayer) {
		DoSetWorkers (stateAbbreviation, workerCount, isBluePlayer);
	}

	void DoSetWorkers(string stateAbbreviation, int workerCount, bool isBluePlayer) {
		State state = GameManager.Instance.StatesByAbbreviation[stateAbbreviation];
		//TODO if (state) state.SetWorkerCount(workerCount, isBluePlayer);
	}
}
