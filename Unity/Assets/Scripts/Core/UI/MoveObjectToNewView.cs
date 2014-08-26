using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Built for the Hiro Battle, but could be used in other cases.
/// When we show this view, we also place a certain gameObject in the view, then put the object back when we leave.
/// </summary>
public class MoveObjectToNewView : MonoBehaviour {
	public Camera m_camera;

  public SpriteRenderer[] m_showSprites; // list of sprites to show/hide

  // Store information about the target object when we get it so that we can put it back
  private Transform m_target;
  private Vector3 m_prevPosition;
  private Vector3 m_prevScale;
  private Transform m_prevParent;

	public void Show(Transform target) {
		// Store where the object is now
    m_target = target;
    m_prevPosition = target.position;
    m_prevParent = target.parent;
    m_prevScale = target.localScale;

		// Put the object in place
    target.parent = transform;
    target.localPosition = Vector3.zero; //m_newPosition;
    target.localScale = Vector3.one; //m_newScale;

		// Turn on the camera
		if (m_camera != null) m_camera.enabled = true;

    // Show every sprite in our list of sprites to show
    // E.g. in Hiro we have 2 cameras and there's a white divider sprite
    foreach (SpriteRenderer sprite in m_showSprites) {
      sprite.enabled = true;
    }
	}

	public void Hide() {
		// Put the object back where it was
    m_target.parent = m_prevParent;
    m_target.position = m_prevPosition;
    m_target.localScale = m_prevScale;

		// Turn off the camera
    if (m_camera != null) m_camera.enabled = false;

    // Hide the sprites
    foreach (SpriteRenderer sprite in m_showSprites) {
      sprite.enabled = false;
    }
  }
}
