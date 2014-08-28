using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// When the mouse is up or down, it calls the functions on the list. Convenience component.
/// </summary>
public class GLMouseEventHandler : MonoBehaviour
{
  public List<EventDelegate> onMouseDown = new List<EventDelegate>();
	public List<EventDelegate> onMouseUp = new List<EventDelegate>();
  public bool debug;

  void Start() {}

  public void MouseDown ()
  {
    if (debug) Debug.Log("Mouse down on "+name, this);
    if (enabled) EventDelegate.Execute(onMouseDown);
  }

  public void MouseUp ()
  {
    if (debug) Debug.Log("Mouse up on "+name, this);
    if (enabled) EventDelegate.Execute(onMouseUp);
  }

	void OnPress (bool down) // NGUI
	{
    if (down) MouseDown ();
    else MouseUp ();
	}

  void OnMouseDown () // non-NGUI
  {
    MouseDown ();
  }

  void OnMouseUp () // non-NGUI
  {
    MouseUp ();
  }
}
