using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

// Globally available enum
public enum GameActions {NEW_SUPPORTER, UPGRADE1, UPGRADE2 }

// Represents one action in the game. Used by OpponentAi, could be used for the replay.
public class GameMove {
	public Player Player; // who made the move
	public StateModel State; // which state they did it in
	public int Round; // when they did it - for use in a replay

  public GameActions Action;

	public int Cost {
		get {
			return GetCost(Action);
		}
	}

  public static int GetCost(GameActions a) {
		switch (a) {
    case GameActions.NEW_SUPPORTER: return 10;
    case GameActions.UPGRADE1: return 15;
    case GameActions.UPGRADE2: return 20;
		default: return 0;
		}
	}

  public GameMove(Player p, StateModel s, GameActions a, int r = 0) {
    Player = p;
    State = s;
    Action = a;
    Round = r;
  }

  public override string ToString() {
    return (System.String.Format("GameMove({0}, {1}, {2})", Player.ToString(), State.StateView.m_name, Action.ToString()));
  }
}

// TODO: refactor to have each State have a StateModel instead of this
public class StateModel {
  public State StateView;
  public int PlayerBasisCount;
  public int PlayerBasisIncrement;
  public int OpponentBasisCount;
  public int OpponentBasisIncrement;
  public int[] PlayerWorkerCounts;
  public int[] OpponentWorkerCounts;

  public StateModel(State s) {
    StateView = s;
    PlayerBasisCount = s.PlayerBasisCount;
    OpponentBasisCount = s.OpponentBasisCount;
    PlayerBasisIncrement = s.PlayerBasisCountIncrement;
    OpponentBasisIncrement = s.OpponentBasisCountIncrement;
    PlayerWorkerCounts = s.PlayerCampaignWorkerCounts;
    OpponentWorkerCounts = s.OpponentCampaignWorkerCounts;
  }

  public StateModel(StateModel s) {
    StateView = s.StateView;
    PlayerBasisCount = s.PlayerBasisCount;
    OpponentBasisCount = s.OpponentBasisCount;
    PlayerBasisIncrement = s.PlayerBasisIncrement;
    OpponentBasisIncrement = s.OpponentBasisIncrement;
    PlayerWorkerCounts = s.PlayerWorkerCounts;
    OpponentWorkerCounts = s.OpponentWorkerCounts;
  }
}

// just used for AI (for now)
public class GameState {
  public Dictionary<State, StateModel> StateModels = new Dictionary<State, StateModel>();
  public List<GameMove> Moves = new List<GameMove>();

  // duplicate a game state
  public GameState(GameState gs) {
    foreach (StateModel sm in gs.StateModels.Values) {
      StateModels.Add(sm.StateView, new StateModel(sm));
    }
    foreach (GameMove m in gs.Moves) {
      Moves.Add(m);
    }
  }

  // create a new gamestate based on the current one
  public GameState() {
    // only need to track states that are in play
    List<State> states = GameObjectAccessor.Instance.StatesContainer.GetComponentsInChildren<State>().Where( s => s.m_inPlay ).ToList();
    foreach (State s in states) {
      StateModels.Add(s, new StateModel(s));
    }
  }

  public GameState ApplyOpponentMove(GameMove move) {
    GameState newGameState = new GameState(this);
    StateModel s = newGameState.StateModels[move.State.StateView];
    if (move.Action == GameActions.NEW_SUPPORTER) {
      s.OpponentBasisIncrement += 1;
    } else if (move.Action == GameActions.UPGRADE1) {
      s.OpponentBasisIncrement += 1;
    } else if (move.Action == GameActions.UPGRADE2) {
      s.OpponentBasisIncrement += 2;
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