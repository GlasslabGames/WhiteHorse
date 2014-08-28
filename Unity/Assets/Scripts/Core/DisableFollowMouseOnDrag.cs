using UnityEngine;
using System.Collections;

public class DisableFollowMouseOnDrag : MonoBehaviour {
  FollowMouseWithinBounds m_follower;

	// Use this for initialization
	void Start () {
    GameObject target = GameObject.FindGameObjectWithTag("CameraFollowTarget");
    if (target != null) m_follower = target.GetComponent<FollowMouseWithinBounds>();
	}

  void OnPress(bool pressed) {
    if (m_follower != null) m_follower.enabled = !pressed;
  }
}
