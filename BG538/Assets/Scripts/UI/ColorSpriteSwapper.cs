using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ColorSpriteSwapper : ColorSwapperBase {
	public Sprite RedSprite;
	public Sprite BlueSprite;

	public override void SetColor(Leaning l) {
		Sprite sprite = (l == Leaning.Blue)? BlueSprite : RedSprite;

		Image i = GetComponent<Image> ();
		if (i != null) i.sprite = sprite;

		SpriteRenderer s = GetComponent<SpriteRenderer> ();
		if (s != null) s.sprite = sprite;
	}
}
