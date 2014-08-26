////////////////////////////////////////////////////////////////////////////////
//  
// @module Flash Like Event System
// @author Osipov Stanislav lacost.20@gmail.com
//
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public class Button : EventDispatcher {

	public static Button instance = null;
	
	public float w = 150;
	public float h = 50;
	
	void Awake() {
		instance = this;
	}
	
	void OnGUI() {
		Rect buttonRect =  new Rect((Screen.width - w) / 2, (Screen.height - h) / 2, w, h);
		if(GUI.Button(buttonRect, "click me")) {
			dispatch(BaseEvent.CLICK, "hello");
		}
	}
	
}
