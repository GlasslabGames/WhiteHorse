using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DisableComponentsOnPause : MonoBehaviour {
	public List<Component> Components;

	void OnEnable () {
    SignalManager.Paused += OnPaused;
    OnPaused(ExplorationUIManager.Instance.Paused);
	}

  void OnDisable () {
    SignalManager.Paused -= OnPaused;
  }

  public void OnPaused(bool paused) {
    foreach (Component c in Components) {
      bool enable = !paused;
      System.Type type = c.GetType();

      // there's no common ancestor that lets us set .enabled for different types of components
      if (type.IsSubclassOf(typeof(Behaviour))) {
        (c as Behaviour).enabled = enable;
      } else if (type.IsSubclassOf(typeof(Collider))) {
        (c as Collider).enabled = enable;
      } else if (type.IsSubclassOf(typeof(Renderer))) {
        (c as Renderer).enabled = enable;
      } else {
        Debug.LogWarning("Can't disable Component with type "+type+" on pause.", this);

      }
      //b.enabled = !paused;
    }
  }
	
}
