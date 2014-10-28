using UnityEngine;
using System.Collections;


public class CampaignWorker : MonoBehaviour
{
	public float PercentChange = 0.05f; // 5%

  public int m_currentLevel;

  public int[] m_levelValues;

  public Sprite[] m_levelSprites;

	public bool Removed; // mark that it should be removed

  public int GetValueForLevel()
  {
    return m_levelValues[ m_currentLevel - 1 ];
  }

  public void Upgrade()
  {
    m_currentLevel++;
    GetComponent< SpriteRenderer >().sprite = m_levelSprites[ m_currentLevel - 1 ];
  }
}