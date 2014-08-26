using UnityEngine;

public class CameraFollow : MonoBehaviour 
{
	public float xMargin = 1f;		// Distance in the x axis the player can move before the camera follows.
	public float yMargin = 1f;		// Distance in the y axis the player can move before the camera follows.
	public float xSmooth = 8f;		// How smoothly the camera catches up with it's target movement in the x axis.
	public float ySmooth = 8f;		// How smoothly the camera catches up with it's target movement in the y axis.
	public Vector2 maxXAndY;		// The maximum x and y coordinates the camera can have.
	public Vector2 minXAndY;		// The minimum x and y coordinates the camera can have.

	private float currentDistance;

	private Transform target;		// Thing to follow
  private bool m_isSnapping = false; // Whether to snap to the target for a frame

  private FollowMouseWithinBounds m_followBehavior;

  public delegate void distanceDel(float dist);
  public event distanceDel OnScroll;

  private static Vector3 VECTOR3 = new Vector3();
  
  void Awake()
  {
    SignalManager.RoomChanged += onRoomChanged;
  }

  void Destroy()
  {
    SignalManager.RoomChanged -= onRoomChanged;
  }

	void Start ()
	{
		// Setting up the reference.
    GameObject obj = GameObject.FindGameObjectWithTag("CameraFollowTarget");

		if (obj != null) target = obj.transform;
    else {
      obj = GameObject.FindGameObjectWithTag("Player"); // legacy since we used the tag Player in older scenes
      if (obj != null) target = obj.transform;
    }
	}

  void onRoomChanged(string roomName)
  {
    m_isSnapping = true;
  }

	bool CheckXMargin()
	{
		// Returns true if the distance between the camera and the player in the x axis is greater than the x margin.
		return Mathf.Abs(transform.position.x - target.position.x) > xMargin;
	}


	bool CheckYMargin()
	{
		// Returns true if the distance between the camera and the player in the y axis is greater than the y margin.
		return Mathf.Abs(transform.position.y - target.position.y) > yMargin;
	}


	void FixedUpdate ()
	{
		if (target != null) {
      if (m_isSnapping)
      {
        SnapToTarget();
      }
      else
      {
		    TrackTarget();
      }
    }
	}
	
  void SnapToTarget()
  {
    transform.position = VECTOR3;
    VECTOR3.Set(target.position.x, target.position.y, transform.position.z);
    m_isSnapping = false;
  }
	
	void TrackTarget ()
	{
		// By default the target x and y coordinates of the camera are it's current x and y coordinates.
		float targetX = transform.position.x;
		float targetY = transform.position.y;

		// If the player has moved beyond the x margin...
		if(CheckXMargin())
			// ... the target x coordinate should be a Lerp between the camera's current x position and the player's current x position.
			targetX = Mathf.Lerp(transform.position.x, target.position.x, xSmooth * Time.deltaTime);

		// If the player has moved beyond the y margin...
		if(CheckYMargin())
			// ... the target y coordinate should be a Lerp between the camera's current y position and the player's current y position.
			targetY = Mathf.Lerp(transform.position.y, target.position.y, ySmooth * Time.deltaTime);

		// The target x and y coordinates should not be larger than the maximum or smaller than the minimum.
		targetX = Mathf.Clamp(targetX, minXAndY.x, maxXAndY.x);
		targetY = Mathf.Clamp(targetY, minXAndY.y, maxXAndY.y);

    Vector3 prevPos = transform.position; 

		// Set the camera's position to the target position with the same z component.
    VECTOR3.Set(targetX, targetY, transform.position.z);
    transform.position = VECTOR3;

    if (OnScroll != null) OnScroll( Vector3.Distance(prevPos, transform.position) );
	}
}
