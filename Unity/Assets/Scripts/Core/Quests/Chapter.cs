using UnityEngine;
using System.Collections.Generic;

public class Chapter : MonoBehaviour {
  public string CompleteFSMEvent;
  
  public const string CHAPTER_COMPLETE_EVENT = "CHAPTER_COMPLETED"; // This event is broadcasted to all FSMs
  
  public Quest IntroQuest;

  // Quests to start when this chapter is activated
  public Quest[] EnableQuestsOnActivate;

  // Quests required to complete this chapter
  public Quest[] RequiredQuests;

  public bool AdvanceWhenRequiredQuestsComplete;

  private Quest[] m_quests;

  [PersistAttribute]
  private bool m_isActive;
  public bool IsActive {
    get {
      return m_isActive;
    }
  }

  public Chapter NextChapter;

  void Awake()
  {
    // Sometimes something else will attempt to access a chapter's quests before Awake is called due to order of execution (ex. QuestManager)
    if (m_quests == null)
    {
      m_quests = GetComponentsInChildren<Quest> (true);
    }

    foreach (Quest q in m_quests)
    {
      q.QuestCompleted += childQuestCompleted;
    }
  }
  public void Deactivate()
  {
    m_isActive = false;
  }
  
  [ContextMenu ("Activate")]
  public void Activate()
  {
    if (!m_isActive)
     {
      m_isActive = true;

      foreach (Quest q in EnableQuestsOnActivate)
      {
        q.gameObject.SetActive(true);
      }

      if (IntroQuest != null)
      {
        IntroQuest.StartQuest();
      }
      PegasusManager.Instance.GLSDK.AddTelemEventValue( "chapterId", name );
      PegasusManager.Instance.AppendDefaultTelemetryInfo();
      PegasusManager.Instance.GLSDK.SaveTelemEvent( "Chapter_start" );
  }
    else
    {
      Debug.LogWarning("[Chapter] Tried to start chapter "+name+" when it's already started.", this);
    }
  }

  private void childQuestCompleted(Quest q)
  {
    if (AdvanceWhenRequiredQuestsComplete)
    {
      foreach (Quest quest in RequiredQuests)
      {
        if (quest.CalculateQuestState() != QuestState.COMPLETE)
        {
          return;
        }
      }

      // If we get past the above loop without returning, all required quests are complete
      complete();
    }
  }

  private void complete()
  {
    Debug.Log ("[Chapter] complete (all required quests are done). Starting chapter "+NextChapter);
    PlayMakerFSM.BroadcastEvent(CompleteFSMEvent);

    QuestManager.Instance.SetActiveChapter(NextChapter);
  }

  void OnDestroy()
  {
    foreach (Quest q in m_quests)
    {
      q.QuestCompleted -= childQuestCompleted;
    }
  }

  public List<Quest> GetActiveQuests()
  {
    if (m_quests == null)
    {
      m_quests = GetComponentsInChildren<Quest> (true);
    }

    List<Quest> returnArray = new List<Quest>();
    foreach (Quest q in m_quests)
    {
      if (q.gameObject.activeInHierarchy)
        returnArray.Add(q);
    }

    return returnArray;
  }

  public List<Quest> GetQuests()
  {
    if (m_quests == null)
    {
      m_quests = GetComponentsInChildren<Quest> (true);
    }

    // TODO: Memory safe?
    return new List<Quest>(m_quests);
  }
}
