using UnityEngine;
using System.Collections;
using System;

public class BackgroundTapCatcher : OnClickHandler {
  public event Action BackgroundTapped;
  public bool AlwaysShow = true;
	public Animator Effect;

  public static BackgroundTapCatcher Instance;

  BackgroundTapCatcher()
  {
    Instance = this;
  }

  void Awake()
  {
    UICamera.fallThrough = this.gameObject;

    SignalManager.ObjectTapped += onObjectClicked;
  }

	protected override void OnMouseClick() {
    GlSoundManager.Instance.PlaySoundByEvent("tap_nothing");
    playAnimation();
	}

  private void playAnimation()
  {
    if (Effect != null) {
      Vector3 pos = UICamera.currentCamera.ScreenToWorldPoint( Input.mousePosition );
      pos.z = 0;
      Effect.transform.position = pos;
      Effect.Play("tapFeedback", -1, 0);
    
      if (BackgroundTapped != null) BackgroundTapped();
    }
  }

  void OnDestroy()
  {
    SignalManager.ObjectTapped -= onObjectClicked;
  }

  private void onObjectClicked(GameObject go) {
    playAnimation();
  }
}
