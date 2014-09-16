using UnityEngine;
using GlassLab.Core.Serialization;
public class Objective : MonoBehaviour {
  // NOTE: this event isn't being used yet.
  public event ObjectiveEvent ObjectiveCompleted; // Event defined in SignalManager

  public string CompleteFSMEvent;

  public const string OBJECTIVE_COMPLETE_EVENT = "OBJECTIVE_COMPLETED"; // This event is broadcasted to all FSMs

  [PersistAttribute]
  protected bool m_isComplete = false;

  public string Description;

  // Evaluate whether isComplete should be true here!
  public virtual void Refresh () {}
  
  protected virtual void OnEnable()
  {
    if(SignalManager.ObjectiveChanged != null)
    {
      SignalManager.ObjectiveChanged(this);
    }
  }

  protected virtual void OnDisable()
  {
    if(SignalManager.ObjectiveChanged != null)
    {
      SignalManager.ObjectiveChanged(this);
    }
  }

  public virtual void Reset()
  {
    m_isComplete = false;
    if(SignalManager.ObjectiveChanged != null)
    {
      SignalManager.ObjectiveChanged(this);
    }
  }

  public bool CalculateIsComplete()
  {
    Refresh ();
    return m_isComplete;
  }

  protected void onComplete()
  {
    if (SignalManager.ObjectiveCompleted != null)
      SignalManager.ObjectiveCompleted(this); // Signal complete

    if(SignalManager.ObjectiveChanged != null)
    {
      SignalManager.ObjectiveChanged(this);
    }

    if (ObjectiveCompleted != null)
      ObjectiveCompleted (this);

    if (CompleteFSMEvent != null && CompleteFSMEvent != "")
    {
      PlayMakerFSM questFSM = transform.parent.GetComponent <PlayMakerFSM>();
      if (questFSM != null)
      {
        questFSM.SendEvent(CompleteFSMEvent);
      }
    }

    SessionManager.InstanceOrCreate.Save();

    PlayMakerFSM.BroadcastEvent (OBJECTIVE_COMPLETE_EVENT);
  }

  public virtual float GetProgress()
  {
    Debug.Log ("[Objective] GetProgress");
    return m_isComplete ? 1f : 0f;
  }

  public bool IsComplete() 
  {
    return m_isComplete;
  }

  public virtual string GetTitle()
  {
    return "[Objective]";
  }

  public virtual string GetDescription()
  {
    return string.IsNullOrEmpty(Description) ? "[Objective Description]" : Description;
  }

  public virtual void ObjectiveComplete()
  {
    m_isComplete = true;
    onComplete();
    gameObject.SetActive(false);
  }
}
