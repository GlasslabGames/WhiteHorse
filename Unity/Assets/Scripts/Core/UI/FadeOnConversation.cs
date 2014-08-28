using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;

public class FadeOnConversation : MonoBehaviour {
  public bool VisibleDuringConversation = false;
  public float Duration = 0.25f;

  public event System.Action FadeComplete;

  private Dictionary<GameObject, Color> m_conversationColors = new Dictionary<GameObject, Color>();
  private Dictionary<GameObject, Color> m_normalColors = new Dictionary<GameObject, Color>();

  void Awake() {
    foreach (SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>(true)) {
      m_conversationColors.Add (sprite.gameObject, (VisibleDuringConversation)? sprite.color : ToTransparent(sprite.color));
      m_normalColors.Add (sprite.gameObject, (!VisibleDuringConversation)? sprite.color : ToTransparent(sprite.color));
    }
    foreach (UISprite sprite in GetComponentsInChildren<UISprite>(true)) {
      m_conversationColors.Add (sprite.gameObject, (VisibleDuringConversation)? sprite.color : ToTransparent(sprite.color));
      m_normalColors.Add (sprite.gameObject, (!VisibleDuringConversation)? sprite.color : ToTransparent(sprite.color));
    }
  }
  
  void Start() {
    // Add any that we missed (e.g. because the object was added, like for the alert bubbles)
    foreach (SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>(true)) {
      if (!m_conversationColors.ContainsKey(sprite.gameObject)) {
        m_conversationColors.Add (sprite.gameObject, (VisibleDuringConversation)? sprite.color : ToTransparent(sprite.color));
        m_normalColors.Add (sprite.gameObject, (!VisibleDuringConversation)? sprite.color : ToTransparent(sprite.color));
      }
    }
    foreach (UISprite sprite in GetComponentsInChildren<UISprite>(true)) {
      if (!m_conversationColors.ContainsKey(sprite.gameObject)) {
        m_conversationColors.Add (sprite.gameObject, (VisibleDuringConversation)? sprite.color : ToTransparent(sprite.color));
        m_normalColors.Add (sprite.gameObject, (!VisibleDuringConversation)? sprite.color : ToTransparent(sprite.color));
      }
    }
  }

  private Color ToTransparent(Color c) {
    c.a = 0f;
    return c;
  }

  void OnEnable () {
    SignalManager.ConversationStarted += OnConversationStarted;
    SignalManager.ConversationEnded += OnConversationEnded;

    // If we've just been enabled during a conversation, immediately go to conversation colors
    if (DialogueManager.IsConversationActive) TweenAll(m_conversationColors, 0f, 0f, false);
  }
  
  void OnDisable () {
    SignalManager.ConversationStarted -= OnConversationStarted;
    SignalManager.ConversationEnded -= OnConversationEnded;
  }
  
  public void OnConversationStarted(int id) {
    TweenAll(m_conversationColors, Duration);
  }
  
  public void OnConversationEnded(int id) {
    TweenAll(m_normalColors, Duration);
  }

  void TweenAll(Dictionary<GameObject, Color> toDict, float duration, float delay = 0.01f, bool sendEvent = true) {
    TweenColor t = null;
    foreach (GameObject obj in toDict.Keys) {
      t = TweenColor.Begin(obj.gameObject, duration, toDict[obj]);
      t.delay = delay; // slight delay to avoid a weird bug where it wouldn't fade out
    }
    if (t != null && sendEvent) { // attach the finished event only to the last tween
      EventDelegate.Add ( t.onFinished, OnFadeComplete, true );
    }
  }

  void OnFadeComplete() {
    if (FadeComplete != null) FadeComplete();
  }
}
