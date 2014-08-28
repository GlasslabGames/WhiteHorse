using UnityEngine;
using System.Collections;

/// <summary>
/// This class should be on labels, etc, which we want to always have a positive x-scale.
/// </summary>
public class DontFlip : MonoBehaviour {
  void Awake() {
    Utility.NextFrame( Refresh ); // if this was just created and added as a child, this should catch it
  }
  
  [ContextMenu("Refresh")]
  public void Refresh() {
    if (this == null) return;

    Vector3 scale = transform.localScale;
    
    if (transform.lossyScale.x < 0) scale.x *= -1; // correct for negative scale by flipping this object
    
    transform.localScale = scale;
  }
}
