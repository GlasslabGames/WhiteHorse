using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Keeps GLDragDropContainers on the UI Layer for objects on the gameplay layer. The object must have a BoxCollider to match the Container's size to.
/// </summary>
public class GLDragDropContainerLayer : MonoBehaviour {
  private Dictionary<Collider, BoxCollider> m_containers = new Dictionary<Collider, BoxCollider>(); // map from the object's collider to its matching GLDragDropContainer
  private Stack<BoxCollider> m_unusedContainers = new Stack<BoxCollider>();

  private Vector3 m_cameraPos;

  public static GLDragDropContainerLayer Instance;

  void Awake() {
    Instance = this;
  }

  void OnEnable() {
    SignalManager.ItemDragStarted += EnableCollidersOnlyWhenDragging;
    SignalManager.ItemDragStopped += EnableCollidersOnlyWhenDragging;
  }

  void OnDisable() {
    SignalManager.ItemDragStarted -= EnableCollidersOnlyWhenDragging;
    SignalManager.ItemDragStopped -= EnableCollidersOnlyWhenDragging;
  }

  void EnableCollidersOnlyWhenDragging(GLDragDropItem item = null) {
    foreach (BoxCollider collider in m_containers.Values) {
      Debug.Log ("Setting "+collider+" enabled to "+(GLDragDropItem.CurrentlyDragging != null));
      collider.enabled = GLDragDropItem.CurrentlyDragging != null;
    }
  }

	void Start () {}
	
  public GLDragDropContainer AddContainer(Collider col) {
    if (m_containers.ContainsKey(col)) {
      Debug.LogWarning("Trying to add a GLDragDropContainer to "+col.name+" when it already has one.", this);
      return null;
    }
    BoxCollider container;
    GLDragDropContainer gddcontainer;
    if (m_unusedContainers.Count > 0) {
      container = m_unusedContainers.Pop();
      container.gameObject.SetActive(true);
      gddcontainer = container.GetComponent<GLDragDropContainer>();
    } else {
      GameObject go = new GameObject();
      container = go.AddComponent<BoxCollider>() as BoxCollider;
      go.transform.parent = transform;
      go.layer = gameObject.layer;
      go.transform.localScale = Vector3.one;
      gddcontainer = container.gameObject.AddComponent<GLDragDropContainer>();
    }

    container.name = col.name + " DragDropContainer";
    Utility.SetUiColliderOverGameObject(container, col);
    m_containers.Add(col, container);

    EnableCollidersOnlyWhenDragging();

    return gddcontainer;
  }

  public GLDragDropContainer RemoveContainer(Collider col) {
    if (!m_containers.ContainsKey(col)) {
      Debug.LogWarning("Trying to remove a GLDragDropContainer from "+col.name+" when it doesn't have one.", this);
      return null;
    }
    BoxCollider container = m_containers[col];
    container.gameObject.SetActive(false);
    m_containers.Remove(col);
    m_unusedContainers.Push(container);

    return container.GetComponent<GLDragDropContainer>();
  }

  void Update () {
    // See if the camera moved. If so, move all the GLDragDropContainers.
    if(Camera.main == null)
    {
      return;
    }

    Vector3 pos = Camera.main.transform.position;
    if (pos != m_cameraPos) {
      foreach (var pair in m_containers) {
        BoxCollider bc = pair.Value;
        Utility.SetUiColliderOverGameObject(bc, pair.Key);
      }
      m_cameraPos = pos;
    }
  }
}
