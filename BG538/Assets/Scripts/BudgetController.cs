using UnityEngine;
using System;
using System.Collections.Generic;

public class BudgetController {

	public float Amount { get; private set; }
	
	public bool IsAmountAvailable(float amount) {
		return (Amount >= amount);
	}
	
	public void ConsumeAmount(float amount) {
		SetAmount( Amount - amount );
	}
	
	public void GainAmount(float amount) {
		SetAmount( Amount + amount );
	}

	public void SetAmount(float amount) {
		Amount = amount;
		if (SignalManager.BudgetChanged != null) SignalManager.BudgetChanged(this, Amount);
	}
		
	public void Reset() {
		SetAmount(0f);
	}
}