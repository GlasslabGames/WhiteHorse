using UnityEngine;

public class InputManager : SingletonBehavior<InputManager> 
{
  private static Plane m_touchPlane = new Plane (Vector3.forward, Vector3.zero);
  
  public static Vector3 GetWorldTouchPoint(float x, float y, Camera cam = null)
  {
    if (cam == null)
      cam = Camera.main;
    
    float distance;
    Ray ray = cam.ScreenPointToRay(new Vector3(x, y, 0));
    m_touchPlane.Raycast (ray, out distance);
    return ray.GetPoint (distance);
  }

  public static Vector3 GetWorldTouchPoint(Camera cam = null)
  {
    return GetWorldTouchPoint (UICamera.lastTouchPosition.x, UICamera.lastTouchPosition.y, cam);
  }
}
