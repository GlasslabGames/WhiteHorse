using UnityEngine;
using System.Collections;

public class ParallaxMovement : MonoBehaviour {
	public Transform Target;
	public float xFactor;
	public float yFactor;
	
	// Update is called once per frame
	void Update () {
		Vector3 pos = Target.transform.position;
		pos.x *= xFactor;
		pos.y *= yFactor;
		pos.z = transform.position.z; // keep the same z
		transform.position = pos;
	}
}
