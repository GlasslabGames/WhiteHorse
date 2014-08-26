using UnityEngine;
using System.Collections;

/**
 * CircleGesture
 * 
 * Assumes center of object is the pivot
 */
using System;


public delegate void UIRotationEvent(float degrees);
public delegate void UILockEvent(float degrees);

public class GLCircleSlider : MonoBehaviour {
  // Events
  public event UIRotationEvent OnRotate; // only when the player turns the wheel
  public event UIRotationEvent OnTurn; // also when the wheel turns by itself
  public event UILockEvent OnLock;
  public event Action OnRelease;
  public event Action OnHold;

  // Limits
  public float MaxRotation = float.MaxValue;
  public float MinRotation = float.MinValue;
  
  // Previous state
  public bool ReturnOnRelease = true;
  private float m_prevRotationDelta;
  private float m_lockDirection;

  /* The rotationRelativeToStart tends to loop around past +/-360, but we want it to always be
   * positive while turning one direction and negative the other. So, account for the loop by
   * using this get/setter instead of setting rotationRelativeToStart directly.
   */
  public float CurrentRotation {
    get {
      return m_rotationRelativeToStart;
    }
    set {
      // Account for errors where we've looped around
      if (value < -180) value += 360;
      else if (value > 180) value -= 360;
      m_rotationRelativeToStart = value;
    }
  }
  private float m_rotationRelativeToStart = 0;

  // Lock positions
  public float[] LockRotations;
  //public float LockSensitivity = -1f;
  private int m_lastLockedRotationIndex = -1;

  // Used in drag angle calculation
  private Vector2 m_prevTouchPoint;
  private Vector2 m_positionOnUI;
  private bool m_isDragging; // We may want to force the drag behavior to stop so we track on our own
  public bool IsDragging { get { return m_isDragging; } }
  
  public float InnerRadius;
  public GameObject InnerTarget; // Pass events to his object when we're inside the radius

  void OnDrag(Vector2 delta)
  {
    if (m_isDragging)
    {
      // Find angle between previous and current positions in relation to center
      Vector2 currentVector = m_prevTouchPoint - m_positionOnUI;
      Vector2 newVector = UICamera.lastTouchPosition - m_positionOnUI;

      // Calculate angle between them with relative sign
      float deltaInDegrees = Vector2.Angle (currentVector, newVector) * Mathf.Sign (Utility.crossProduct (currentVector, newVector));
      float actualDelta = Mathf.Clamp(CurrentRotation + deltaInDegrees, MinRotation, MaxRotation) - CurrentRotation;

      // Rotate
      if (actualDelta != 0)
      {
        float movedDelta = ManualRotateByDegrees(actualDelta);

        // Notify anyone who needs to know
        if (OnRotate != null)
          OnRotate (movedDelta);
      }

      // Record last touch position for future drags
      m_prevTouchPoint = UICamera.lastTouchPosition;
    }
  }

  void OnPress(bool isDown)
  {
    if (isDown)
    {
      m_prevTouchPoint = UICamera.lastTouchPosition;
      m_positionOnUI = UICamera.currentCamera.WorldToScreenPoint (new Vector3 (transform.position.x, transform.position.y, 0));
	
			// Check if we fall inside the inner radius
      // Try to get the touch point in local coordinates
      Vector2 screenPos = Input.mousePosition;
      Camera cam = NGUITools.FindCameraForLayer(gameObject.layer);
      Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
      Vector3 localPos = transform.worldToLocalMatrix.MultiplyPoint3x4(worldPos);
      localPos.z = 0;
      float distSquared = localPos.sqrMagnitude;

      if (distSquared > InnerRadius*InnerRadius)
      {
        m_isDragging = true;
        
        if (OnHold != null)
          OnHold();
      } else if (InnerTarget != null) {
        InnerTarget.SendMessage("OnClick");
      }
    } else if (m_isDragging)
    {
      // Check if near any lock points
      int closestLockIndex = -1;
      float closestLockDistance = float.MaxValue; //LockSensitivity != -1 ? LockSensitivity : float.MaxValue;
      for (int i = LockRotations.Length-1; i >= 0; i--)
      {
        float distance = Mathf.Abs(LockRotations[i] - CurrentRotation);

        if (distance < closestLockDistance)
        {
          closestLockDistance = distance;
          closestLockIndex = i;
        }
      }

      if (closestLockIndex != -1)
      {
        m_lastLockedRotationIndex = closestLockIndex;
        if (OnLock != null) OnLock(CurrentRotation);
      }

      if (ReturnOnRelease)
      {
        if (m_lastLockedRotationIndex != -1)
        {
          ManualSetRotation(LockRotations[m_lastLockedRotationIndex]);
        }else 
        {
          ManualRotateByDegrees(CurrentRotation);
        }
      }
      m_isDragging = false;

      if (OnRelease != null)
        OnRelease();
    }
  }

  public float ManualRotateByDegrees(float degrees)
  {
    if (m_lockDirection == Mathf.Sign (degrees))
    {
      return 0;
    } else if (m_lockDirection != 0f)
    {
      m_lockDirection = 0f;
    }

    transform.Rotate (0, 0, degrees);
    CurrentRotation += degrees;
    m_prevRotationDelta = degrees;

    if (OnTurn != null) OnTurn (degrees);
    
    return degrees;
  }

  public void ManualSetRotation(float degrees)
  {
    CurrentRotation += degrees - transform.localEulerAngles.z;
    transform.localEulerAngles = new Vector3(
      transform.localEulerAngles.x,
      transform.localEulerAngles.y,
      degrees);
  }

  public void LockDirection(bool flip = false)
  {
    m_lockDirection = Mathf.Sign (m_prevRotationDelta) * (flip ? -1 : 1);
  }

  /**
   * Lock - Locks the current rotation as the base rotation and releases the drag behavior
   */
  public void Lock()
  {
    m_isDragging = false; // Force drag behavior to end

    if (OnRelease != null)
      OnRelease();
  }
}
