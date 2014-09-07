using UnityEngine;
using System.Collections;

public class AutoSetColor : MonoBehaviour {
	public bool IsPlayer;
	public enum ColorChoice
	{
		LIGHT,
		DARK,
		DARKER
	}
	public ColorChoice color;

	void Start () {
		UITexture t = GetComponent<UITexture> ();
		t.color = GetColor (IsPlayer, color);
	}

	public static Color GetColor(bool isPlayer, ColorChoice choice) {
		GameColorSettings colors = GameObjectAccessor.Instance.GameColorSettings;
		// if player and player's red (red) or not player and player's blue (red)
		if (isPlayer ^ GameObjectAccessor.Instance.Player.m_leaning == Leaning.Blue) {
			switch (choice) {
			case ColorChoice.LIGHT: return colors.redState;
			case ColorChoice.DARK: return colors.redStateDark;
			case ColorChoice.DARKER: return colors.redDarker;
			}
		} else { // blue
			switch (choice) {
			case ColorChoice.LIGHT: return colors.blueState;
			case ColorChoice.DARK: return colors.blueStateDark;
			case ColorChoice.DARKER: return colors.blueDarker;
			}
		}
		return Color.white;
	}
}
