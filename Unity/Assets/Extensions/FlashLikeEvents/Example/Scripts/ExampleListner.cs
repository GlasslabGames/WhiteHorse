////////////////////////////////////////////////////////////////////////////////
//  
// @module Flash Like Event System
// @author Osipov Stanislav lacost.20@gmail.com
//
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public class ExampleListner : MonoBehaviour {
	
	public GUIStyle style;
	
	private string label = "Click's: ";
	private int count = 0;
	

	void Start () {
		Button.instance.addEventListener(BaseEvent.CLICK, onButtonClick);
		Button.instance.addEventListener(BaseEvent.CLICK, onButtonClickData);
	}
	
	
	private void onButtonClick() {
		count++;
	}
	
	private void onButtonClickData(CEvent e) {
		Debug.Log("================================");
		Debug.Log("onButtonClickData");
			
		Debug.Log("dispatcher: " + e.dispatcher.ToString());
		Debug.Log("event data: " + e.data.ToString());
		Debug.Log("event name: " + e.name.ToString());
		Debug.Log("================================");
	}
	
	void OnGUI() {
		GUI.Label(new Rect(0, 0, 200, 200), label + count.ToString(), style);
	}
	
}
