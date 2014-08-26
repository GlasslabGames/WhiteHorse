using UnityEngine;
using System.Collections;

/// <summary>
/// Controls which quest popup appears depending on the case.
/// </summary>
public class QuestAlert : MonoBehaviour {
  public GameObject MainQuestSprite;
  public GameObject SideQuestSprite;
  public GameObject MainBattleSprite;
  public GameObject SideBattleSprite;
  private GameObject[] m_sprites;

  void Awake() {
    m_sprites = new GameObject[] { MainQuestSprite, SideQuestSprite, MainBattleSprite, SideBattleSprite };
  }

	public void ShowForQuest(bool sidequest) {
    Debug.Log ("Showing quest alert with sidequest? "+sidequest, this);
    gameObject.SetActive(true);
    HideSprites();
    if (sidequest) SideQuestSprite.SetActive(true);
    else MainQuestSprite.SetActive(true);
  }

  public void ShowForBattle(bool sidequest) {
    Debug.Log ("Showing battle alert with sidequest? "+sidequest, this);
    gameObject.SetActive(true);
    HideSprites();
    if (sidequest) SideBattleSprite.SetActive(true);
    else MainBattleSprite.SetActive(true);
  }

  public void Hide() {
    gameObject.SetActive(false);
  }

  protected void HideSprites() {
    foreach (GameObject obj in m_sprites) {
      obj.SetActive(false);
    }
  }
}
