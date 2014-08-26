using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
/// <summary>
/// A simple button component that can be assigned a callback, and calls that method when it's pressed.
/// </summary>
public class CallbackButton : MonoBehaviour {

  public delegate void CallbackDelegate (CallbackButton button);
  public CallbackDelegate Callback { get; set; }

  public void Trigger()
  {
    OnPress (false);
  }

  void OnPress(bool pressed) {
    if (!pressed) {
      if (Callback != null) {
        Callback(this);
      }
    }
  }

}
