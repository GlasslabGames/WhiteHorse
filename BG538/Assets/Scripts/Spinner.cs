using UnityEngine;
using DG.Tweening;

public class Spinner : MonoBehaviour {
	public bool Clockwise = true;
	public float RotationDuration = 1;

	void Start () {
		float val = ((Clockwise)? -1 : 1) * 360;
		transform.DORotate(new Vector3(0, 0, val), RotationDuration, RotateMode.FastBeyond360).SetLoops(-1);
	}
}
