using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Defines appearance for a button with states up, down, disabled, and selected by swapping the NGUI sprite name.
/// </summary>

[RequireComponent(typeof(Collider))]
public class SpriteSwapButton : MonoBehaviour {
  private List<UISprite> m_sprites = new List<UISprite>();
  private List<UILabel> m_labels = new List<UILabel>();

  public string m_upString = "_up";
  public string m_downString = "_down";
  public string m_selectedString = "_selected";
  public string m_disabledString = "_disabled";
  private string m_replaceString;

  public bool m_colorSpritesInstead;

  // Alternatively, dim the sprite
  public bool m_dimOnDisable;

  public Color m_upColor;
  public Color m_downColor;
  public Color m_selectedColor;
  public Color m_disabledColor;

  // conveniently identify a label that we want to edit from some other code
  public UILabel m_targetLabel;

  private bool m_enabled = true;
  public bool Enabled {
    set {
      m_enabled = value;
      collider.enabled = value;
      if (!value) { SetDisabled (); }
      else { SetUp (); }
    }
    get { return m_enabled; }
  }

  private bool m_selected = false;
  public bool Selected {
    set {
      m_selected = value;
      if (!value) { SetUp (); }
      else { SetSelected (); }
    }
    get { return m_selected; }
  }


	// Use this for initialization
	void Awake () {
    m_sprites = new List<UISprite>( GetComponentsInChildren<UISprite>() );
    m_labels = new List<UILabel>( GetComponentsInChildren<UILabel>() );
    // Filter out sprites and labels that aren't me or my direct children

    m_sprites = m_sprites.FindAll(delegate (UISprite sprite) {
      return (sprite.transform == transform || sprite.transform.parent == transform);
    });

    m_labels = m_labels.FindAll(delegate (UILabel label) {
      return (label.transform == transform || label.transform.parent == transform);
    });

    m_replaceString = m_upString;
    if (m_downString.Length > 0) { m_replaceString += "|" + m_downString; }
    if (m_disabledString.Length > 0) { m_replaceString += "|" + m_disabledString; }
    if (m_selectedString.Length > 0) { m_replaceString += "|" + m_selectedString; }

    // Ensure that our state matches whether we're supposed to be enabled or not
    Enabled = Enabled;
	}

  public void SetUp() {
    foreach (UILabel label in m_labels) { label.color = m_upColor; }
    foreach (UISprite sprite in m_sprites) {
      if (m_dimOnDisable) { sprite.alpha = 1; }
      if (m_colorSpritesInstead) sprite.color = m_upColor;
      else Replace (sprite, m_upString);
    }
  }

  public void SetDown() {
    foreach (UILabel label in m_labels) { label.color = m_downColor; }
    foreach (UISprite sprite in m_sprites) {
      if (m_dimOnDisable) { sprite.alpha = 1; }
      if (m_colorSpritesInstead) sprite.color = m_downColor;
      else Replace (sprite, m_downString);
    }
  }

  public void SetDisabled() {
    foreach (UILabel label in m_labels) { label.color = m_disabledColor; }
    foreach (UISprite sprite in m_sprites) {
      if (m_dimOnDisable) { sprite.alpha = 0.5f; }
      if (m_colorSpritesInstead) sprite.color = m_disabledColor;
      else Replace (sprite, m_disabledString);
    }
  }

  public void SetSelected() {
    foreach (UILabel label in m_labels) { label.color = m_selectedColor; }
    foreach (UISprite sprite in m_sprites) {
      if (m_dimOnDisable) { sprite.alpha = 1; }
      if (m_colorSpritesInstead) sprite.color = m_selectedColor;
      else Replace (sprite, m_selectedString);
    }
  }
  
  void OnPress(bool pressed) {
    if (Enabled) {
      if (pressed) {
        SetDown ();
      } else {
        if (Selected) { SetSelected(); }
        else { SetUp (); }
      }
    }
  }
	
  public void Replace(UISprite sprite, string replace) {
    sprite.spriteName = Regex.Replace(sprite.spriteName, m_replaceString, replace);
  }
}
