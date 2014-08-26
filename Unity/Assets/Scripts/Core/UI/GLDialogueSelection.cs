using UnityEngine;
using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem.NGUI;


public class GLDialogueSelection : MonoBehaviour {

  public void LogSelection() {
    string selection = gameObject.GetComponent<NGUIResponseButton>().nguiLabel.text;

    // Write Telemetry Data
    PegasusManager.Instance.GLSDK.AddTelemEventValue( "option", selection );
    PegasusManager.Instance.AppendDefaultTelemetryInfo();
    PegasusManager.Instance.GLSDK.SaveTelemEvent( "Dialogue_select_option" );
  }
}