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

    // also send the click through to the tap indicator so that it can respond
    if (BackgroundTapCatcher.Instance != null) BackgroundTapCatcher.Instance.ClickThrough();

    // This should really not be in this file since some things (e.g. evidence) is not an exploration object
    if (SignalManager.ExplorationObjectTapped != null) SignalManager.ExplorationObjectTapped(gameObject);
  }
}
