using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayTweensOnEnable : MonoBehaviour {
  public List<UITweener> Tweens;

  void OnStart() {}

	void OnEnable() {

    foreach (UITweener tween in Tweens) {
      tween.ResetToBeginning();
      tween.PlayForward();
    }
  }
}
