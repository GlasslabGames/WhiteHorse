using UnityEngine;
using System.Collections;

/// <summary>
/// Add this to a button to write Pegasus data whenever the button is pressed.
/// </summary>
public class PegasusRecordClick : MonoBehaviour {
	
	public string m_identifier;
	public string m_value;
	public bool m_useCustomTelemetryEvent;
	public string m_customTelemetryEventName;

	public void OnPress( bool down ) {
		if( down ) {
			// Write the telemetry data
			if( m_useCustomTelemetryEvent ) {
				if (m_value.Length > 0) {
					PegasusManager.Instance.GLSDK.AddTelemEventValue( "name", m_value );
				}
        PegasusManager.Instance.AppendDefaultTelemetryInfo();
				PegasusManager.Instance.GLSDK.SaveTelemEvent( m_customTelemetryEventName );
			}
			else {
				if (m_value.Length > 0) {
					PegasusManager.Instance.GLSDK.AddTelemEventValue( "name", m_value );
				}
				PegasusManager.Instance.GLSDK.SaveTelemEvent( "Menu_item_" + m_identifier );
			}
		}
	}
}
