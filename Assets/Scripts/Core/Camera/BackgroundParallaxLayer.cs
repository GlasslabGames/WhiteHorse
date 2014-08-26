using UnityEngine;

public class BackgroundParallaxLayer : MonoBehaviour
{
  public float CameraMoveMultiplier = 1f;

  private float m_prevCameraPositionX;

	void Awake ()
	{
    if (Camera.main == null)
    {
      Debug.LogError("Could not find main camera, disabling.", this);
      enabled = false;
      return;
    }
    m_prevCameraPositionX = Camera.main.transform.position.x;
	}
	
	void Update ()
	{
    if (Camera.main.transform.position.x != m_prevCameraPositionX)
    {
      m_prevCameraPositionX = Camera.main.transform.position.x;

      refreshPosition();
    }
	}

  void OnEnable()
  {
    refreshPosition();
  }

  private void refreshPosition()
  {
	Vector3 VECTOR = transform.position;
    VECTOR.x = -Camera.main.transform.position.x * CameraMoveMultiplier;
    transform.position = VECTOR;
  }
}
