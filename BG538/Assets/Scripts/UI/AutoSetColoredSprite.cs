using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AutoSetColoredSprite : MonoBehaviour {
	public bool IsPlayer;
	public Sprite RedSprite;
	public Sprite BlueSprite;
	
	void Start () {
		Refresh ();

		SignalManager.PlayerColorSet += Refresh;
	}

	void OnDestroy() {
		SignalManager.PlayerColorSet -= Refresh;
	}

	void Refresh() {
		bool isRed = (IsPlayer ^ GameManager.Instance.PlayerIsBlue); // if player and player's not blue or not player and player's blue

		Image i = GetComponent<Image> ();
		if (i != null) i.sprite = (isRed) ? RedSprite : BlueSprite;

		SpriteRenderer s = GetComponent<SpriteRenderer> ();
		if (s != null) s.sprite = (isRed) ? RedSprite : BlueSprite;
	}
}
