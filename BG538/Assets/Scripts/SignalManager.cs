using UnityEngine;
using System;

public delegate void StringEvent(string s);
public delegate void IntEvent(int n);
public delegate void FloatEvent(float f);
public delegate void BoolEvent(bool b);

public delegate void TurnPhaseEvent(TurnPhase t);
public delegate void BudgetEvent(BudgetController b, float amount);
public delegate void VoteEvent(int votes, bool isUpdate);
public delegate void LeaningEvent(Leaning color);

public static class SignalManager {
	public static TurnPhaseEvent EnterTurnPhase;
	public static TurnPhaseEvent ExitTurnPhase;
	public static IntEvent BeginWeek;
	public static BudgetEvent BudgetChanged;
	public static VoteEvent PlayerVotesChanged;
	public static VoteEvent OpponentVotesChanged;
	public static Action PlayerColorSet;
	public static LeaningEvent PlayerFinished;
	public static Action TryingPhotonConnect;
	public static Action RoomGroupChanged;
	// public static Action foo;
}