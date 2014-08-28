using UnityEngine;

/// <summary>
/// A button that sends an event to the given FSM (and/or calls a given callback) when clicked.
/// </summary>
public class FSMButton : CallbackButton {

  // Call this event on this fsm when the button is pressed
  public PlayMakerFSM m_fsm;
  public string m_eventName;

	// Telemetry information
	public bool m_telemetryUseCustomInfo;
	public string m_telemetryEventName;

  public string SoundEventName = "ButtonTap";

  void OnClick() {
#if FABRIC
    if (Fabric.EventManager.Instance != null) {
    	Fabric.EventManager.Instance.PostEvent(SoundEventName);
    }
#endif
      
    if (Callback != null) {
      Callback(this);
    }

    if (m_fsm != null) {
      m_fsm.Fsm.Event(m_eventName);
    }
    else
    {
      PlayMakerFSM.BroadcastEvent(m_eventName);
    }
  }
}
