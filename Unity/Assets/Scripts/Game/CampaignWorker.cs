using UnityEngine;
using System.Collections;


public class CampaignWorker : MonoBehaviour
{
  public int m_currentLevel;

  public int[] m_levelValues;

  public Sprite[] m_levelSprites;


  public int GetValueForLevel()
  {
    return m_levelValues[ m_currentLevel - 1 ];
  }

  public void Upgrade()
  {
    m_currentLevel++;
    GetComponent< SpriteRenderer >().sprite = m_levelSprites[ m_currentLevel - 1 ];
    gameObject.SendMessage( "BounceOut" );
  }
}