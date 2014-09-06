using UnityEngine;
using System;
using System.Collections.Generic;


public class BudgetController : MonoBehaviour
{
  public float m_amount;
  public UILabel m_label;


  public void Start()
  {
    UpdateLabel();
  }

  public bool IsAmountAvailable( float amount, bool consumeIfAvailable = false )
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

  public void ConsumeAmount( float amount )
  {
    m_amount -= amount;
    UpdateLabel();
  }

  public void GainAmount( float amount )
  {
    m_amount += amount;
    UpdateLabel();
  }

  public void ResetPool()
  {
    m_amount = 0.0f;
    UpdateLabel();
  }

  public void UpdateLabel()
  {
    if( m_label )
    {
      m_label.text = "$" + m_amount;
    }
  }
}