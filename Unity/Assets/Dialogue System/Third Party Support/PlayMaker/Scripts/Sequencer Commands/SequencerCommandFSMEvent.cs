using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;

/// <summary>
/// Implements sequencer command FSMEvent(event, [subject, [fsm]]).
/// </summary>
public class SequencerCommandFSMEvent : SequencerCommand {

	public void Start() {
		string eventName = GetParameter(0);
		Transform subject = GetSubject(1, Sequencer.Speaker);
		string fsmName = GetParameter(2);
		if (string.IsNullOrEmpty(eventName)) {
			if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: FSMEvent(): event name is empty", DialogueDebug.Prefix));
		} else if (subject == null) {
			if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: FSMEvent({1}, {2}, {3}): subject is null", DialogueDebug.Prefix, eventName, GetParameter(1), fsmName));
		} else {
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: FSMEvent({1}, {2}, {3}) sending event to FSM(s)", DialogueDebug.Prefix, eventName, subject.name, fsmName));
			foreach (var fsm in subject.GetComponents<PlayMakerFSM>()) {
				if (string.IsNullOrEmpty(fsmName) || string.Equals(fsmName, fsm.FsmName)) {
					fsm.SendEvent(eventName);
				}
			}
		}
		Stop();
	}
	
}
