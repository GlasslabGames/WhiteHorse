using UnityEngine;
using System.Collections;

public class ToggleGameObjectsByScene : MonoBehaviour {

	public string targetSceneName;
	public GameObject[] showInTargetScene;
	public GameObject[] hideInTargetScene;

	void OnEnable() {
		bool inTargetScene = (targetSceneName == Application.loadedLevelName);

		for (var i = 0; i < showInTargetScene.Length; i++) {
			showInTargetScene[i].SetActive( inTargetScene );
		}

		for (var i = 0; i < hideInTargetScene.Length; i++) {
			hideInTargetScene[i].SetActive( !inTargetScene );
		}
	}
}
