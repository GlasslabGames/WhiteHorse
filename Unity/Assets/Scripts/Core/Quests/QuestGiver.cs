using UnityEngine;
using System.Collections;

/**
 * Component responsible for showing the ! above the quest-giving character.
 * 
 * NOTE: Quest is not started in this component. Look in StartQuest.cs
 */
public class QuestGiver : MonoBehaviour {
  private QuestAlert m_exclamationMark;

  public string QuestName;

  public string LuaCode = "";

  public string IntroConversation;

  private Quest m_quest;

  public bool Active { get { return m_exclamationMark.gameObject.activeSelf; } }

  void Awake()
  {
    GameObject obj = (GameObject) Instantiate(Resources.Load ("QuestAlert"));
    m_exclamationMark = obj.GetComponent<QuestAlert>();
    // Try to attach under the Interactions object if we have one
    Transform interactions = transform.Find("Interactions");
    if (interactions != null) m_exclamationMark.transform.parent = interactions;
    else m_exclamationMark.transform.parent = transform;
    m_exclamationMark.transform.localScale = Vector3.one;
    m_exclamationMark.transform.localPosition = Vector3.zero;
  }

  void Start()
  {
    if (QuestManager.Instance == null) return;
    m_quest = QuestManager.Instance.GetQuest (QuestName);
    
    if (m_quest == null)
    {
      Debug.LogError ("[QuestGiver("+gameObject.name+")] Could not find quest: " + QuestName);
    } else
    {
      Refresh();
    }

    // Make the speech bubble trigger the controller (e.g. to start a quest by tapping the speech bubble)
    GLButton button = m_exclamationMark.GetComponent<GLButton>();
    if (button != null) {
      EventDelegate.Add (button.onClick, OnClickExclamationMark);
    }
  }

  void OnClickExclamationMark() {
    // Send up events from the exclamation mark as if we had clicked on this object (so not if the collider's disabled)
    if (collider != null && collider.enabled) {
      gameObject.SendMessage("OnClick", null, SendMessageOptions.DontRequireReceiver); // NGUI
      gameObject.SendMessage("OnMouseUpAsButton", null, SendMessageOptions.DontRequireReceiver); // non-NGUI
    }
  }

  void OnEnable()
  {
    SignalManager.QuestStateChanged += onQuestStateChanged;
    SignalManager.QuestStarted += onQuestStateChanged;
    SignalManager.QuestCanceled += onQuestStateChanged;
    Refresh();
  }

  void OnDisable()
  {
    SignalManager.QuestStateChanged -= onQuestStateChanged;
    SignalManager.QuestStarted -= onQuestStateChanged;
    SignalManager.QuestCanceled -= onQuestStateChanged;
  }

  private void onQuestStateChanged(Quest q)
  {
    if (gameObject.activeInHierarchy)
      StartCoroutine(RefreshNextFrame());
  }

  private IEnumerator RefreshNextFrame()
  {
    yield return null;
    Refresh();
  }

  private void Refresh()
  {
    if (CanStartQuest()) {
      m_exclamationMark.ShowForQuest( m_quest.IsSideQuest );
    } else {
      m_exclamationMark.Hide ();
    }
  }

  public bool CanStartQuest()
  {
    //Debug.Log ("[QuestGiver] CanStartQuest? Current quest: "+QuestManager.Instance.GetCurrentActiveQuest());
    return m_quest != null && QuestManager.Instance != null && m_quest.IsAvailable () && QuestManager.Instance.GetCurrentActiveQuest() == null;
  }
}
