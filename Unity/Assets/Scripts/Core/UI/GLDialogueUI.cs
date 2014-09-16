using UnityEngine;
using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.NGUI;
using GlassLab.Core.Serialization;


public class GLDialogueUI : NGUIDialogueUI
{
  public UITable HistoryTable;
  public UIScrollView HistoryScrollView;

  public bool Animate;
  private bool m_updateTable = false;

  public TweenScale PopupTween;
  public bool TweenChildInstead;
  
  public GLTexture NpcPortrait;
  public GLTexture PcPortrait;

  public bool SetListenerToNeutral = true;

  private float m_tapTime;
  public float TapWaitTime;

  private Transform m_prevPanel;

  /**
   * Depending on whether the player is male or female, these are the sprites to use.
   * NOTE: The order must be the same as the declaration order in AvatarType (excluding AvatarType.NONE).
   */
  private string PlayerName = "zodiac";
  private string[] PlayerReplaceNames = new string[] {
    "malziak",
    "femziak"
  };

  public string DialogueClickSoundEvent = "DialogueContinue";

  override public void Awake()
  {
    base.Awake();

    if (DialogueManager.IsConversationActive)
    {
      Debug.LogError("There shouldn't be any dialogues happening already. Canceling them...", this);
      DialogueManager.Instance.StopConversation();
    }

  }

  override public void Start() {
    base.Start();
  }

  override public void Open() {
    base.Open();
  }

  override public void Close() {
    base.Close();

    // Clear existing dialogue history
    if (HistoryTable != null) {
      List<Transform> children = HistoryTable.GetChildList();
      foreach (Transform child in children) {
        if (child != null) Destroy(child.gameObject);
      }
    }
  }

  void OnEnable()
  {
    if (DialogueManager.IsConversationActive)
    {
      Debug.LogError("DialogueManager shouldn't have an active conversation before the UI is enabled!", this);
    }
  }

  void OnDisable()
  {
    if (DialogueManager.IsConversationActive && GLDialogueManager.Instance != null) // null check for if game is shutting down
    {
      Debug.LogWarning("Dialogue was active when GLDialogueUI was disabled. Canceling them...", this);
      DialogueManager.Instance.StopConversation();
    }
  }
  
  override public void Update() {
    if (m_updateTable) RefreshTable();
  }

  protected void OnClick() {
    // When you click the background, it continues the dialogue
    OnContinue();
  }

  override public void ShowSubtitle(Subtitle subtitle) {
  	// Write the telemetry event
    if (subtitle.formattedText.text.Length > 0) {
    	PegasusManager.Instance.GLSDK.AddTelemEventValue( "speaker", subtitle.speakerInfo.Name );
    	PegasusManager.Instance.GLSDK.AddTelemEventValue( "content", subtitle.formattedText.text );
      PegasusManager.Instance.AppendDefaultTelemetryInfo();
      PegasusManager.Instance.GLSDK.SaveTelemEvent( "Dialogue_display" );
    }

    if (GlSoundManager.Instance != null) {
      GlSoundManager.Instance.PlaySoundByEvent(DialogueClickSoundEvent, gameObject);
    }

    NGUISubtitleControls subtitleControls = null; // TODO: Not sure if this stuff is correct
    if (subtitle != null)
    {
      if (subtitle.listenerInfo.characterType == CharacterType.NPC)
      {
        subtitleControls = dialogue.pcSubtitle;
      }
      else
      {
        subtitleControls = dialogue.npcSubtitle;
      }
    }

    if (subtitleControls != null) {
      //SetSubtitle(subtitleControls, subtitle);
      subtitleControls.ShowSubtitle(subtitle);

      if (subtitleControls.panel != null && subtitleControls.panel.gameObject.activeInHierarchy) {
        
        // If we found a piece of evidence, indicate it
        int itemID = DialogueLua.DoesVariableExist("GiveItem") ? DialogueLua.GetVariable("GiveItem").AsInt : -1;
        bool hasEvidence = (EquipableModel.GetModel(itemID) != null);

        // Look for the evidence icon, then show or hide it as necessary
        foreach (UISprite sprite in subtitleControls.panel.GetComponentsInChildren<UISprite>()) {
          if (sprite.tag == "Icon") {
            sprite.enabled = hasEvidence;
            if (hasEvidence) { // switch the image depending on whether we've found this evidence yet or not
              if (EquipmentManager.Instance.HasEquipment(itemID)) {
                sprite.spriteName = "evidenceIcon_empty";
              } else {
                sprite.spriteName = "evidenceIcon";
              }
            }
            // refresh all tables
            foreach (UITable table in subtitleControls.panel.GetComponentsInChildren<UITable>(true)) {
              table.enabled = true;
              table.Reposition();
            }
            break;
          }
        }
        
        CopyIntoToHistory(subtitleControls.panel.transform);

      }

      HideSubtitle(subtitle);
    }

    PixelCrushers.DialogueSystem.CharacterInfo npcInfo;
    PixelCrushers.DialogueSystem.CharacterInfo pcInfo;
    bool npcTalking = false;
    bool pcTalking = false;

    if (subtitle.speakerInfo.IsPlayer) {
      npcInfo = subtitle.listenerInfo;
      pcInfo = subtitle.speakerInfo;
      //if (SetListenerToNeutral) DialogueLua.SetVariable("npcMood", "neutral");
      if (SetListenerToNeutral) Lua.Run("Variable['npcMood']='neutral'");
      pcTalking = true;
    } else {
      pcInfo = subtitle.listenerInfo;
      npcInfo = subtitle.speakerInfo;
      //if (SetListenerToNeutral) DialogueLua.SetVariable("pcMood", "neutral");
      if (SetListenerToNeutral) Lua.Run("Variable['pcMood']='neutral'");
      npcTalking = true;
    }

    string npcMood = DialogueLua.GetVariable("npcMood").AsString;
    string pcMood = DialogueLua.GetVariable("pcMood").AsString;

    SetPortrait(NpcPortrait, npcInfo, npcMood, npcTalking);
    SetPortrait(PcPortrait, pcInfo, pcMood, pcTalking);

    if (GlSoundManager.Instance != null) {
      GlSoundManager.Instance.PlaySoundByEvent("InputSFXGroup/ConversationTap");
    }

    if (SignalManager.ConversationLine != null) SignalManager.ConversationLine(subtitle); 
  }

  protected void SetPortrait(GLTexture portrait, PixelCrushers.DialogueSystem.CharacterInfo info, string mood, bool talking) {
    portrait.gameObject.SetActive(true);

    string name = info.Name;

    if (name == " SAM") name = "hairySAM"; // pretty hacky way to make hairy sam show up FIXME
    else name = name.Trim().Split(' ')[0].ToLower();

    if (name == PlayerName)
    {
      AvatarType avatarType = AccountManager.InstanceOrCreate.GetAvatar();
      int avatarNum = (int) avatarType;
      if (avatarNum == 0)
      {
        Debug.LogWarning("[GLDialogueUI] Avatar is set to NONE, defaulting to first avatar", this);
        avatarNum = 1;
      }
      name = PlayerReplaceNames[avatarNum - 1];
    }

    string start = "dialog_" + name + "_";
    string spriteName = start + ((mood.Length > 0)? mood : "neutral");


    if ( talking && portrait.HasSprite( spriteName + "_talking" )) { // try emotion_talking first
      portrait.spriteName = spriteName + "_talking";
    } else if ( portrait.HasSprite( spriteName ) ) { // then prioritize whatever emotion was set
      portrait.spriteName = spriteName;
    } else if ( talking && portrait.HasSprite( start + "neutral_talking" )) { // then if they're talking, try neutral_talking
      portrait.spriteName = start + "neutral_talking";
    } else { // else, just default to neutral
      Debug.LogWarning ("Sprite "+spriteName+" doesn't exist! Using a neutral sprite instead.");
      portrait.spriteName = start + "neutral";
    }
  }

  protected void CopyIntoToHistory(Transform panel) {
    // Copy the panel and move it into place under the history table
    Transform newPanel = Instantiate(panel) as Transform;
    panel.transform.position = panel.transform.position;
    newPanel.parent = HistoryTable.transform;
    newPanel.localScale = Vector3.one;

    // Reposition all tables
    /*
    Utility.Delay( delegate() {
      foreach (UITable table in newPanel.GetComponentsInChildren<UITable>()) {
        table.Reposition();
      }
    }, 2f);
    */

    if (Animate && HistoryTable.transform.childCount > 1) {
      m_updateTable = true;

      // we don't want to tween the whole panel because its center is not in the right place, so tween its first child
      Transform child = (TweenChildInstead)? newPanel.transform.GetChild(0) : newPanel.transform;

      child.localScale = PopupTween.from;

      // Make a new tween that applies to the popup but copies everything about the bubble popup tween
      TweenScale tween = TweenScale.Begin(child.gameObject, PopupTween.duration, PopupTween.to);
      tween.from = PopupTween.from;
      tween.animationCurve = PopupTween.animationCurve;

      EventDelegate.Add(tween.onFinished, delegate() {
        m_updateTable = false;
        RefreshTable();
      });

    }

    AdjustPrevPanel(newPanel);

    RefreshTable();

    int count = HistoryTable.transform.childCount;
    newPanel.name = "Dialogue History " + (1000 - count).ToString("D3"); // reverse numbers so that sort order in table is correct

    // Remove UIPanel so that it gets clipped correctly by the history panel
    Destroy(newPanel.GetComponent<UIPanel>());

    // Add scroll view to the background collider
    GLButton button = newPanel.GetComponentInChildren<GLButton>();
    UIDragScrollView dragScrollView = button.gameObject.AddComponent<UIDragScrollView>();
    dragScrollView.scrollView = HistoryScrollView;
  }

  protected void AdjustPrevPanel(Transform newPanel) {
    if (m_prevPanel != null) {
      // Change the panel background by appending "_gray"
      // May have to refine this later
      foreach (UISprite sprite in m_prevPanel.GetComponentsInChildren<UISprite>()) {
        if (sprite.tag != "Icon") { // if it is the evidence icon, don't change it
          sprite.spriteName += "_gray";
        }
      }
    }
    m_prevPanel = newPanel;
  }

  protected void ResetScroll() {
    HistoryScrollView.SetDragAmount(0, 1, false);
  }
  
  private void RefreshTable() {
    HistoryTable.Reposition();
    HistoryScrollView.GetComponent<UIPanel>().Refresh();
  }

  override public void OnContinue() {
    if (Time.time - m_tapTime < TapWaitTime) return; // Don't advance the dialog if we just tapped
    else m_tapTime = Time.time;

    PegasusManager.Instance.AppendDefaultTelemetryInfo();
  	PegasusManager.Instance.GLSDK.SaveTelemEvent( "Dialogue_advance" );

    ResetScroll();
    base.OnContinue();
  }
}

