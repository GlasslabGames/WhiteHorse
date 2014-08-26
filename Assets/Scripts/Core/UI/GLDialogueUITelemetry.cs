using UnityEngine;
using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.NGUI;


public class GLDialogueUITelemetry : NGUIDialogueUI
{
	override public void ShowSubtitle(Subtitle subtitle) {
		// Write the telemetry event
		PegasusManager.Instance.GLSDK.AddTelemEventValue( "speaker", subtitle.speakerInfo.Name );
		PegasusManager.Instance.GLSDK.AddTelemEventValue( "content", subtitle.formattedText.text );
    PegasusManager.Instance.AppendDefaultTelemetryInfo();
		PegasusManager.Instance.GLSDK.SaveTelemEvent( "Dialogue_display" );

		// Call the base function
		base.ShowSubtitle( subtitle );
	}

	override public void OnContinue() {
		// Write the telemetry event
    PegasusManager.Instance.AppendDefaultTelemetryInfo();
		PegasusManager.Instance.GLSDK.SaveTelemEvent( "Dialogue_advance" );

		// Call the base function
		base.OnContinue();
	}
}