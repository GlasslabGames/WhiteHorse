using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
/// <summary>
/// Allows you to register outside callbacks for animation events. Then set the animation events to call OnEvent(int).
/// </summary>
public class AnimationEventHandler : MonoBehaviour {
  private Dictionary<int, Action> m_callbacks = new Dictionary<int, Action>();
  public Animator Animator {get; private set; }

  void Awake() {
    Animator = GetComponentInChildren<Animator>();
  }

  public void SetCallback(int eventId, Action action) {
    m_callbacks[eventId] = action;
  }

  void OnEvent(int eventId) {
    //Debug.Log ("AnimationEventHandler processing event " + eventId);
    if (m_callbacks.ContainsKey(eventId)) {
      if (m_callbacks[eventId] != null) {
        m_callbacks[eventId]();
      }
    }
  }
}
