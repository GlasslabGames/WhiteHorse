using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PercentMeter : MonoBehaviour {
	public Image fill;
	public float percentIncrementPerSec;

	private bool animating = false;
	private float targetFillAmount;
	
	public void Set (float amount, bool animate = true) {
		if (animate) {
			animating = true;
			targetFillAmount = amount;
		} else {
			fill.fillAmount = amount;
		}
	}
	
	void Update() {
		if (animating) {
			if (fill.fillAmount < targetFillAmount - percentIncrementPerSec) {
				fill.fillAmount += percentIncrementPerSec;
			} else if (fill.fillAmount > targetFillAmount + percentIncrementPerSec) {
				fill.fillAmount -= percentIncrementPerSec;
			} else {
				fill.fillAmount = targetFillAmount;
				animating = false;
			}
		}
	}
}
