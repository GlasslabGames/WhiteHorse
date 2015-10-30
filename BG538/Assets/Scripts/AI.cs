using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class AI: MonoBehaviour {
//	List<State> states;
	public BudgetController Budget = new BudgetController();

	// Used for planning our turn
	List<GameMove> possibleMoves = new List<GameMove>();
	int availableWorkers;

	private class GameMove {
		public string stateKey;
		public int workerCount;
		public float value;

		public GameMove(string stateKey, int workerCount, float value) {
			this.stateKey = stateKey;
			this.workerCount = workerCount;
			this.value = value;
		}

		public override string ToString()
		{
			return "[" + stateKey + "+" + workerCount + ": " + value + "]";
		}
	}

	public void Reset() {
		Budget.Reset();
	}

	public void StartTurn(List<State> states) {
		StartCoroutine(ChooseMoves(states));
	}

	IEnumerator ChooseMoves(List<State> states) {
		possibleMoves.Clear();

		// Figure out how many workers we can place
		float workerCost = GameSettings.InstanceOrCreate.GetGameActionCost(GameAction.PlaceWorker);
		availableWorkers = Mathf.FloorToInt(Budget.Amount / workerCost);
		float workerIncrement = GameSettings.InstanceOrCreate.WorkerIncrement * 2;
		// When we're working with the -1 to 1 vote number, the worker increment has to doubled

		// Add actions for each state
		foreach (State state in states) {
			if (!state.InPlay) continue;
			float vote = state.CalculateVote(); // the predicted vote at the end of the next harvest
			State.Controller controller = State.GetController(vote);
			if (controller == State.Controller.Neutral) {
				// If it's neutral, it will only take one worker to capture it
				TryAddMove(state, 1, 1);
			} else if (controller == State.Controller.Player) {
				vote = Math.Abs (vote); // between 0 and 1 (total control by player)

				// Calculate how many workers we'd have to place to bring the 
				int workersNeeded = Mathf.CeilToInt( vote / workerIncrement );
				float predictedVote = vote - (workersNeeded * workerIncrement); // we're negative compared to the player
				predictedVote = Mathf.Round (predictedVote * 1000) / 1000; // avoid rounding errors

				//Debug.Log ("Current: "+vote+" workers: "+workersNeeded+" result: "+predictedVote);
				if (predictedVote < 0) { // we can capture the state with just this many workers
					TryAddMove(state, 2, workersNeeded);
				} else {
					TryAddMove(state, 1, workersNeeded); // bringing state to neutral
					TryAddMove(state, 2, workersNeeded + 1); // capturing state
				}
			}
			yield return null;
		}

		// Sort the possible moves by value, with moves that use more workers prioritized
		possibleMoves = possibleMoves.OrderByDescending(m => m.value)
			.ThenByDescending(m => m.workerCount).ToList();

		// Do as many of the best moves that we can
		Dictionary<string, State> stateDict = GameManager.Instance.StatesByAbbreviation;
		GameMove move;
		for (var i = 0; i < possibleMoves.Count; i++) {
			move = possibleMoves[i];
			if (availableWorkers >= move.workerCount) {
				// DO IT
				//Debug.Log ("Doing "+move);
				for (var j = 0; j < move.workerCount; j++) {
					availableWorkers --;
					stateDict[move.stateKey].AddWorker(false);
					Budget.ConsumeAmount(workerCost);
				}
			}
		}

		yield return null;

		// Now if we still have available workers, put them on the states that belong to the player but are closest to neutral
		// Put the states we don't control above the ones we do control, then look at the ones closest to switching
		if (availableWorkers > 0) {
			states = states.Where(s => s.InPlay)
				.OrderBy(s => (s.GetPredictedController() == State.Controller.Opponent))
				.ThenBy( s => Mathf.Abs(s.PopularVote) )
				.ThenByDescending( s => s.electoralVotes ).ToList();

			//Debug.Log ("Backup state: "+states.First());
			// Add all workers to the state that was sorted to the top
			while (availableWorkers > 0) {
				availableWorkers --;
				states.First().AddWorker(false);
				Budget.ConsumeAmount(workerCost);
			}
		}

		// Done!
		GameManager.Instance.SetAIFinished();
	}

	void TryAddMove(State state, int factor, int workerCount) {
		if (availableWorkers >= workerCount) {
			float value = state.electoralVotes * factor / workerCount;
			GameMove move = new GameMove(state.Model.Abbreviation, workerCount, value);
			possibleMoves.Add(move);
			// Debug.Log ("Adding move "+move);
		}
	}
}
