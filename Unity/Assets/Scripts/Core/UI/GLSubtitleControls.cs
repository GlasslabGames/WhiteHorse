using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.NGUI;


public class GLSubtitleControls : NGUISubtitleControls
{
  public override void SetActive (bool value) {
    /*
      // After update, not sure if this is actually useful anymore.
      if (line != null) NGUIDialogueUIControls.SetControlActive(line.gameObject, value);
      if (portraitImage != null) NGUIDialogueUIControls.SetControlActive(portraitImage.gameObject, value);
      if (portraitName != null) NGUIDialogueUIControls.SetControlActive(portraitName.gameObject, value);
      if (continueButton != null) NGUIDialogueUIControls.SetControlActive(continueButton.gameObject, value);
     */
    base.SetActive(value);
  }
}

