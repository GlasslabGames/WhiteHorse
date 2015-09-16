using UnityEngine;
using System.Collections;

public class SceneChanger : MonoBehaviour {
	public string SceneName;

	public void ChangeScene() {
		Application.LoadLevel(SceneName); 
	}
}
