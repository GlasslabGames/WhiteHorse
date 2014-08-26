using UnityEngine;
using System.Collections;

/// <summary>
/// Catches swipes and calls a callback. This is just a really fast and simple way to do it using 1 touch/mouse click.
/// For now it only catches horizontal swipes. In the future we should use that touch addon instead.
/// </summary>
public class SwipeListener : MonoBehaviour {
  public enum Dir {
    LEFT, RIGHT, UP, DOWN
  }

  public float m_minSwipeDist;
  public float m_maxSwipeTime;

  public delegate void CallbackDelegate (Dir direction);
  public CallbackDelegate Callback { get; set; }

  private bool m_swiping;
  private Vector2 m_startPos;
  private float m_startTime;

  public bool Horizontal = true;

	void Update () {
    if (Input.GetMouseButtonDown(0)) {
      m_startPos = Input.mousePosition;
      m_startTime = Time.time;
      m_swiping = true;
    } 
    if (m_swiping && Input.GetMouseButtonUp(0)) {
      m_swiping = false;

      float time = Time.time - m_startTime;
      float dist = (Horizontal)? Input.mousePosition.x - m_startPos.x : Input.mousePosition.y - m_startPos.y;
      //Debug.Log (System.String.Format("Swipe time: {0} Dist: {1}", time, dist));
      if (time < m_maxSwipeTime && Mathf.Abs(dist) > m_minSwipeDist) {
        if (Callback != null) {
          if (Horizontal) {
            Callback( (dist > 0)? Dir.RIGHT : Dir.LEFT );
          } else {
            Callback( (dist > 0)? Dir.UP : Dir.DOWN );
          }
        }
      }
    }
	}
}
