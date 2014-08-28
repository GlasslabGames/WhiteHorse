using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// The most basic button functionality. When clicked, it calls the functions on the list.
/// </summary>
public class GLButton : MonoBehaviour
{
  private static GLButton PRESSED_BUTTON; // Button that received the Down event
	public List<EventDelegate> onClick = new List<EventDelegate>();
  public bool debug;
  public bool UseDragProof = false;

  void Start() {}

  public void Click (bool isDragProofClick = false)
  {
    if (isDragProofClick ^ UseDragProof) return;
    if (debug) Debug.Log("Clicked on "+name, this);
    if (enabled)EventDelegate.Execute(onClick);
  }

  void OnPress(bool isDown)
  {
    if (isDown)
    {
      PRESSED_BUTTON = this;
      
    }
    else
    {
      if (PRESSED_BUTTON == this)
      {
        if (UICamera.lastHit.collider == collider)
        {
          Click();
        }
      }

      PRESSED_BUTTON = null;
    }
  }

  void OnClick()
  {
    Click (true);
  }

  void OnMouseUpAsButton () // non-NGUI
  {
    Click ();
  }
}
