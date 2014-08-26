//
// QuestView.cs
// Author: Jerry Fu <jerry@glasslabgames.org>
// 2014 - 7 - 8

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class QuestView : MonoBehaviour
{
  public const float AUTOMATIC_OPEN_SECONDS = 5f;
  public TweenScale OpenTween;
  public GameObject ContentsContainer;
  private TextPopup m_contentPopup;

  private ObjectiveView[] m_objectiveViews;
  
  public QuestCompleteAnimationControl questCompleteAni;

  private bool m_paused;
  private float m_openForSecondsAfterActivity = -1f; // how long to keep the window open after it was automatically shown


  void Awake()
  {
    SignalManager.QuestStarted += onQuestStarted;
    SignalManager.ObjectiveCompleted += onObjectiveCompleted;
    SignalManager.QuestCompleted += onQuestCompleted;
    SignalManager.QuestCanceled += onQuestCompleted;
    SignalManager.Paused += onPaused;

    m_contentPopup = ContentsContainer.GetComponentsInChildren<TextPopup>(true)[0]; // need to search in inactive objects too
    m_objectiveViews = GetComponentsInChildren<ObjectiveView>(true);
  }

  void Start() {
    // wait a moment for the scene transtion to end
    Utility.Delay( delegate {
      Open (AUTOMATIC_OPEN_SECONDS);
    }, 1f);
  }

  void Update()
  {
    // if we had opened the popup automatically for a brief period of time
    if (m_openForSecondsAfterActivity > 0 && !m_paused) {
      m_openForSecondsAfterActivity -= Time.deltaTime;
      if (m_openForSecondsAfterActivity <= 0) {
        Close ();
      }
    }
  }

  public bool IsOpen()
  {
    return ContentsContainer.activeSelf;
  }

  private void onPaused(bool paused)
  {
    m_paused = paused;
    if (paused) Close(); // when the game becomes paused, hide the window
    else if (m_openForSecondsAfterActivity > 0) {
      Open (m_openForSecondsAfterActivity); // if we still want to automatically open it, do that after unpausing
    }
  }

  private void onObjectiveCompleted(Objective o)
  {
    Open(AUTOMATIC_OPEN_SECONDS);
  }

  private void onQuestStarted(Quest q)
  {
    Open(AUTOMATIC_OPEN_SECONDS);
  }

  private void onQuestCompleted(Quest q)
  {
    Debug.Log ("[QuestView] Quest completed! Open popup and show anim.");
    questCompleteAni.IsPendingPlayAnimation = true;

    Open(AUTOMATIC_OPEN_SECONDS);
  }

  public void OpenButton() {
    questCompleteAni.IsPendingPlayAnimation = false; // never play the animation when they choose to open the popup

    // If we're on the very first quest, jiggle the suitcase to draw their attention to it
    if (QuestManager.Instance != null && QuestManager.Instance.GetCurrentActiveQuest() != null && 
        QuestManager.Instance.GetCurrentActiveQuest().name == "Quest0-1") {
      // look for the animation that's on the suitcase item in their inventory
      foreach (PlayAnimationWithInterval ani in transform.root.GetComponentsInChildren<PlayAnimationWithInterval>())
      {
        if(ani.gameObject.name.Contains("Suitcase"))
          ani.Play(true);
      }
    }

    Open ();
    
    PegasusManager.Instance.AppendDefaultTelemetryInfo();
    PegasusManager.Instance.GLSDK.SaveTelemEvent( "Access_missionPopup" );
  }

  public void Open(float closeAfterSeconds = 0)
  {
    Debug.Log ("[QuestView] Open mission popup. Close after: "+closeAfterSeconds);
    // record that we wanted to have it open automatically for a certain time
    m_openForSecondsAfterActivity = closeAfterSeconds;

    // if we wanted to open automatically, then m_openForSecondsAfterActivity is > 0, so we'll open when the game is unpaused anyway
    if (ExplorationUIManager.Instance.Paused) {
      Debug.Log ("[QuestView] Game is paused, so don't open the popup yet.");
      return;
    }

    // else open now
    refreshQuest();
    ContentsContainer.SetActive(true);

    OpenTween.Reset();
    OpenTween.PlayForward();

  }

  public void CloseButton()
  {
    if (m_paused) return;

    if (IsOpen()) {
      PegasusManager.Instance.AppendDefaultTelemetryInfo();
      PegasusManager.Instance.GLSDK.SaveTelemEvent( "Close_missionPopup" );
    }

    m_openForSecondsAfterActivity = 0; // clear the autoclose counter
    Close();
  }

  public void Close(bool permanently = false)
  {
    ContentsContainer.SetActive(false);
  }
  
  private void refreshQuest()
  {
    int i=0;
    Quest q = QuestManager.Instance.GetCurrentActiveQuest();
    if (q != null)
    {
      List<Objective> objectives = q.GetObjectives();

      if (objectives.Count > m_objectiveViews.Length)
      {
        Debug.LogError("[QuestView] There are more objectives in quest '" + q.name + "' than objective views to show them!");
      }

      // Update objective views for the ones we have
      for (; i < objectives.Count; i++)
      {
        ObjectiveView objectiveView = m_objectiveViews[i];
        Objective objective = objectives[i];

        objectiveView.SetObjective(objective);
        objectiveView.name = objective.name + "-objectiveView";
        objectiveView.gameObject.SetActive(true);
      }
    }
    else
    {
      ObjectiveView objectiveView = m_objectiveViews[i];
      objectiveView.SetObjective (null);
      objectiveView.SetInfo(QuestManager.Instance.BackupObjective);
      objectiveView.gameObject.SetActive(true);

      i++;
    }

    // Disable remaining objective views
    for (; i < m_objectiveViews.Length; i++)
    {
      m_objectiveViews[i].SetObjective (null);
      m_objectiveViews[i].gameObject.SetActive(false);
    }

    m_contentPopup.Refresh();
  }
  
  void OnDestroy()
  {
    SignalManager.QuestStarted -= onQuestStarted;
    SignalManager.ObjectiveCompleted -= onObjectiveCompleted;
    SignalManager.QuestCompleted -= onQuestCompleted;
    SignalManager.QuestCanceled -= onQuestCompleted;
    SignalManager.Paused -= onPaused;
  }
}