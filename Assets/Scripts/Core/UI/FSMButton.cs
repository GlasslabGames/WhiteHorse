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
    if (Fabric.EventManager.Instance != null) {
    	Fabric.EventManager.Instance.PostEvent(SoundEventName);
    }

		// If this button is supposed to use custom information, send it along
		/*if( m_telemetryUseCustomInfo ) {
			// Write Telemetry Data
			PegasusManager.Instance.GLSDK.SaveTelemEvent( m_telemetryEventName );
		}
		// Otherwise, use generic
		else {
			// Write Telemetry Data
			PegasusManager.Instance.GLSDK.AddTelemEventValue( "name", name );
			PegasusManager.Instance.GLSDK.SaveTelemEvent( "Button_press" );
		}*/
      
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
