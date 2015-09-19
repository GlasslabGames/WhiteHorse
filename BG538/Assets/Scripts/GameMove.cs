using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

// Globally available enum
public enum GameActions {
	PLACE_WORKER,
	REMOVE_WORKER
}

// Represents one action in the game. Used by OpponentAi, could be used for the replay.
public class GameMove {
	public AiStateModel State; // which state they did it in
	public int Round; // when they did it - for use in a replay

	public GameActions Action;

	public int Cost {
		get {
			return GetCost(Action);
		}
	}

	public static int GetCost(GameActions a) {
		switch (a) {
		case GameActions.PLACE_WORKER:
			return 10;
		case GameActions.REMOVE_WORKER:
			return -5; // return half of the price
		default:
			return 0;
		}
	}

	public GameMove(AiStateModel s, GameActions a, int r = 0) {
		State = s;
		Action = a;
		Round = r;
	}

	public override string ToString() {
		return (System.String.Format("GameMove({0}, {1})", State.StateView.Model.Abbreviation, Action.ToString()));
	}
}

// TODO: refactor to have each State have a StateModel instead of this
public class AiStateModel {
	public State StateView;
	public float Vote;
	public int PlayerWorkerCount;
	public int OpponentWorkerCount;

	public AiStateModel(State s) {
		StateView = s;
		PlayerWorkerCount = s.PlayerWorkerCount;
		OpponentWorkerCount = s.OpponentWorkerCount;
		Vote = s.PopularVote;
	}

	public AiStateModel(AiStateModel s) {
		StateView = s.StateView;
		PlayerWorkerCount = s.PlayerWorkerCount;
		OpponentWorkerCount = s.OpponentWorkerCount;
		Vote = s.Vote;
	}

	public Leaning GetLeaning() {
		float redWorkerCount;
		float blueWorkerCount;
		if (!GameManager.Instance.PlayerIsBlue) {
			redWorkerCount = PlayerWorkerCount;
			blueWorkerCount = OpponentWorkerCount;
		} else {
			blueWorkerCount = PlayerWorkerCount;
			redWorkerCount = OpponentWorkerCount;
		}
		var newVote = Vote + (blueWorkerCount - redWorkerCount) * GameSettings.InstanceOrCreate.WorkerIncrement * 2;

		if (newVote < 0) {
			return Leaning.Red;
		} else if (newVote > 0) {
			return Leaning.Blue;
		} else {
			return Leaning.Neutral;
		}
	}
}

// just used for AI (for now)
public class GameState {
	public Dictionary<State, AiStateModel> StateModels = new Dictionary<State, AiStateModel>();
	public List<GameMove> Moves = new List<GameMove>();

	// duplicate a game state
	public GameState(GameState gs) {
		foreach (AiStateModel sm in gs.StateModels.Values) {
			StateModels.Add(sm.StateView, new AiStateModel(sm));
		}
		foreach (GameMove m in gs.Moves) {
			Moves.Add(m);
		}
	}

	// create a new gamestate based on the current one
	public GameState() {
		// only need to track states that are in play
		List<State> states = ObjectAccessor.Instance.StatesContainer.GetComponentsInChildren<State>().Where(s => s.InPlay).ToList();
		foreach (State s in states) {
			StateModels.Add(s, new AiStateModel(s));
		}
	}

	public GameState ApplyOpponentMove(GameMove move) {
		GameState newGameState = new GameState(this);
		AiStateModel s = newGameState.StateModels[move.State.StateView];
		if (move.Action == GameActions.PLACE_WORKER) {
			s.OpponentWorkerCount ++;
		} else if (move.Action == GameActions.REMOVE_WORKER) {
			s.OpponentWorkerCount --;
		}
		newGameState.Moves.Add(move);
		return newGameState;
	}

	public override string ToString() {
		string s = "GameState from moves: ";
		foreach (GameMove m in Moves) {
			s += m.ToString() + ", ";
		}
		return s;
	}
}