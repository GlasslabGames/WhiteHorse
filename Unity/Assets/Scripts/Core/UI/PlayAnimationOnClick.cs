using UnityEngine;
using System.Collections;

public class PlayAnimationOnClick : OnClickHandler {
	protected Animator m_animator;

	void Awake() {
		m_animator = GetComponent<Animator>();
	}

  protected override void OnMouseClick() {
    if (m_animator != null) {
      m_animator.SetTrigger("CLICK");
    }

    // This should really not be in this file since some things (e.g. evidence) is not an exploration object
    if (SignalManager.ObjectTapped != null) SignalManager.ObjectTapped(gameObject);
  }
}
