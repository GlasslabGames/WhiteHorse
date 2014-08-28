using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(UISprite))]

/// <summary>
/// Changes the background sprite and the text sizes/colors to indicate which tab is selected.
/// Requires full sprites like "tabBar_tab1", "tabBar_tab2", etc.
/// </summary>
public class TabBar : MonoBehaviour {
  public Color m_activeColor;
  public Color m_inactiveColor;

  public int m_activeTextSize;
  public int m_inactiveTextSize;

  public string[] m_spriteNames;
  public bool m_includeNoTab = true; // if true, 0 refers to no tab; else, 0 refers to the first tab

  public List<UILabel> m_tabLabels;  // add these manually to ensure the correct order
  private UISprite m_sprite;

	void Awake () {
    m_sprite = GetComponent<UISprite>();
	}
	
  public void SetTab (int tab) {
    // show the appropriate sprite
	  if (tab < m_spriteNames.Length) {
      m_sprite.spriteName = m_spriteNames[tab];
    }

    if (m_includeNoTab) { tab --; } // 0 now refers to the first label

    UILabel label;
    for (int i = 0; i < m_tabLabels.Count; i++) {
      label = m_tabLabels[i];
      label.color = (tab == i)? m_activeColor : m_inactiveColor;
      label.fontSize = (tab == i)?  m_activeTextSize : m_inactiveTextSize;
    }
	}
}
