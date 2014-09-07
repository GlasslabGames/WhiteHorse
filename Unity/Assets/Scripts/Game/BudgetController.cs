using UnityEngine;
using System;
using System.Collections.Generic;


public class BudgetController : MonoBehaviour
{
  public int m_amount;
  public UILabel m_label;


  public void Start()
  {
    UpdateLabel();
  }

  public bool IsAmountAvailable( int amount, bool consumeIfAvailable = false )
  {
    if( m_amount >= amount )
    {
      if( consumeIfAvailable )
      {
        ConsumeAmount( amount );
      }

      return true;
    }

    return false;
  }

  public void ConsumeAmount( int amount )
  {
    m_amount -= amount;
    UpdateLabel();
  }

  public void GainAmount( int amount )
  {
    m_amount += amount;
    UpdateLabel();
  }

  public void ResetPool()
  {
    m_amount = 0;
    UpdateLabel();
  }

  public void UpdateLabel()
  {
    if( m_label )
    {
      m_label.text = m_amount.ToString();
    }
  }
}