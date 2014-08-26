using UnityEngine;
using System.Collections;

/// <summary>
/// Add this as a child to an InteractableRoomView.
/// </summary>
public class Interaction : MonoBehaviour {
  public InteractionProperties Properties = new InteractionProperties();
	private bool m_done;

	protected InteractiveObject m_item;

  // HACK ideally this is handled by type or another type of check. Instead it's set in constructor.
  public bool m_usesDragDropInteractions = false;
  public bool UsesDragDropInteractions { get { return m_usesDragDropInteractions; } }

  protected virtual void Reset() {
    Properties = new InteractionProperties();
	  Properties.Priority = InteractionProperties.Priorities.MEDIUM;
  }

  public virtual bool CanUse(GLDragDropItem target)
  {
    return false;
  }

  protected virtual void Awake() {
    // look for the interactable room view on this object or on the parent
    m_item = GetComponent<InteractiveObject>();
    if (m_item == null) {
      m_item = transform.parent.GetComponent<InteractiveObject>();
    }
    if (m_item == null) {
      Debug.LogError("Couldn't find an interactive parent ("+transform.parent.name+") on interaction "+name+"!", this);
    }
  }

  public virtual void Do(GLDragEventArgs sourceArgs)
  {
    Do();
  }

  public virtual void Do() {
    // if we used an object for this action, remove it
    if (Properties.ActivatedBy > 0) {
      if (!EquipmentManager.Instance.HasEquipment(Properties.ActivatedBy)) {
        Debug.LogError("Trying to use item "+Properties.ActivatedBy+" on "+name+", but it's not in the inventory", this);
        return;
      }
      else
      {
        EquipmentManager.Instance.Remove(Properties.ActivatedBy);
      }
    }

    m_done = true;

    if (Properties.DestroyAfterUse) {
      Destroy (m_item.gameObject);
    }
  }

  public virtual bool IsPossible() { // override this for cases like advanceQuest, where the action might be impossible despite the condition being satisfied
    return !Properties.OnceOnly || !m_done;
  }
}
