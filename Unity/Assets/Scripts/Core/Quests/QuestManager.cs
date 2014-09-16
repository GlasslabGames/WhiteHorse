using UnityEngine;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using System.Collections;
using GlassLab.Core.Serialization;

public class QuestManager : SingletonBehavior<QuestManager> {
  //private List<Chapter> m_chapters = new List<Chapter>();
  private Chapter[] m_chapters;

  private Chapter m_currentChapter;
  public Chapter CurrentChapter
  {
    get { return m_currentChapter; }
  }

  [PersistAttribute]
  private List<string> m_completedQuests = new List<string>();
  
  [PersistAttribute]
  private List<string> m_activeQuests = new List<string>();

  private Quest m_currentActiveQuest;

  [PersistAttribute]
  private Dictionary<string, string> m_lastQuestStates = new Dictionary<string, string> ();

  public Chapter StartingChapter;

  private QuestManager() {}

  public Quest GetCurrentActiveQuest()
  {
    return m_currentActiveQuest;
  }

  // Used when there is no quest to display some sort of information on the QuestView on the upper left.
  [PersistAttribute]
  public string BackupQuestName = "<Quest>";
  
  // Used when there is no quest to display some sort of information on the QuestView on the upper left.
  [PersistAttribute]
  public string BackupObjective = "<Objective>";

  // Is true when current quest label is not updated by BackupQuestName
  [PersistAttribute]
  public bool IsNewBackupQuest = false;

#if UNITY_EDITOR
  public string DebugBroadcastFSMEvent = "";

  void Update()
  {
    if (!string.IsNullOrEmpty(DebugBroadcastFSMEvent))
    {
      PlayMakerFSM.BroadcastEvent(DebugBroadcastFSMEvent);
      DebugBroadcastFSMEvent = "";
    }
  }
#endif


  public void StartQuestByName(string questName)
  {
    Quest q = GetQuest(questName);
    if (q == null) Debug.LogError("Couldn't find a quest with name "+questName);
    q.StartQuest();
  }

  public void SetActiveChapter(Chapter c)
  {
    if (m_currentChapter != null)
    {
      m_currentChapter.Deactivate();
    }

    m_currentChapter = c;
    c.Activate();

    SessionManager.InstanceOrCreate.Save();
  }

  public Chapter GetChapter(string chapterName)
  {
    foreach (Chapter c in m_chapters)
    {
      if (c.name == chapterName)
        return c;
    }

    return null;
  }

  // Note: This can be made more efficient by hashing the quests in the manager
  public Quest GetQuest(string questName)
  {
    foreach (Chapter c in m_chapters)
    {
      List<Quest> quests = c.GetQuests();
      if (quests == null) { return null; }
      foreach (Quest q in quests)
      {
        if (q.gameObject.name == questName)
        {
          return q;
        }
      }
    }

    return null;
  }

  // This returns all available quests as well as the quest we're on
  public Quest[] GetAllActiveQuests()
  {
    List<Quest> returnList = new List<Quest>();
    foreach (Chapter c in m_chapters)
    {
      returnList.AddRange(c.GetActiveQuests());
    }

    return returnList.ToArray();
  }

  void OnSave()
  {
    if (m_activeQuests == null)
    {
      return;
    }

    foreach (string questName in m_activeQuests)
    {
      Quest q = GetQuest(questName);
      m_lastQuestStates[questName] = q.GetCheckpointEventString();
    }
  }

  private void onQuestCanceled(Quest q)
  {
    m_currentActiveQuest = null;
    m_activeQuests.Remove(q.name);
  }

  private void onQuestStarted(Quest q)
  {
    m_currentActiveQuest = q;
    
    if (m_activeQuests.Contains(q.name))
    {
      Debug.LogWarning("[QuestManager] Got a quest start event from a quest that's already started", this);
      return;
    }
    m_activeQuests.Add(q.name);
  }

  private void onQuestCompleted(Quest q)
  {
    m_completedQuests.Add (q.name);

    // if we have a variable to track when this quest is complete, update it
    string var = "QuestComplete_"+q.name;
    if (DialogueLua.DoesVariableExist(var)) {
      DialogueLua.SetVariable(var, "true");
      Debug.Log ("[QuestManager] Setting "+var+" to true since we finished the quest.", q);
    }

    if (q == m_currentActiveQuest)
      m_currentActiveQuest = null;

    m_activeQuests.Remove (q.name);
  }

  public List<string> GetCompletedQuests()
  {
    return m_completedQuests;
  }

  // return true if this is the player's first mission (after the intro chapter)
  public bool CheckForMissionIntro() {
    foreach (string q in m_completedQuests) Debug.Log(q);
    return m_completedQuests.Count <= 3; // if they've done all the intro quests but no more 
  }

  override protected void Awake() {
    m_chapters = GetComponentsInChildren<Chapter> ();
    
    SignalManager.QuestCompleted += onQuestCompleted;
    SignalManager.QuestCanceled += onQuestCanceled;
    SignalManager.QuestStarted += onQuestStarted;

    // Must run after subscribing to events (current quest tracked through events followed by starting quest)
    if ((m_completedQuests == null || m_completedQuests.Count == 0) &&
        (m_activeQuests == null || m_activeQuests.Count == 0) &&
        m_currentActiveQuest == null) // no previous state deserialization
    {
      // Default init
      if (m_currentChapter == null) // If we have no previous active chapter, start the initial chapter.
      {
        SetActiveChapter(StartingChapter);
      }
    } else
    {
      foreach (Chapter c in m_chapters)
      {
        if (c.IsActive)
        {
          m_currentChapter = c;
          break;
        }
      }
      
      // DESERIALIZE
      for (int i=m_activeQuests.Count-1; i >= 0; i--)
      {
        string questName = m_activeQuests[i];
        string lastState = m_lastQuestStates[questName];
        Quest q = GetQuest(questName);
        if (q != null)
        {
          q.StartQuest(lastState);
        }
        else
        {
          Debug.LogError("[QuestManager] Could not start quest "+questName, this);
        }
      }
    }

    base.Awake();
  }

  protected override void OnDestroy()
  {
    SignalManager.QuestCompleted -= onQuestCompleted;
    SignalManager.QuestCanceled -= onQuestCanceled;
    SignalManager.QuestStarted -= onQuestStarted;
  }
}
