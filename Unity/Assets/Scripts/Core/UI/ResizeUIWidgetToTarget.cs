using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UIWidget))]
public class ResizeUIWidgetToTarget : MonoBehaviour {
  private UIWidget m_widget;
  public Transform Target;
  public float padding = 5;
  public bool ResizeWidth = true;
  public bool ResizeHeight = true;

  public void Awake()
  {
    m_widget = GetComponent<UIWidget> ();
  }

  void OnEnable()
  {
    StartCoroutine(refreshNextFrame());
  }

  private IEnumerator refreshNextFrame()
  {
    yield return null;

    Refresh();
  }

  [ContextMenu("Execute")]
  public void Refresh()
  {
    Bounds targetBounds = NGUIMath.CalculateAbsoluteWidgetBounds (Target);
    Matrix4x4 widgetWorldToLocal = m_widget.transform.worldToLocalMatrix;

    targetBounds.SetMinMax(widgetWorldToLocal.MultiplyPoint(targetBounds.min),widgetWorldToLocal.MultiplyPoint(targetBounds.max));
    
    Vector3 newPosition = m_widget.transform.localPosition;
    if (ResizeWidth) {
      newPosition.x = targetBounds.min.x - padding;
      m_widget.width = (int)(targetBounds.extents.x*2 + padding*2);
    }

    if (ResizeHeight) {
      newPosition.y = targetBounds.min.y - padding;
      m_widget.height = (int)(targetBounds.extents.y*2 + padding*2);
    }

    m_widget.transform.localPosition = newPosition;
  }
}
