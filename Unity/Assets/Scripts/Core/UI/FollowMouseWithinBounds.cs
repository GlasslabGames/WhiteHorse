using UnityEngine;
using System.Collections.Generic;


public class FollowMouseWithinBounds : MonoBehaviour
{
  private Collider boundingCollider;
  private Bounds currentBounds;

  // Inertia
  public float VelocityDragCoefficient = .9f;
  private Vector2 m_velocity = new Vector2(); // Velocity of this object
  private Vector3 m_lastTouchLocation; // Used to determine m_touchMovedLastFrame
  private bool m_touchMovedLastFrame = false; // Used to track whether a touch point actually moved last frame

  public static FollowMouseWithinBounds Instance; // give access to the instance only if this object exists
  private static Vector3 VECTOR = new Vector3();

  void Awake()
  {
    SignalManager.RoomChanged += onRoomChanged;
  }

  void OnDestroy()
  {
    SignalManager.RoomChanged -= onRoomChanged;
  }

	private void Start()
	{
    Instance = this;

    refresh();
	}

  void OnEnable() 
  {
    SignalManager.SceneLoaded += onSceneLoaded;
  }

  void OnDisable() 
  {
    SignalManager.SceneLoaded -= onSceneLoaded;
  }

  void Update()
  {
    if (Input.GetMouseButtonDown(0))
    {
      m_touchMovedLastFrame = m_lastTouchLocation != Input.mousePosition;
      m_lastTouchLocation = Input.mousePosition;
    }
    else if (m_velocity.sqrMagnitude > .0001f) // If we're moving by less than a thousandth of a pixel, just stop.
    {
      m_velocity *= VelocityDragCoefficient;
      MoveWithinBounds(m_velocity);
    }
  }
  private void onPress(bool isDown)
  {
    // If we were moving last frame, continue with velocity
    if (m_touchMovedLastFrame)
    {
      Vector2 delta = getWorldPositionDeltaFromEvent(e);

      m_velocity = -delta;
    }
    else
    {
      m_velocity = Vector2.zero;
    }
  }

  private void onDrag(Vector3 delta)
  {
    Vector2 delta2d = new Vector2(delta.x, delta.y);

    // Raycast to the ground to find where we're trying to go in 2d world.
    MoveWithinBounds(-delta2d);
  }

  private Vector3 getWorldPositionDeltaFromEvent()
  {
    Vector2 currentTouchPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    Vector2 previousTouchPosition = new Vector2(m_lastTouchLocation.x, m_lastTouchLocation.y);
    
    // Map the screen coordinates to world coordinates (in 3d world)
    currentTouchPosition = InputManager.GetWorldTouchPoint(currentTouchPosition.x, currentTouchPosition.y);
    previousTouchPosition = InputManager.GetWorldTouchPoint(previousTouchPosition.x, previousTouchPosition.y);
    
    return currentTouchPosition - previousTouchPosition;
  }
  
  private void MoveWithinBounds(Vector2 delta) {
    // check to see if we've changed what we should follow (e.g. changed rooms)
    // Now try both x and y
    float newX = Mathf.Clamp(transform.position.x + delta.x, currentBounds.min.x, currentBounds.max.x);
    float newY = Mathf.Clamp(transform.position.y + delta.y, currentBounds.min.y, currentBounds.max.y);
    VECTOR.Set(newX, newY, 0);
    transform.position = VECTOR;
  }

  private void onSceneLoaded(string sceneName, GameObject sceneObj)
  {
    refresh();
  }

  private void onRoomChanged(string roomName)
  {
    refresh();
  }

  // Call this to look for a new bounding container
  private void refresh() {
    // use the first active Bounds object with a collider
    GameObject[] objs = GameObject.FindGameObjectsWithTag("Bounds");

    foreach (GameObject obj in objs) {
      boundingCollider = obj.collider;
      
      if (boundingCollider != null) {

        // shrink the bounds so that we don't go all the way to the edge of the screen
        Bounds bounds = boundingCollider.bounds;
        if (!boundingCollider.enabled)
        {
          Debug.LogWarning("Collider disabled, forcing reading of bounds", this);
          boundingCollider.enabled = true;
          bounds = boundingCollider.bounds;
          boundingCollider.enabled = false;
        }

        float height = 2*Camera.main.orthographicSize;
        float width = height*Camera.main.aspect;
        VECTOR.Set(
          Mathf.Max(bounds.extents.x - 0.5f*width,0),
          Mathf.Max(bounds.extents.y - 0.5f*height,0),
          bounds.extents.z
          );
        bounds.extents = VECTOR;
        
        currentBounds = bounds;

        transform.position = bounds.center; // move to the center

        break;
      }
    }
  }
}