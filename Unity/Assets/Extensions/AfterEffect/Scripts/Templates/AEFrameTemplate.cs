using UnityEngine;
using System.Collections;

[System.Serializable]
public class AEFrameTemplate  {
  public int index;

  public float rotation;
  public float opacity;

	public Vector3 scale;
	public Vector3 pivot;
	public Vector3 position;
	public Vector3 positionUnity;


	public bool IsNothingChanged = false;
	public bool IsScaleChanged = false;
  public bool IsPivotChanged = false;
  public bool IsRotationChanged = false;
  public bool IsOpacityChanged = false;
  public bool IsPositionChanged = false;
 
  public AEFrameTemplate(){}

  public AEFrameTemplate(AEFrameTemplate frameToCopy)
  {
    rotation = frameToCopy.rotation;
    opacity = frameToCopy.opacity;
    scale = frameToCopy.scale;
    pivot = frameToCopy.pivot;
    position = frameToCopy.position;
    positionUnity = frameToCopy.positionUnity;
  }

	public void SetPosition(Vector3 pos) {
		position = pos;
		positionUnity = position;
		positionUnity.y = -positionUnity.y;
	}

	private bool V3Equal(Vector3 a, Vector3 b){
		return Vector3.SqrMagnitude(a - b) < 0.0001;
	}

	private bool FCompare(float a, float b){
		return Mathf.Abs(a - b) < 0.01;
	}

}
