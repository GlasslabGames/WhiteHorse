using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GLDragDropItem))]
public class ChangeUiSpriteOnDrag : MonoBehaviour {

  public UISprite IdleSprite;
  public UISprite DragSprite;

  private GLDragDropItem m_dragDrop;

  void Awake() {
    DragSprite.enabled = false;
    IdleSprite.enabled = true;

    m_dragDrop = GetComponent<GLDragDropItem>();
  }

  void OnEnable() {
    if (m_dragDrop != null) {
      m_dragDrop.OnDragStart += OnDragBegin;
      m_dragDrop.OnDragEnd += OnDragDone;
    }
  }

  void OnDisable() {
    if (m_dragDrop != null) {
      m_dragDrop.OnDragStart -= OnDragBegin;
      m_dragDrop.OnDragEnd -= OnDragDone;
    }
  }

  protected void OnDragBegin(GLDragEventArgs args = null) {
    IdleSprite.enabled = false;
    DragSprite.enabled = true;
  }

	protected void OnDragDone(GLDragEventArgs args = null) {
    IdleSprite.enabled = true;
    DragSprite.enabled = false;
  }
}
