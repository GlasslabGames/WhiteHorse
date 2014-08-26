//
// ObjectiveView.cs
// Author: Jerry Fu <jerry@glasslabgames.org>
// 2014 - 7 - 8

using UnityEngine;

public class ObjectiveView : MonoBehaviour
{
  private const string CHECKBOX_CHECKED_IMAGE = "checkbox_checked";
  private const string CHECKBOX_EMPTY_IMAGE = "checkbox_empty";
  private readonly Color HIGHLIGHT_COLOR = new Color(1.0f, .918f, .5f);

  public UITexture BulletPointImage;

  public UILabel Text;

  private Objective m_objective;

  void Awake()
  {
    SignalManager.ObjectiveChanged += onObjectiveUpdated;
    SignalManager.ObjectiveCompleted += onObjectiveCompleted;
  }

  private void refresh()
  {
    if (m_objective == null) return;


    Text.text = m_objective.GetDescription();

    if (m_objective.IsComplete())
    {
      BulletPointImage.mainTexture = Resources.Load<Sprite>(CHECKBOX_CHECKED_IMAGE).texture;
    }
    else
    {
      BulletPointImage.mainTexture = Resources.Load<Sprite>(CHECKBOX_EMPTY_IMAGE).texture;
    }

    if (!m_objective.gameObject.activeInHierarchy)
    {
      Text.color = BulletPointImage.color = Color.gray;
    }
    else
    {
      Text.color = Color.white;
      BulletPointImage.color = HIGHLIGHT_COLOR;
    }
  }
  
  private void onObjectiveUpdated(Objective o)
  {
    if (o == m_objective)
    {
      refresh();
    }
  }
  private void onObjectiveCompleted(Objective o)
  {
    if (o == m_objective)
    {
      refresh();
    }
  }

  public void SetObjective(Objective o)
  {
    m_objective = o;

    refresh();
  }

  public void SetInfo(string description)
  {
    Text.text = description;
    
    BulletPointImage.mainTexture = Resources.Load<Sprite>(CHECKBOX_EMPTY_IMAGE).texture;
    Text.color = Color.white;
    BulletPointImage.color = HIGHLIGHT_COLOR;
  }

  void OnDestroy()
  {
    SignalManager.ObjectiveChanged -= onObjectiveUpdated;
    SignalManager.ObjectiveCompleted -= onObjectiveCompleted;
  }
}