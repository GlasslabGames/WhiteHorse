using UnityEngine;
using System;

public delegate void StringEvent(string s);
public delegate void IntEvent(int n);
public delegate void BoolEvent(bool b);

public delegate void TurnPhaseEvent(TurnPhase t);
public delegate void BudgetEvent(BudgetController b, float amount);

public static class SignalManager {
	public static TurnPhaseEvent EnterTurnPhase;
	public static TurnPhaseEvent ExitTurnPhase;
	public static IntEvent BeginWeek;
	public static BudgetEvent BudgetChanged;
	// public static Action foo;
}