using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GLDragDropItem))]
public class ChangeAppearanceWhenAttached : MonoBehaviour {
  private GLDragDropItem m_dragItem;

  public GameObject[] m_enableWhenAttached;
  public GameObject[] m_enableWhenUnattached;
  public UILabel[] m_labels;
  public Color m_textColorWhenAttached;
  public Color m_textColorWhenUnattached;

	void Awake() {
    m_dragItem = GetComponent<GLDragDropItem>();
    m_dragItem.OnDropped += HandleOnDrop;
  }

  void OnDestroy() {
    m_dragItem.OnDropped -= HandleOnDrop;
  }

  void HandleOnDrop (GLDragEventArgs args)
  {
    bool attached = args.DragContainer != null;
    foreach (GameObject g in m_enableWhenAttached) {
      g.SetActive( attached );
    }
    foreach (GameObject g in m_enableWhenUnattached) {
      g.SetActive( !attached );
    }
    foreach (UILabel l in m_labels) {
      l.color = (attached)? m_textColorWhenAttached : m_textColorWhenUnattached;
    }
  }
}
