using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayTweensOnEnable : MonoBehaviour {
  public List<UITweener> Tweens;

  void OnStart() {}

  // This doesn't really work the first time that the tweens are supposed to be played.
	void OnEnable() {
    foreach (UITweener tween in Tweens) {
      tween.ResetToBeginning();
      Utility.NextFrame ( tween.PlayForward ); // delay because doing it right away wasn't working.
    }
  }

  // Basically tweens are hard to use, and this script is definitely not perfect. But it does its job most of the time.
}
