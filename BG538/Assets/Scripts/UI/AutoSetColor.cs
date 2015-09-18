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
		RefreshColor();

		SignalManager.PlayerColorSet += RefreshColor;
	}

	void OnDestroy() {
		SignalManager.PlayerColorSet -= RefreshColor;
	}

	void RefreshColor() {
		Color c = GetColorForPlayer(IsPlayer, color);

		Image i = GetComponent<Image> ();
		if (i != null) i.color = c;
		
		Text t = GetComponent<Text>();
		if (t != null) t.color = c;
		
		SpriteRenderer s = GetComponent<SpriteRenderer>();
		if (s != null) s.color = c;
	}
	
	public static Color GetColorForPlayer(bool isPlayer, ColorChoice choice) {
		bool isBlue = (isPlayer ^ !GameManager.Instance.PlayerIsBlue); // if player and player's blue or not player and player's not blue
		return AutoSetColor.GetColor (isBlue, choice);
	}

	public static Color GetColor(bool isBlue, ColorChoice choice) {
		GameColorSettings colors = GameSettings.Instance.Colors;

		switch (choice) {
		case ColorChoice.light: return (isBlue)? colors.lightBlue : colors.lightRed;
		case ColorChoice.med: return (isBlue)? colors.medBlue : colors.medRed;
		case ColorChoice.dark: return (isBlue)? colors.darkBlue : colors.darkRed; 
		case ColorChoice.darker: return (isBlue)? colors.darkerBlue : colors.darkerRed; 
		}
		return Color.white;
	}
}
