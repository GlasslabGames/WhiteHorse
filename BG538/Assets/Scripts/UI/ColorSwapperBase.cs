using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ColorSwapperBase : MonoBehaviour {
	public enum ColorSource {
		none,
		player,
		opponent
	}
	public ColorSource AutoSet;

	void Start() {
		if (AutoSet != ColorSource.none) {
			SetColorBySource();
			SignalManager.PlayerColorSet += SetColorBySource;
		}
	}

	void OnDestroy() {
		SignalManager.PlayerColorSet -= SetColorBySource;
	}

	void SetColorBySource() {
		bool isBlue = (AutoSet == ColorSource.player ^ !GameManager.Instance.PlayerIsBlue);
		SetColor ((isBlue) ? Leaning.Blue : Leaning.Red);
	}

	public virtual void SetColor(Leaning l) { }

	[ContextMenu("Set blue")]
	public void SetBlue() {
		SetColor(Leaning.Blue);
	}

	[ContextMenu("Set red")]
	public void SetRed() {
		SetColor(Leaning.Red);
	}
}