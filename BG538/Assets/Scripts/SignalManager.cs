using UnityEngine;
using System;

public delegate void StringEvent(string s);
public delegate void IntEvent(int n);
public delegate void BoolEvent(bool b);

public delegate void TurnPhaseEvent(TurnPhase t);

public static class SignalManager /*: SingletonBehavior<SignalManager>*/ {
	public static TurnPhaseEvent EnterTurnPhase;
	public static TurnPhaseEvent ExitTurnPhase;

	// public static Action foo;
}