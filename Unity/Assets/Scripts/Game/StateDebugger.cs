using UnityEngine;
using System.Collections;


public class StateDebugger : MonoBehaviour
{
  public Player m_playerScript;
  public BudgetController m_playerBudget;


  public void PrintState()
  {
    Debug.Log( "Player: " + m_playerScript.m_leaning );
    Debug.Log( "Player Budget: " + m_playerBudget.m_amount );
  }
}