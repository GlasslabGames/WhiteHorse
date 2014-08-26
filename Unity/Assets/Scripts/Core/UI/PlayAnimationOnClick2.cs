using UnityEngine;
using System.Collections;

// Fixed a few things for use with evidence - mainly we don't want to use OnClickHandler, but I don't want to mess up exploration objects
public class PlayAnimationOnClick2 : MonoBehaviour {
	protected Animator m_animator;

	void Awake() {
		m_animator = GetComponent<Animator>();
	}

  public void Play() {
    if (m_animator != null) {
      m_animator.SetTrigger("CLICK");
    }

    // also send the click through to the tap indicator so that it can respond
    if (BackgroundTapCatcher.Instance != null) BackgroundTapCatcher.Instance.ClickThrough();
  }
}
