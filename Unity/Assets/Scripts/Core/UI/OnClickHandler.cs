using UnityEngine;
using System.Collections;

public class OnClickHandler : MonoBehaviour {
  private Vector3 m_clickPos;
  private float m_maxClickDiff = 100;

  void Start() {}

	void OnMouseDown() {
    m_clickPos = Input.mousePosition;
  }

  void OnMouseUp() {
    // Ignore clicks if they hit UI
    if (UICamera.lastHit.collider != null)
    {
      return;
    }

    Vector3 pos = Input.mousePosition;
    if (m_clickPos != null && Vector3.Distance(pos, m_clickPos) < m_maxClickDiff) {
      OnMouseClick();
    }
	}

  void OnClick() { // NGUI
    Debug.Log ("OnClickHandler for "+name+" NGUI OnClick!");
    OnMouseClick();
  }

  protected virtual void OnMouseClick() { }
}
