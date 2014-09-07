using UnityEngine;
using System.Collections;


public class CampaignWorker : MonoBehaviour
{
  public int m_currentLevel;

  public int[] m_levelValues;


  public int GetValueForLevel()
  {
    return m_levelValues[ m_currentLevel - 1 ];
  }
}