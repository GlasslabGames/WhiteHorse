using UnityEngine;
using System;
using System.Collections.Generic;

public enum TimerType
{
	OneShot,
	Continuous
}

public class Timer : MonoBehaviour
{
	public float Duration;
	public float CurrentTime { get; private set; }
	public bool Active { get; private set; }
	public TimerType type = TimerType.Continuous;
	private Action callback;

	public float PercentageComplete {
		get {
			if (Duration > 0) {
				return CurrentTime / Duration;
			} else {
				return 1;
			}
		}
	}

	public void TimerCompleteCallback() {}

	public void StartTimer(Action callback = null) {
		CurrentTime = 0.0f;
		Active = true;

		callback = (callback == null)? TimerCompleteCallback : callback;
	}

	public void StopTimer(bool triggerCallback = false) {
		Active = false;
		if (triggerCallback) callback ();
	}

	public void Restart (bool triggerCallback = false)
	{
		StopTimer(triggerCallback);
		StartTimer(callback);
	}

	public void Update ()
	{
		if (Active) {
			CurrentTime += Time.deltaTime;
			if (CurrentTime >= Duration) {
				if (type == TimerType.OneShot) {
					StopTimer(true);
				} else if (type == TimerType.Continuous) {
					Restart(true);
				}
			}
		}
	}
}