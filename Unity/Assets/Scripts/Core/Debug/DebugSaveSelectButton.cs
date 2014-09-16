using UnityEngine;
using System.Collections;
using GlassLab.Core.Serialization;

public class DebugSaveSelectButton : MonoBehaviour
{
  public GameObject SaveManager;
  public UILabel Label;
  public int SaveNumber = -1;

  public const string NO_SAVE_FOUND = "NO SAVE FOUND";
  public const string NO_SESSIONMANAGER = "NO SESSIONMANAGER";

  private UISprite m_background;
  private bool m_selected = false;
  public bool Selected
  {
    get{return m_selected;}
    set{m_selected = value;}
  }

  void Awake ()
  {
    m_background = GetComponentInChildren<UISprite>();
  }

  // Use this for initialization
  void Start ()
  {
  
  }
  
  // Update is called once per frame
  void Update ()
  {
  
  }

  void OnEnable()
  {
    UpdateDisplaySaveInfo();
  }

  void OnDisable()
  {
    UpdateDisplaySaveInfo();
  }

  public void Click ()
  {
    if (enabled) {
      ChooseMe();
    }
  }

  void ChooseMe()
  {
    DebugSaveSelectButton[] choices;
    if (SaveManager != null)
    {
      choices = SaveManager.GetComponentsInChildren<DebugSaveSelectButton>(true);
    }
    else
    {
      GameObject debugManager = GameObject.FindGameObjectWithTag("DebugManager");
      choices = debugManager.GetComponentsInChildren<DebugSaveSelectButton>(true);
    }
    foreach (var choice in choices)
    {
      if (choice.gameObject.Equals(gameObject))
      {
        choice.Selected = true;
        choice.UpdateDisplaySaveInfo();
        choice.ChangeBackgroundColor(true);
      }
      else 
      {
        choice.Selected = false;
        choice.UpdateDisplaySaveInfo();
        choice.ChangeBackgroundColor(false);
      }
    }
  }

  public void ChangeBackgroundColor(bool selected)
  {
    if (m_background == null) return;
    if (selected) m_background.color = new Color(0f/255f, 90f/255f, 200f/255f, 160f/255f);
    else m_background.color = new Color(0, 0, 0, 160f/255f);
  }

  public void UpdateDisplaySaveInfo()
  {
    if (Label == null) return;
    if (SessionManager.Instance == null)
    {
      Label.text = NO_SESSIONMANAGER;
      SetButtonWhite();
      AdjustLabelColliderSize(Label);
      return;
    }
    string accountName = AccountManager.InstanceOrCreate.GetCurrentAccount();
    if (SessionManager.Instance.IsSaveExists(accountName, SaveNumber))
    {
      string saveTime = SessionManager.Instance.GetSaveTime(accountName, SaveNumber);
      string saveNote = SessionManager.Instance.GetSaveNote(accountName, SaveNumber);
      if (saveNote != null)
        saveNote.Replace(System.Environment.NewLine, " ").Replace("\r", " ").Replace("\n", " ");
      //Debug.Log(SaveNumber + " save note: " + saveNote);
      Label.text = accountName + " @ " + saveTime + "\n" + saveNote;
    }
    else
    {
      Label.text = NO_SAVE_FOUND;
    }
    UpdateButtonColor();
    AdjustLabelColliderSize(Label);
  }

  public void UpdateButtonColor()
  {
    UILabel buttonLabel = GetComponent<UILabel>();
    if (buttonLabel == null) return;
    buttonLabel.color = (DebugSystemManager.Instance.CurrentSaveNum == SaveNumber) ? Color.red : Color.white;
  }

  public void SetButtonWhite()
  {
    UILabel buttonLabel = GetComponent<UILabel>();
    if (buttonLabel == null) return;
    buttonLabel.color = Color.white;
  }

  public void AdjustLabelColliderSize(UILabel label)
  {
    Collider c = label.gameObject.GetComponent<Collider>();
    if (label != null && c != null)
    {
      if (label.autoResizeBoxCollider)
      {
        BoxCollider box = c as BoxCollider;
        if (box != null)
          NGUITools.UpdateWidgetCollider(box, true);
      }
    }
  }

  void OnClick () // NGUI
  {
    Click ();
  }
  
  void OnMouseUpAsButton () // non-NGUI
  {
    Click ();
  }
}
