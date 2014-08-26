using UnityEngine;
using System.Collections;

/// <summary>
/// Starts a quest on the attached questGiver.
/// </summary>
using PixelCrushers.DialogueSystem;


public class StartQuest : Interaction {
  private QuestGiver[] m_questGiverComponents;

  protected override void Reset() {  // sets the default in the inspector
    base.Reset ();
    Properties.OnceOnly = true;
    Properties.Priority = InteractionProperties.Priorities.HIGH;
  }
  
  protected override void Awake() {
    base.Awake();

    m_questGiverComponents = transform.parent.GetComponentsInChildren<QuestGiver>(true);
    if (m_questGiverComponents.Length == 0) {
      Debug.LogError(name + " has a Start Quest Interaction but no Quest Giver!", this);
    }
  }

  public override void Do() {
    Debug.Log ("Start quest with "+name);
    for (int i=m_questGiverComponents.Length-1; i>=0; i--)
    {
      QuestGiver m_questGiver = m_questGiverComponents[i];
      if (m_questGiver.CanStartQuest ())
      {
#if FABRIC
        Fabric.EventManager.Instance.PostEvent("InputSFXGroup/ExclamationPointTap");
#endif

        if (m_questGiver.IntroConversation != null && m_questGiver.IntroConversation != "")
        {
          // Start conversation instead. NOTE: This relies on the conversation to start the quest instead!
          if (m_questGiver.LuaCode != null && m_questGiver.LuaCode != "")
            Lua.Run(m_questGiver.LuaCode);
          GLDialogueManager.Instance.StartConversation (m_questGiver.IntroConversation, null);
        }
        else
        {
          QuestManager.Instance.GetQuest (m_questGiver.QuestName).StartQuest ();
        }

        base.Do ();
      }
      else
      {
        Debug.Log ("[StartQuest] Couldn't start quest because QuestGiver can't start quest");
      }
    }
  }

  public override bool IsPossible() {
    bool questGiverActive = false;
    for (int i=m_questGiverComponents.Length-1; i>=0; i--)
    {
      QuestGiver m_questGiver = m_questGiverComponents[i];
      if (m_questGiver.Active && m_questGiver.CanStartQuest())
      {
        questGiverActive = true;
        break;
      }
    }
    return questGiverActive && base.IsPossible();
  }
}
