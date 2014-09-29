using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class OpponentAi : Player {
  List<State> states;
  Dictionary<State, int> m_predictedBasisCounts = new Dictionary<State, int>();
  public BudgetController Budget;

  public void Start() {
    Budget = GetComponent<BudgetController>();
    states = GameObjectAccessor.Instance.StatesContainer.GetComponentsInChildren<State>().Where( s => s.InPlay ).ToList();
  }

  public void DoTurn() {
    GameState nextState = GetBestNextState(null, Budget.m_amount);
    foreach (GameMove m in nextState.Moves) {
      Debug.Log ("Doing "+m.ToString());
      if (m.Action == GameActions.NEW_SUPPORTER) {
        m.State.StateView.NextOpponentSupporters.Add( 1 );
      } else if (m.Action == GameActions.UPGRADE1) {
        m.State.StateView.OpponentUpgrade(1);
      } else if (m.Action == GameActions.UPGRADE1) {
        m.State.StateView.OpponentUpgrade(2);
      }
      Budget.ConsumeAmount(m.Cost);
    }
  }

  public GameState GetBestNextState(GameState gameState, int funds, int depth = 0) {
    if (funds <= 0 || depth > 3) return gameState;

    if (gameState == null) gameState = new GameState(); // based on current game

    GameState bestState = gameState;
    float bestValue = -999;
    GameMove bestMove = null;

    //float v = Evaluate(gameState);
    //Debug.Log("> "+v+" starting from state "+gameState.ToString()+" with "+funds+" left.");
    foreach (AiStateModel state in gameState.StateModels.Values) {
      GameMove move = GetMoveForState(state);
      GameState newState = gameState.ApplyOpponentMove( move );
      newState = GetBestNextState (newState, funds - move.Cost, depth + 1);
      float value = Evaluate(newState);
      //Debug.Log (value+" to state "+newState.ToString());
      if (value > bestValue) {
        //Debug.Log ("Best move: "+move.ToString()+" to "+gameState.ToString());

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
      int newOppCount = s.OpponentBasisCount + s.OpponentBasisIncrement;
      int newPlayerCount = s.PlayerBasisCount + s.PlayerBasisIncrement;
      if (newOppCount > newPlayerCount) {
        value += s.StateView.Model.ElectoralCount; // 3-20
      } else if (newOppCount < newPlayerCount) {
        value -= s.StateView.Model.ElectoralCount;
      }
    }
    //Debug.Log ("Value of "+gs.ToString()+": "+value);
    return value + UnityEngine.Random.value; // some randomness to choose between ties
  }

  public GameMove GetMoveForState(AiStateModel s) {
    GameActions a;
    // hard coded rules: placing a new worker is best (as long as it's not full), followed by Upgrade 2 (as long as we have someone to upgrade.)
    if (s.OpponentWorkerCounts[0] < s.StateView.UnitCap) a = GameActions.NEW_SUPPORTER;
    else if (s.OpponentWorkerCounts[1] > 0) a = GameActions.UPGRADE2;
    else a = GameActions.UPGRADE1;
    return new GameMove(this, s, a, 0);
  }

  List<GameMove> GetNextMoves() {
    List<GameMove> bestSequence = new List<GameMove>();
    float bestValue; // = num votes / cost

    // try to take over a good state.
    foreach (State state in states) {
      bool controlled = state.OpponentBasisCount > state.PlayerBasisCount;
      if (!controlled) {

      }
    }

    return bestSequence;
  }
}
