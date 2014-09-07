using UnityEngine;
using System.Collections;

public class AutoSwitchColor : MonoBehaviour {
	public bool IsPlayer;
	public Leaning CurrentColor;
	public Texture OtherTexture;

	// Use this for initialization
	void Start () {
		// if this is player but the player isn't this color, or this is opponent but the player IS this color
		if (IsPlayer ^ GameObjectAccessor.Instance.Player.m_leaning == CurrentColor) {
			GetComponent<UITexture>().mainTexture = OtherTexture;
		}
	}
}
