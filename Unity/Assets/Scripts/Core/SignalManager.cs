using UnityEngine;
using PixelCrushers.DialogueSystem;
using System;

public delegate void NameEvent(string id);
public delegate void IDEvent(int id);
public delegate void BoolEvent(bool isTrue);
public delegate void BattleEvent(bool victory);
public delegate void DialogueSubtitleEvent(Subtitle subtitle);
public delegate void DragDropEvent(GLDragDropItem item);
public delegate void QuestEvent(Quest q);
public delegate void ObjectiveEvent(Objective o);
public delegate void ChapterEvent(Chapter c);
public delegate void SceneLoadedEvent(string name, GameObject obj); // string => sceneName, GameObject => sceneObject
public delegate void RoomEvent(Room r);
public delegate void GameObjectEvent(GameObject go);

public delegate void ToggleEvent(bool b);

// TODO: Many of these signals are global. They could be made more targetted for performance purposes.
public class SignalManager : SingletonBehavior<SignalManager> {
  // CONVERSATION
  public static IDEvent ConversationEnded;
  //public static void ConversationEnded (string id) { if (onConversationEnded) onConversationEnded(id); }
  public static IDEvent ConversationStarted;
  public static DialogueSubtitleEvent ConversationLine;

  // COMBAT
  public static NameEvent CombatStarted;
  public static BattleEvent CombatEnded;

  // CORE CONSTRUCTION
  public static BoolEvent CoreConstructionPuzzleAttempted; // True for success, false for not
  public static BoolEvent CoreConstructionCompleted; // True for all puzzles completed, false for not
  public static IDEvent CoreConstructionPuzzleLoaded;

  // CORE EQUIP
  public static BoolEvent CoreEquipFuse;
  public static Action CoreEquipClosed;

  // INVENTORY
  public static Action EquipmentChanged; // Equipment in inventory has changed somehow

  // Bot Storage
  public static BoolEvent BotIntoStorage;

  // UI
  public static Action UIRefresh;
  public static DragDropEvent ItemDragStarted;
  public static DragDropEvent ItemDragStopped;

  // QUEST
  public static QuestEvent QuestChanged;
  public static QuestEvent QuestStarted;
  public static QuestEvent QuestCanceled;
  public static QuestEvent QuestCompleted;
  public static QuestEvent QuestStateChanged;
  public static ObjectiveEvent ObjectiveChanged;
  public static ObjectiveEvent ObjectiveCompleted;

  // CHAPTER
  public static ChapterEvent ChapterChanged;

  // PAUSE
  public static ToggleEvent Paused;

  // SCENES
  public static SceneLoadedEvent SceneLoaded;
  public static NameEvent RoomChanged;
  public static RoomEvent RoomStateChanged;
  public static RoomEvent RoomLoaded;
  public static GameObjectEvent ObjectTapped;

  // SESSIONS
  public static Action NewGameStarted;

  // ACHIEVEMENTS
  public static AchievementEvent AchievementUnlocked;

  // Exploration
  public static Action EquipmentTrayOpened;
  public static Action EquipmentTrayClosed;
}