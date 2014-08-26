using UnityEngine;
using System;
using System.Collections;

// A component that handles zooming a camera in to a specific point and back out again
[RequireComponent(typeof(Camera))]
public class CameraZoom : MonoBehaviour {
  // the camera sibling
	private Camera m_camera;

  // store the starting position & size so we can zoom out later
	private float m_startSize;
	private Vector3 m_startPos;

  public float m_defaultZoomSize = 5;
	public Vector3 m_defaultZoomPos;
	public float m_defaultDuration = 1;

  private Action m_callback;

	void Start () {
		m_camera = GetComponent<Camera>();
		m_startSize = m_camera.orthographicSize;
		m_startPos = m_camera.transform.position;
	}
	
	void Update () {}

	public void ZoomIn(Action callback = null) {
		// Use the zoomSize & zoomPos set in the inspector
		ZoomTo (m_defaultZoomSize, m_defaultZoomPos, m_defaultDuration, callback);
	}

  public void ZoomIn(float zoomSize, Vector3 zoomPos, float duration, Action callback = null) {
		ZoomTo (zoomSize, zoomPos, duration, callback);
	}

  public void ZoomOut(Action callback = null) {
		ZoomTo (m_startSize, m_startPos, m_defaultDuration, callback);
	}

  public void ZoomOut(float duration, Action callback = null) {
		ZoomTo (m_startSize, m_startPos, duration, callback);
	}

  public void ZoomTo(float toSize, Vector3 toPos, float duration, Action callback) {
    m_callback = callback;
    TweenOrthoSize.Begin(gameObject, duration, toSize);
    TweenPosition.Begin(gameObject, duration, toPos)
      .onFinished.Add( new EventDelegate(OnFinish));
  }

  public void OnFinish() {
    if (m_callback != null) { m_callback(); }
  }
}
