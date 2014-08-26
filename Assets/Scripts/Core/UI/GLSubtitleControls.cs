using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.NGUI;


public class GLSubtitleControls : NGUISubtitleControls
{
  public override void SetControlsActive (bool value) {
    if (line != null) SetControlActive(line.gameObject, value);
    if (portraitImage != null) SetControlActive(portraitImage.gameObject, value);
    if (portraitName != null) SetControlActive(portraitName.gameObject, value);
    if (continueButton != null) SetControlActive(continueButton.gameObject, value);
    base.SetControlsActive(value);
  }
}

