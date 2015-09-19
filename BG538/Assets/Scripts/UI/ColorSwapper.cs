using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class ColorSwapper : ColorSwapperBase {
	public enum ColorChoice
	{
		light,
		med,
		dark,
		darker
	}
	public ColorChoice color;
	public float TweenDuration;

	public override void SetColor(Leaning l) {
		Color c = GetColor(l == Leaning.Blue, color);
		SetColor(c);
	}

	void SetColor(Color c) {
		Image i = GetComponent<Image> ();
		if (i != null) {
			if (TweenDuration > 0) i.DOColor(c, TweenDuration);
			else i.color = c;
		}
		
		Text t = GetComponent<Text>();
		if (t != null) {
			if (TweenDuration > 0) t.DOColor(c, TweenDuration);
			else t.color = c;
		}
		
		SpriteRenderer s = GetComponent<SpriteRenderer>();
		if (s != null) {
			if (TweenDuration > 0) s.DOColor(c, TweenDuration);
			else s.color = c;
		}
	}

	public static Color GetColor(bool isBlue, ColorChoice choice) {
		GameColorSettings colors = GameSettings.InstanceOrCreate.Colors;

		switch (choice) {
		case ColorChoice.light: return (isBlue)? colors.lightBlue : colors.lightRed;
		case ColorChoice.med: return (isBlue)? colors.medBlue : colors.medRed;
		case ColorChoice.dark: return (isBlue)? colors.darkBlue : colors.darkRed; 
		case ColorChoice.darker: return (isBlue)? colors.darkerBlue : colors.darkerRed; 
		}
		return Color.white;
	}
}
