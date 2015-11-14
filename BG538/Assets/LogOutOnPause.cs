using UnityEngine;
using System.Collections;

public class LogOutOnPause : MonoBehaviour {

	private float pauseTime;

	void OnApplicationPause( bool pause ) {
		if (pause) {
			pauseTime = Time.realtimeSinceStartup;
		} else {
			float timePassed = Time.realtimeSinceStartup - pauseTime;
			if (timePassed > 30) {
				SdkManager.Instance.Logout();
				if (Application.loadedLevelName != "title") Application.LoadLevel("title");
			}
		}
	}
}
