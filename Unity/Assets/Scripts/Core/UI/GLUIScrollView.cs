using UnityEngine;
using System.Collections;

public class GLUIScrollView : UIScrollView {

  private float eps = 1e-1f;

  protected override bool shouldMove {
    get {

      if (panel == null) return base.shouldMove;

      if (!disableDragIfFits) return true;
      Vector4 clip = panel.finalClipRegion;
      Bounds b = bounds;
      
      float hx = (clip.z == 0f) ? Screen.width  : clip.z * 0.5f;
      float hy = (clip.w == 0f) ? Screen.height : clip.w * 0.5f;
      
      if (canMoveHorizontally)
      {
        if (b.min.x + eps < clip.x - hx) return true;
        if (b.max.x - eps > clip.x + hx) return true;
      }
      
      if (canMoveVertically)
      {
        if (b.min.y + eps < clip.y - hy) return true;
        if (b.max.y - eps > clip.y + hy) return true;
      }
      return false;
    }
  }
}
