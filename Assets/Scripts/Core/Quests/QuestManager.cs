using UnityEngine;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using System.Collections;

public class QuestManager : SingletonBehavior<QuestManager> {

  // HACK
  public ArgubotSchemes CurrentTestedScheme = ArgubotSchemes.NONE;
  private List<FindEquipment> m_schemeTestFindComponents;

  public void SetTestSchemeComponents(List<FindEquipment> components)
  {
    if (m_schemeTestFindComponents != null)
    {
      for (int i=m_schemeTestFindComponents.Count-1; i>=0; i--)
      {
        Destroy(m_schemeTestFindComponents[i]);
      }
    }

    m_schemeTestFindComponents = components;
  }

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

  private List<EquipableModel> m_givenItems = new List<EquipableModel>();
  private EquipableModel m_currentPopupItem;

  public Chapter StartingChapter;

  // Keep seperate dictionaries of data by type
  [PersistAttribute]
  private Dictionary<string, string> m_dialogueLuaStringData = new Dictionary<string, string>();
  [PersistAttribute]
  private Dictionary<string, int> m_dialogueLuaIntData = new Dictionary<string, int>();

  private readonly string[] persistedLuaStringVariables = {
    "progress",
    "VideoGamePosition",
    "QuestComplete_Quest11",
    "QuestComplete_Quest24",
    "QuestComplete_Quest0-6"
  };

  private readonly string[] persistedLuaIntVariables = {
    "RenRelationship",
    "AdrianRelationship",
    "MayaRelationship",
    "DaraRelationship",
    "SAMRelationship",
    "ChloeRelationship",
    "TelosRelationship",
    "DeanRelationship",
    "LilakaiRelationship"
  };

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

  private void onDialogueLine(Subtitle s)
  {
    string questName = DialogueLua.DoesVariableExist("StartQuest") ? DialogueLua.GetVariable("StartQuest").AsString : "";
    if (questName != "")
    {
      StartQuestByName(questName);
      //DialogueLua.SetVariable("StartQuest", "");
      Lua.Run("Variable['StartQuest']=''");
    }

    int itemID = DialogueLua.DoesVariableExist("GiveItem") ? DialogueLua.GetVariable("GiveItem").AsInt : -1;
    if (itemID != -1)
    {
      if (!EquipmentManager.Instance.HasEquipment(itemID))
      {
        EquipableModel item = EquipableModel.GetModel(itemID);
        if (item != null) {
          Debug.Log("[QuestManager] Adding GiveItem "+itemID, this);
          m_givenItems.Add(item); // add it to the list to display at the end of the conversation
        } else {
          Debug.LogWarning("[QuestManager] Can't add item "+itemID+" because it's not a valid id.", this);
        }
      }
      else 
      {
        Debug.Log("[QuestManager] Equipment already exists, aborting add of item "+itemID+".", this);
      }

      //DialogueLua.SetVariable("GiveItem", -1);
      Lua.Run("Variable['GiveItem']=-1");
    }
    
    string roomName = DialogueLua.DoesVariableExist("UnlockRoom") ? DialogueLua.GetVariable("UnlockRoom").AsString : "";
    if (roomName != "")
    {
      ExplorationManager.Instance.UnlockRoom(roomName);
      //DialogueLua.SetVariable("UnlockRoom", "");
      Lua.Run("Variable['UnlockRoom']=''");
    }
  }

  private void onConversationEnd(int id) {
    // Delay the function where we check for OPEN_BATTLE, etc
    // Since we've already sent out "DIALOGUE_END" event this frame, wait until the next to send out other events.
    StartCoroutine(conversationEndEvents());
  }

  private IEnumerator conversationEndEvents()
  {
    yield return null;

    int battleIndex = DialogueLua.DoesVariableExist("StartBattle") ? DialogueLua.GetVariable("StartBattle").AsInt : -1;
    if (battleIndex != -1)
    {
      GlobalData.SetValue<int>("OPPONENT_ID", battleIndex);
      //DialogueLua.SetVariable("StartBattle", -1);
      Lua.Run("Variable['StartBattle']=-1");

      // have to send StartQuestBattle and then OpenBattle so that pickupBattle's OpenBattle doesn't advance the quest
      PlayMakerFSM.BroadcastEvent("START_QUEST_BATTLE");
      yield return null;
      PlayMakerFSM.BroadcastEvent("OPEN_BATTLE");

      return false;
    }

    int coreConstructionIndex = DialogueLua.DoesVariableExist("StartCoreConstruction") ? DialogueLua.GetVariable("StartCoreConstruction").AsInt : -1;
    if (coreConstructionIndex != -1)
    {
      GlobalData.SetValue<int>("CC_TRAINING_ID", coreConstructionIndex);
      //DialogueLua.SetVariable("StartCoreConstruction", -1);
      Lua.Run("Variable['StartCoreConstruction']=-1");
      
      PlayMakerFSM.BroadcastEvent("OPEN_CORECONSTRUCTION");
      return false;
    }

    if (DialogueLua.DoesVariableExist("CancelQuest") && DialogueLua.GetVariable("CancelQuest").AsBool) {
      if (m_currentActiveQuest != null && m_currentActiveQuest.IsCancelable) {
        m_currentActiveQuest.Cancel();
        Debug.Log ("[QuestManager] Canceling current quest after a dialogue script!");
      } else {
        Debug.LogWarning ("[QuestManager] Want to cancel current quest from dialogue, but it doesn't exist or it's not cancelable!");
      }

      DialogueLua.SetVariable("CancelQuest", false);
    }

    // Possibly unlock a new bot -- removing this b/c we don't have to unlock bots this way
    /*
    if (DialogueLua.DoesVariableExist("UnlockBot")) {
      ArgubotSchemes scheme = (ArgubotSchemes) DialogueLua.GetVariable("UnlockBot").AsInt;
      if (!EquipmentManager.Instance.UnlockedSchemes.Contains(scheme)) { // if we haven't unlocked this bot yet
        Debug.Log ("Unlock bot "+scheme);
        EquipmentManager.Instance.UnlockScheme(scheme);
      }
    }
    */

    // If we got any new evidence, display it now.
    // Please note that this isn't guaranteed to work with any other events including unlocking a bot
    if (m_givenItems.Count > 0) {
      Debug.Log ("[QuestManager] Giving "+m_givenItems.Count+" items.");
      GiveNextItem();
    }

    // Save after a conversation in case any variables changed
    SessionManager.Instance.Save();
  }

  private void GiveNextItem(FindItemPopup popup = null)
  {    
    Debug.Log ("[QuestManager] GiveNextItem with "+m_givenItems.Count+" items");
    // open the popup for the first item from the list of items still to give
    m_currentPopupItem = m_givenItems[0];
    m_givenItems.RemoveAt(0); // since this one has been added, we can remove it

    // record that we discovered this item, even if we don't take it
    EquipmentManager.Instance.LogEquipmentDiscovered(m_currentPopupItem);

    if (popup == null) popup = FindItemPopup.Instance;
    if (popup != null) {
      Fabric.EventManager.Instance.PostEvent("tap_evidence_discovered");

      // For now set the popup title, description, and sprite directly so we don't have to set it in the inspector
      popup.Show (FindEquipment.EVIDENCE_DISCOVERED_TEXT, m_currentPopupItem.Description, "environmentItem_dataChip");
      popup.Destination = FindItemPopup.SendItemDestinations.EQUIPMENT;

      popup.ModelId = m_currentPopupItem.Id; // kinda hacky b/c we need this info for telemetry

      popup.OnKeep += KeepEquipment; // wait to add the equipment until they choose to keep it
      // note that the popup clears its own OnKeep so we won't try to add this equipment again

      popup.OnClose -= DelayGiveNextItem; // remove the previous event handlers

      // if we have more, set a callback to give them next
      if (m_givenItems.Count > 0) {
        Debug.Log ("[QuestManager] Showed a popup, now setting callback for "+m_givenItems.Count+" more.");
   
        popup.OnClose += DelayGiveNextItem;
      }
    }
  }

  void DelayGiveNextItem(FindItemPopup popup = null) {
    Debug.Log ("[QuestManager] Delay give next item to try to avoid having it animate away immediately");
    //Utility.Delay(GiveNextItem, 1f);
    GiveNextItem (popup);
  }
 
  void KeepEquipment(FindItemPopup popup) {
    Debug.Log ("[QuestManager] Keeping equipment from popup "+popup);
    EquipmentManager.InstanceOrCreate.Add ( m_currentPopupItem );
  }

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

  public void SignalNPCChallenged()
  {
    m_currentActiveQuest.Trigger("BATTLE_START");
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

  public void SetLuaVariable(string key, string value) {
    if (!DialogueLua.DoesVariableExist(key)) Debug.LogWarning("Setting Lua variable "+key+" which doesn't already exist!");

    Lua.Run("Variable[\""+key+"\"]=\""+value+"\"");
    Debug.Log ("[QuestManager] Setting "+key+" to "+value+" (string): "+DialogueLua.GetVariable(key).AsString);
  }

  void OnSave()
  {
    for (int i=persistedLuaStringVariables.Length-1; i>=0; i--)
    {
      string key = persistedLuaStringVariables[i];
      if (DialogueLua.DoesVariableExist(key))
      {
        m_dialogueLuaStringData[key] = DialogueLua.GetVariable(key).AsString;
        //Debug.Log ("[QuestManager] Setting "+key+": "+DialogueLua.GetVariable(key).AsString+" into m_dialogueLuaStringData");
      } else {
        //Debug.Log ("[QuestManager] Couldn't find key "+key+" (string) in DialogueLua!");
      }
    }

    for (int i=persistedLuaIntVariables.Length-1; i>=0; i--)
    {
      string key = persistedLuaIntVariables[i];
      if (DialogueLua.DoesVariableExist(key))
      {
        m_dialogueLuaIntData[key] = DialogueLua.GetVariable(key).AsInt;
      } else {
        Debug.LogWarning ("[QuestManager] Couldn't find key "+key+" (int) in DialogueLua!");
      }
    }

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
    SetTestSchemeComponents(null); // Clear any previous components used to test schemes
    
    m_currentActiveQuest = null;
    refreshLua();
    m_activeQuests.Remove(q.name);
  }

  private void onQuestStarted(Quest q)
  {
    m_currentActiveQuest = q;
    
    refreshLua();
    if (m_activeQuests.Contains(q.name))
    {
      Debug.LogWarning("[QuestManager] Got a quest start event from a quest that's already started", this);
      return;
    }
    m_activeQuests.Add(q.name);
  }

  private void refreshLua()
  {
    string quest = (m_currentActiveQuest == null)? "none" : m_currentActiveQuest.name;
    Lua.Run("Variable['CurrentQuest']='"+quest+"'");
  }

  private void onQuestCompleted(Quest q)
  {
    SetTestSchemeComponents(null); // Clear any previous components used to test schemes
    m_completedQuests.Add (q.name);

    // if we have a variable to track when this quest is complete, update it
    string var = "QuestComplete_"+q.name;
    if (DialogueLua.DoesVariableExist(var)) {
      DialogueLua.SetVariable(var, "true");
      Debug.Log ("[QuestManager] Setting "+var+" to true since we finished the quest.", q);
    }

    if (q == m_currentActiveQuest)
      m_currentActiveQuest = null;

    refreshLua();

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

    SignalManager.ConversationLine += onDialogueLine;
    SignalManager.ConversationEnded += onConversationEnd;

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

  void Start()
  {
    refreshLua();

    // Re-set dialogue Lua variables
    for (int i=persistedLuaStringVariables.Length-1; i>=0; i--)
    {
      string key = persistedLuaStringVariables[i];
      string val = (m_dialogueLuaStringData.ContainsKey(key))? m_dialogueLuaStringData[key] : "";
      Lua.Run("Variable[\""+key+"\"]=\""+val+"\"");
      //Debug.Log ("[QuestManager] Setting "+key+" to "+val+" (string): "+DialogueLua.GetVariable(key).AsString);
    }

    for (int i=persistedLuaIntVariables.Length-1; i>=0; i--)
    {
      string key = persistedLuaIntVariables[i];
      int val = (m_dialogueLuaIntData.ContainsKey(key))? m_dialogueLuaIntData[key] : 0;
      Lua.Run("Variable[\""+key+"\"]="+val);
      //Debug.Log ("[QuestManager] Setting "+key+" to "+val+" (int): "+DialogueLua.GetVariable(key).AsInt);
    }
  }

  protected override void OnDestroy()
  {
    SignalManager.QuestCompleted -= onQuestCompleted;
    SignalManager.QuestCanceled -= onQuestCanceled;
    SignalManager.QuestStarted -= onQuestStarted;

    SignalManager.ConversationLine -= onDialogueLine;
    SignalManager.ConversationEnded -= onConversationEnd;
  }
}
