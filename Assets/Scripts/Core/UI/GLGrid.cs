using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("NGUI/Interaction/Grid")]
public class GLGrid : UIGrid {

  public List<EventDelegate> onReposition = new List<EventDelegate>();

  public Bounds Bounds { get; private set; }

  public bool KeepAlignedWithPanelBounds;

  [ContextMenu("Execute")]
  public override void Reposition ()
  {
    base.Reposition();
    
    Bounds = calculateBounds();
    if (KeepAlignedWithPanelBounds)
      Displace(Vector3.zero);

    EventDelegate.Execute(onReposition);
  }
  
  public void DisplaceRight()
  {
    Displace(new Vector3(cellWidth,0,0));
  }

  public void DisplaceLeft()
  {
    Displace(new Vector3(-cellWidth,0,0));
  }

  public void DisplaceDown()
  {
    Displace(new Vector3(0,-cellHeight,0));
  }

  public void DisplaceUp()
  {
    Displace(new Vector3(0,cellHeight,0));
  }

  // Note: This displaces by the given vector ONLY if there's anything to reveal by the displacement. Otherwise it stays still.
  public void Displace(Vector3 v)
  {
    // Bound extremes must be within one cell space inwardly of panel bounds
    UIPanel parentPanel = NGUITools.FindInParents<UIPanel>(gameObject);
    if (parentPanel.clipping == UIDrawCall.Clipping.None)
    {
      // No bounds on the panel
      return;
    }

    Vector3[] panelCorners = parentPanel.worldCorners;
    Matrix4x4 worldToLocal = transform.worldToLocalMatrix;
    for (int i=0; i < panelCorners.Length; i++)
    {
      panelCorners[i] = worldToLocal.MultiplyPoint3x4(panelCorners[i]);
    }
    
    Bounds panelBounds = new Bounds();
    panelBounds.SetMinMax(panelCorners[0], panelCorners[2]);

    if (panelBounds.Contains(Bounds.max) && panelBounds.Contains(Bounds.min))
    {
      // Return early and don't do anything if the panel already contains the grid in its space
      return;
    }

    // Maximum displacements
    float maxLeft = panelCorners[0].x + parentPanel.clipSoftness.x - Bounds.min.x;
    float maxUp = panelCorners[0].y + parentPanel.clipSoftness.y - Bounds.min.y;
    float maxRight = panelCorners[2].x - parentPanel.clipSoftness.x - Bounds.max.x;
    float maxDown = panelCorners[2].y - parentPanel.clipSoftness.y - Bounds.max.y;

    if (maxRight < maxLeft)
    {
      v.x = Mathf.Clamp(v.x, maxRight, maxLeft);
    }

    if (maxDown < maxUp)
    {
      v.y = Mathf.Clamp(v.y, maxDown, maxUp);
    }

    Vector3 targetPosition = transform.localPosition + v;

    SpringPosition.Begin(gameObject, targetPosition, 6f);
  }

  private Bounds calculateBounds()
  {
    Vector3 center, size;
    int numChildren = Utility.GetNumChildren(gameObject, true);
    if (numChildren == 0)
    {
      center = new Vector3(0,0);
      size = new Vector3(0,0);
    }
    else
    {
      if (arrangement == Arrangement.Horizontal)
      {
        center = new Vector3((numChildren-1)*cellWidth/2f, 0);
        size = new Vector3(cellWidth * numChildren, cellHeight);
      }
      else
      {
        center = new Vector3(0, -cellHeight * (numChildren-1)/2);
        size = new Vector3(cellWidth, cellHeight * numChildren);
      }
    }

    return new Bounds(center, size);
  }

}
