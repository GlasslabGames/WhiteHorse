using UnityEngine;
using System.Collections;

public class QuitButton : MonoBehaviour {

	public void Quit() {
    Debug.Log ("Quitting app. (This doesn't do anything in the editor.)");
    Application.Quit();
  }
}
