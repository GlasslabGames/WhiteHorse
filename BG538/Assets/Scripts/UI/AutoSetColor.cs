using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AutoSetColor : MonoBehaviour {
	public bool IsPlayer;
	public enum ColorChoice
	{
		light,
		med,
		dark,
		darker
	}
	public ColorChoice color;
	
	void Start () {
		Color c = GetColor(IsPlayer, color);
		
		Image i = GetComponent<Image> ();
		if (i != null) i.color = c;
		
		Text t = GetComponent<Text>();
		if (t != null) t.color = c;

		SpriteRenderer s = GetComponent<SpriteRenderer>();
		if (s != null) s.color = c;
	}
	
	public static Color GetColor(bool isPlayer, ColorChoice choice) {
		GameColorSettings colors = GameSettings.Instance.Colors;
		bool isRed = (isPlayer ^ GameManager.Instance.PlayerIsBlue); // if player and player's not blue or not player and player's blue

		switch (choice) {
		case ColorChoice.light: return (isRed)? colors.lightRed : colors.lightBlue;
		case ColorChoice.med: return (isRed)? colors.medRed : colors.medBlue;
		case ColorChoice.dark: return (isRed)? colors.darkRed : colors.darkBlue;
		case ColorChoice.darker: return (isRed)? colors.darkerRed : colors.darkerBlue;
		}
		return Color.white;
	}
}
