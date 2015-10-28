using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class AI {
//	List<State> states;
	public BudgetController Budget = new BudgetController();

	public void Reset() {
		Budget.Reset();
	}

	public void DoTurn() {
		GameState nextState = GetBestNextState(null, Budget.Amount);
		foreach (GameMove m in nextState.Moves) {
			if (m.Action == GameActions.PLACE_WORKER) {
				m.State.StateView.AddWorker(false);
			} else if (m.Action == GameActions.REMOVE_WORKER) {
				m.State.StateView.RemoveWorker(false);
			}
			Budget.ConsumeAmount(m.Cost);
		}
	}

	public GameState GetBestNextState(GameState gameState, float funds, int depth = 0) {
		if (funds <= 0 || depth > 3) {
			return gameState;
		}

		if (gameState == null) {
			gameState = new GameState();
		} // based on current game

		GameState bestState = gameState;
		float bestValue = 0;
		GameMove bestMove = null;

		// For every state, consider making a move there
		foreach (AiStateModel state in gameState.StateModels.Values) {
			GameMove move = GetMoveForState(state);
			GameState newState = gameState.ApplyOpponentMove(move);

			newState = GetBestNextState(newState, funds - move.Cost, depth + 1); // recursively choose the best moves after this one
			float value = Evaluate(newState); // evaluate the value of this move (plus the best moves after it)

			if (bestMove == null || value > bestValue) {
				bestState = newState;
				bestValue = value;
				bestMove = move;
			}
		}

		return bestState;
	}

	float Evaluate(GameState gs) {
		float value = 0;
		foreach (AiStateModel s in gs.StateModels.Values) {
			Leaning leaning = s.GetLeaning();
			if (leaning == Leaning.Neutral) {
				// no change in value
			} else if (leaning == Leaning.Red ^ !GameManager.Instance.PlayerIsBlue) {
				value += s.StateView.electoralVotes; // if the state is red xor we're blue
			} else {
				value -= s.StateView.electoralVotes;
			}
		}
//		Debug.Log ("Value of "+gs.ToString()+": "+value);
		return value + UnityEngine.Random.value; // some randomness to choose between ties
	}

	public GameMove GetMoveForState(AiStateModel s) {
		GameActions a = GameActions.PLACE_WORKER; // TODO: we need an AI that can remove workers
		return new GameMove(s, a, 0);
	}
}
