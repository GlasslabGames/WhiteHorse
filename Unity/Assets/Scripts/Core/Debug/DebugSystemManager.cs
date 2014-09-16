using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GlassLab.Core.Serialization;

public class DebugSystemManager : SingletonBehavior<DebugSystemManager>
{

  public GameObject DebugWindow;
  public List<GameObject> AlwaysInteractiveChildren;

  public DebugOpenButton OpenButton;
  public GameObject InputNotePanel;
  public UIInput InputNote;

  public static int DebugLogNum = -1;

  public const int MAX_DEBUG_SAVE = 4;

  private DebugSaveSelectButton[] m_saveSelectButtons = null;
  private int m_currentSaveNum = -1;
  public int CurrentSaveNum
  {
    get
    {
      return m_currentSaveNum;
    }
    set
    {
      if (SessionManager.Instance != null)
      {
        bool isIgnoreSave = SessionManager.Instance.SyncContextMenuToggleSaves();
        if (value == -1 && m_currentSaveNum != -1 && isIgnoreSave)
        {
          SessionManager.Instance.ContextMenuToggleSaves();
        }
        else if (value != -1 && m_currentSaveNum == -1 && !isIgnoreSave)
        {
          SessionManager.Instance.ContextMenuToggleSaves();
        }
      }
      m_currentSaveNum = value;
    }
  }

  private List<Collider> m_colliders;
  private bool m_isInteractEnabled = false;

  private Dictionary<string, string> m_startNextQuest = new Dictionary<string, string>(){
    {"Quest0-1", "Quest0-2"},
    {"Quest0-2", "Quest0-3"},
    {"Quest0-3", "Quest0-4"},
    {"Quest0-4", "Quest0-5"},
    {"Quest0-5", "Quest0-7"},
    {"Quest18", "Quest19"}
  };

  private Dictionary<string, string> m_activateNextQuest = new Dictionary<string, string>(){
    {"Quest0-7", "Quest1-1"},
    {"Quest1-1", "Quest0-6"},
    {"Quest0-6", "Quest11"},
    {"Quest11", "Quest13"},
    {"Quest13", "Quest14"},
    {"Quest14", "Quest16"},
    {"Quest16", "Quest18"},
    {"Quest19", "Quest21"},
    {"Quest21", "Quest23"},
    {"Quest23", "Quest24"},
    {"Quest24", "Quest26"},
    {"Quest26", "Quest27"},
    {"Quest27", "Quest28"},
    {"Quest28", "Quest30"},
    {"Quest30", "Quest33"},
    {"Quest33", "Quest34"}
  };

  private Dictionary<string, string> m_questBackupName = new Dictionary<string, string>(){
    {"Quest0-1", "Unpack"},
    {"Quest0-2", "Talk to Lucas"},
    {"Quest0-3", "Choose Your Argubot"},
    {"Quest0-4", "Talk to Lucas"},
    {"Quest0-5", "Build a Bot"},
    {"Quest0-7", "More Training!"},
    {"Quest1-1", "Talk to Dara"},
    {"Quest0-6", "Repair the Argu-Mech"},
    {"Quest11", "Talk to Maya"},
    {"Quest13", "Talk to Lucas"},
    {"Quest14", "Talk to Lucas"},
    {"Quest16", "Talk to Chloe"},
    {"Quest18", "Talk to Ren"},
    {"Quest19", "Talk to Lucas"},
    {"Quest21", "Talk to Dean Ochoa"},
    {"Quest23", "Let's Evo-2"},
    {"Quest24", "Talk to Dara"},
    {"Quest26", "Talk to Lev"},
    {"Quest27", "Talk to Lucas"},
    {"Quest28", "Talk to Dean Ochoa"},
    {"Quest30", "Talk to Adrian"},
    {"Quest33", "Talk to Dean Ochoa"},
    {"Quest34", "Talk to SAM"}
  };

  private Dictionary<string, string> m_objectiveBackupName = new Dictionary<string, string>(){
    {"Quest0-1", "Unpack"},
    {"Quest0-2", "Find Lucas in Workshop"},
    {"Quest0-3", "Choose Your Argubot"},
    {"Quest0-4", "Find Lucas in Rec Room"},
    {"Quest0-5", "Build a Bot"},
    {"Quest0-7", "Store a bot"},
    {"Quest1-1", "Find Dara in Classroom"},
    {"Quest0-6", "Find Argu-Mech in the Workshop"},
    {"Quest11", "Find Maya in Atrium"},
    {"Quest13", "Find Lucas in Workshop"},
    {"Quest14", "Find Lucas in Workshop"},
    {"Quest16", "Find Chloe in Hydrofarm"},
    {"Quest18", "Find Ren in Rec Room"},
    {"Quest19", "Find Lucas in Workshop"},
    {"Quest21", "Find Dean Ochoa in Office"},
    {"Quest23", "Find Lucas in Workshop"},
    {"Quest24", "Find Dara in Classroom"},
    {"Quest26", "Find Lev in Rec Room"},
    {"Quest27", "Find Lucas in Workshop"},
    {"Quest28", "Find Dean Ochoa in Office"},
    {"Quest30", "Find Adrian in Atrium"},
    {"Quest33", "Find Dean Ochoa in Office"},
    {"Quest34", "Find SAM in Rec Room"}
  };

  override protected void Awake()
  {
    DontDestroyOnLoad(this);
    DebugTapListener taplistener = GetComponent<DebugTapListener>();
    if (taplistener != null) taplistener.Callback = ClickOnOpenButton;
    //m_colliders = new List<Collider>(GetComponentsInChildren<Collider>(true));
    SetDebugInteraction(false);
    m_saveSelectButtons = GetComponentsInChildren<DebugSaveSelectButton>(true);
    Debug.Log("Log Num: " + DebugLogNum);
  }

  public void ClickOnOpenButton()
  {
    DebugOpenButton openButton = GetComponentInChildren<DebugOpenButton>();
    if (openButton != null)
      openButton.Click();
  }

  public void ShowOrHideDebugWindow()
  {
    if (DebugWindow != null)
    {
      SetActiveDebugWindow(!DebugWindow.activeSelf);
    }
  }

  public void SetActiveDebugWindow(bool isActive)
  {
    if (DebugWindow != null)
    {
      DebugWindow.SetActive(isActive);
    }
  }

  public bool SwitchInteraction()
  {
    SetDebugInteraction(!m_isInteractEnabled);
    m_isInteractEnabled = !m_isInteractEnabled;
    return m_isInteractEnabled;
  }

  public bool SyncInteraction()
  {
    return m_isInteractEnabled;
  }

  public void SetDebugInteraction(bool isEnable)
  {
    m_colliders = new List<Collider>(GetComponentsInChildren<Collider>(true));
    foreach (Collider c in m_colliders)
    {
      if (c != null)
      {
        if (!AlwaysInteractiveChildren.Contains(c.gameObject))
        {
          c.enabled = isEnable;
          if (isEnable) CheckColliderSize(c);
        }
      }
    }
  }

  private void CheckColliderSize(Collider c)
  {
    DebugLabelControl labelControl = c.gameObject.GetComponent<DebugLabelControl>();
    if (labelControl != null)
      labelControl.AdjustColliderSize();
  }

  public void LoadLastSavedLevel()
  {
    //		var lockButtons = GetComponentsInChildren<DebugLockButton>(true);
    //		foreach(var button in lockButtons)
    //		{
    //			button.TurnOff();
    //		}
    if (!Application.loadedLevelName.Equals("MainMenu"))
      Application.LoadLevel(Application.loadedLevelName);
    Debug.Log("[DebugManager] Load last save.");
  }

  public void ActivateAllQuests()
  {
    var quests = Resources.FindObjectsOfTypeAll(typeof(Quest));
    foreach (Quest q in quests)
    {
      q.gameObject.SetActive(true);
    }
    Debug.Log("Unlock all quests. Instances found: " + (quests != null ? quests.Length.ToString() : "none"), this);
  }

  public void QuitCurrentQuest()
  {
    if (QuestManager.Instance != null)
    {
      Quest q = QuestManager.Instance.GetCurrentActiveQuest();
      if (q != null && q.IsCancelable)
      {
        Debug.Log("Cancel quest: " + q.name, this);
        q.Cancel();
      }
      else
      {
        // show message, can't cancel
        var enableObjects = GetComponentInChildren<EnableObjectForSeconds>();
        enableObjects.ShowObjects(q == null ? "Not In Quest!" : "Quest Not Cancelable!");
      }
    }
  }

  // also activate or start next one
  public void CompleteCurrentQuest()
  {
    if (QuestManager.Instance != null)
    {
      Quest q = QuestManager.Instance.GetCurrentActiveQuest();
      if (q != null && (q.name.Equals("MCreateBot") || q.name.Equals("MLevelUpBot")))
      {
        // can't complete create bot and levelup bot
        var enableObjects = GetComponentInChildren<EnableObjectForSeconds>();
        enableObjects.ShowObjects("Can't complete quest: " + q.name + "!");
      }
      else if (q != null)
      {
        Debug.Log("Complete quest: " + q.name, this);
        q.CompleteQuest();
        if (m_startNextQuest.ContainsKey(q.name))
        {
          Quest nextQuest = QuestManager.Instance.GetQuest(m_startNextQuest[q.name]);
          Debug.Log("Start next quest: " + nextQuest.name, this);
          nextQuest.StartQuest();
        }
        else if (m_activateNextQuest.ContainsKey(q.name))
        {
          Quest nextQuest = QuestManager.Instance.GetQuest(m_activateNextQuest[q.name]);
          Debug.Log("Activate next quest: " + nextQuest.name, this);
          nextQuest.gameObject.SetActive(true);
          ActivateQuest(nextQuest.name,
                        m_questBackupName.ContainsKey(nextQuest.name) ? m_questBackupName[nextQuest.name] : "",
                        m_objectiveBackupName.ContainsKey(nextQuest.name) ? m_objectiveBackupName[nextQuest.name] : "");
        }
      }
      else
      {
        // show message, can't cancel
        var enableObjects = GetComponentInChildren<EnableObjectForSeconds>();
        enableObjects.ShowObjects("Not In Quest!");
      }
    }
  }

  // I apology for this messed up function
  public void ActivateQuest1_1()
  {
    ActivateQuest("Quest1-1", m_questBackupName["Quest1-1"], m_objectiveBackupName["Quest1-1"]);
  }
  public void ActivateQuest11()
  {
    ActivateQuest("Quest11", m_questBackupName["Quest11"], m_objectiveBackupName["Quest11"]);
  }
  public void ActivateQuest14()
  {
    ActivateQuest("Quest14", m_questBackupName["Quest14"], m_objectiveBackupName["Quest14"]);
  }
  public void ActivateQuest16()
  {
    ActivateQuest("Quest16", m_questBackupName["Quest16"], m_objectiveBackupName["Quest16"]);
  }
  public void ActivateQuest18()
  {
    ActivateQuest("Quest18", m_questBackupName["Quest18"], m_objectiveBackupName["Quest18"]);
  }
  public void ActivateQuest21()
  {
    ActivateQuest("Quest21", m_questBackupName["Quest21"], m_objectiveBackupName["Quest21"]);
  }
  public void ActivateQuest24()
  {
    ActivateQuest("Quest24", m_questBackupName["Quest24"], m_objectiveBackupName["Quest24"]);
  }
  public void ActivateQuest26()
  {
    ActivateQuest("Quest26", m_questBackupName["Quest26"], m_objectiveBackupName["Quest26"]);
  }
  public void ActivateQuest28()
  {
    ActivateQuest("Quest28", m_questBackupName["Quest28"], m_objectiveBackupName["Quest28"]);
  }
  public void ActivateQuest30()
  {
    ActivateQuest("Quest30", m_questBackupName["Quest30"], m_objectiveBackupName["Quest30"]);
  }
  public void ActivateQuest33()
  {
    ActivateQuest("Quest33", m_questBackupName["Quest33"], m_objectiveBackupName["Quest33"]);
  }
  public void ActivateQuest34()
  {
    ActivateQuest("Quest34", m_questBackupName["Quest34"], m_objectiveBackupName["Quest34"]);
  }
  // but it's too late to apology.

  public void ActivateQuest(string questName, string backupQuestName = "", string backupObjective = "")
  {
    if (QuestManager.Instance != null)
    {
      if (QuestManager.Instance.GetCurrentActiveQuest() != null)
      {
        // please quit the quest first!
        var enableObjects = GetComponentInChildren<EnableObjectForSeconds>();
        enableObjects.ShowObjects("Cancel or complete the current quest first!");
      }
      else
      {
        Quest q = QuestManager.Instance.GetQuest(questName);
        if (q.IsComplete)
        {
          var enableObjects = GetComponentInChildren<EnableObjectForSeconds>();
          enableObjects.ShowObjects("The quest is already completed!");
        }
        else
        {
          Quest[] quests = QuestManager.Instance.GetAllActiveQuests();
          foreach (var quest in quests)
          {
            if (quest.name != "MCreateBot" && quest.name != "MLevelUpBot" && quest.name != questName)
            {
              quest.gameObject.SetActive(false);
            }
          }
          QuestManager.Instance.BackupQuestName = backupQuestName;
          QuestManager.Instance.BackupObjective = backupObjective;
          q.gameObject.SetActive(true);
          if (SignalManager.QuestChanged != null)
            SignalManager.QuestChanged(q);
          if (SignalManager.ObjectiveChanged != null)
            SignalManager.ObjectiveChanged(null);
          Debug.Log("Enable quest: " + q.name, this);
        }
      }
    }
  }

  public void ActivateLucasSideQuest()
  {
    if (QuestManager.Instance != null)
    {
      if (QuestManager.Instance.GetCurrentActiveQuest() != null)
      {
        // please quit the quest first!
        var enableObjects = GetComponentInChildren<EnableObjectForSeconds>();
        enableObjects.ShowObjects("Cancel or complete the current quest first!");
      }
      else
      {
        Quest createBotQuest = QuestManager.Instance.GetQuest("MCreateBot");
        Quest levelUpBotQuest = QuestManager.Instance.GetQuest("MLevelUpBot");
        createBotQuest.gameObject.SetActive(true);
        levelUpBotQuest.gameObject.SetActive(true);
      }
    }
  }

  public void SyncLockButtons()
  {
    var lockButtons = GetComponentsInChildren<DebugLockButton>(true);
    foreach (var button in lockButtons)
    {
      button.SyncLock();
    }
  }

  public void LoadFromSelectedSave()
  {
    int selectedNum = GetSelectedSaveNum();
    if (selectedNum == SessionManager.NONE_SELECT) return;
    if (SessionManager.Instance == null) return;
    if (!SessionManager.Instance.IsSaveExists(null, selectedNum)) return;
    //SessionManager.Instance.DebugLoad(selectedNum);
    SessionManager.DebugLoadFlag = selectedNum;
    Application.LoadLevel(Application.loadedLevelName);
    if (OpenButton != null) OpenButton.Close();
    StartCoroutine(WaitForSessionManagerToReload(selectedNum));
  }

  IEnumerator WaitForSessionManagerToReload(int selectedNum)
  {
    while (SessionManager.DebugLoadFlag != SessionManager.NONE_SELECT)
    {
      yield return null;
    }
    CurrentSaveNum = selectedNum;
    UpdateAllSaveSlots();
  }

  public void ShowInputNotePanel()
  {
    int selectedNum = GetSelectedSaveNum();
    if (selectedNum == SessionManager.NONE_SELECT) return;
    if (SessionManager.Instance == null) return;
    if (InputNotePanel != null)
      InputNotePanel.SetActive(true);
  }

  public void HideInputNotePanel()
  {
    if (InputNote != null)
      InputNote.value = "";
    if (InputNotePanel != null)
      InputNotePanel.SetActive(false);
  }

  public void SaveToSelectedSave()
  {
    string note = "";
    if (InputNote != null)
      note = InputNote.value;
    if (note == null) note = "";
    HideInputNotePanel();
    int selectedNum = GetSelectedSaveNum();
    if (selectedNum == SessionManager.NONE_SELECT) return;
    if (SessionManager.Instance == null) return;
    SessionManager.Instance.DebugSave(selectedNum, note);
    CurrentSaveNum = selectedNum;
    UpdateAllSaveSlots();
  }

  public void SendLastAutoSaveToCloud()
  {
    Debug.Log("DERP");
    if (SessionManager.Instance != null)
    {
      PegasusManager.Instance.GLSDK.SaveGame(SessionManager.Instance.GetSaveJSON());
    }
  }

  public void SetSelectedSaveAsDefault()
  {
    int selectedNum = GetSelectedSaveNum();
    if (selectedNum == -1 || selectedNum == SessionManager.NONE_SELECT) return;
    if (SessionManager.Instance == null) return;
    if (!SessionManager.Instance.IsSaveExists() || !SessionManager.Instance.IsSaveExists(null, selectedNum)) return;

    string currentSave = SessionManager.Instance.GetSaveJSON(null, selectedNum);
    string defaultSave = SessionManager.Instance.GetSaveJSON();
    string currentSaveTime = SessionManager.Instance.GetSaveTime(null, selectedNum);
    string defaultSaveTime = SessionManager.Instance.GetSaveTime();
    string currentSaveNote = SessionManager.Instance.GetSaveNote(null, selectedNum);
    string defaultSaveNote = SessionManager.Instance.GetSaveNote();
    SessionManager.Instance.SetSaveJSON(defaultSave, null, selectedNum);
    SessionManager.Instance.SetSaveJSON(currentSave);
    SessionManager.Instance.SetSaveTime(defaultSaveTime, null, selectedNum);
    SessionManager.Instance.SetSaveTime(currentSaveTime);
    SessionManager.Instance.SetSaveNote(defaultSaveNote, null, selectedNum);
    SessionManager.Instance.SetSaveNote(currentSaveNote);
    if (CurrentSaveNum == selectedNum)
      CurrentSaveNum = -1;
    else if (CurrentSaveNum == -1)
      CurrentSaveNum = selectedNum;

    UpdateAllSaveSlots();
  }

  public int GetSelectedSaveNum()
  {
    if (m_saveSelectButtons == null)
      m_saveSelectButtons = GetComponentsInChildren<DebugSaveSelectButton>(true);
    if (m_saveSelectButtons == null) return SessionManager.NONE_SELECT;
    foreach (var choice in m_saveSelectButtons)
    {
      if (choice.Selected)
        return choice.SaveNumber;
    }
    return SessionManager.NONE_SELECT;
  }

  public void UpdateAllSaveSlots()
  {
    var choices = GetComponentsInChildren<DebugSaveSelectButton>(true);
    foreach (var choice in choices)
    {
      choice.UpdateDisplaySaveInfo();
    }
  }
}
