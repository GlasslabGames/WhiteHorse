using UnityEngine;
using System.Collections;

public class HeaderBackground : MonoBehaviour {
	public UITexture m_opponentImage;
	public UITexture m_divider;
	public float defaultFillAmount;

	public void TweenImage(bool color1Win = true, EventDelegate callback = null) {
		m_divider.gameObject.SetActive(false);

		float target = color1Win? 0 : 1;
		float duration = 0.5f * (target == 0? 1 : 4); // tween is about 4x as long when the target is 0
		TweenFillAmount t = TweenFillAmount.Begin(m_opponentImage.gameObject, duration, target);
		t.method = TweenFillAmount.Method.EaseIn;
		EventDelegate.Add(t.onFinished, callback);
	}

	public void Reset() {
		m_divider.gameObject.SetActive(true);
		m_opponentImage.fillAmount = defaultFillAmount;
	}
}
