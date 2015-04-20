//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/***
 * Modification of the NGUI dragDropRoot so that when one is disabled, we default to a previous one
 * Behavior might be strange if you enable/disable the old roots, but it should work ok.
 **/
public class GLDragDropRoot : UIDragDropRoot
{
	// static public Transform root; // in parent
  private static Stack<Transform> m_previousRoots = new Stack<Transform>();

	protected void OnEnable () {
    if (root != null) m_previousRoots.Push(root);
    root = transform;

    //Debug.Log ("[GLDragDropRoot] Enabling root "+this, this );
  }

	protected void OnDisable () {
    if (root == transform) {
      root = null;

      // look through previous roots
      while (m_previousRoots.Count > 0) {
        Transform prev = m_previousRoots.Pop();
        // stop when we reach one that is visible
        if (prev != null && prev.gameObject.activeInHierarchy) {
          root = prev;
          break;
        }
      }
    }

    //Debug.Log ("[GLDragDropRoot] Disabling root "+this+". Singleton: "+root, this );
  }

}
