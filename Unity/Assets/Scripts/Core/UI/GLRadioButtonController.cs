using UnityEngine;

/**
 * 
 */
public class GLRadioButtonController : MonoBehaviour
{
  public event RadioButtonEvent ButtonSelected;

  private GLRadioButton[] m_radioButtonList;
  
  private GLRadioButton m_selectedRadioButton;

  public GLRadioButton DefaultSelectedButton;
  
  // Use this for initialization
  void Start () {
    UpdateTabs ();
  }
  
  public void UpdateTabs()
  {
    DeregisterChildButtonHandlers ();

    m_radioButtonList = GetComponentsInChildren<GLRadioButton> ();

    GLRadioButton button;
    bool previousTabFound = false;
    for (int i=0; i < m_radioButtonList.Length; i++) {
      button = m_radioButtonList[i];
      if (button != m_selectedRadioButton)
      {
        button.Deselect();
      }
      else previousTabFound = true;
    }

    if (!previousTabFound) {
      m_selectedRadioButton = null;
      
      if (DefaultSelectedButton != null)
      {
        DefaultSelectedButton.Select();
        m_selectedRadioButton = DefaultSelectedButton;
      }
    }

    RegisterChildButtonHandlers ();
  }

  private void RegisterChildButtonHandlers()
  {
    if (m_radioButtonList == null)
      return;

    GLRadioButton button;
    for (int i=0; i < m_radioButtonList.Length; i++) {
      button = m_radioButtonList[i];
      button.OnSelectHandler += OnRadioButtonSelected;
    }
  }

  private void DeregisterChildButtonHandlers()
  {
    if (m_radioButtonList == null)
      return;

    GLRadioButton button;
    for (int i=0; i < m_radioButtonList.Length; i++) {
      button = m_radioButtonList[i];
      button.OnSelectHandler -= OnRadioButtonSelected;
    }
  }

  public void OnRadioButtonSelected(GLRadioButton selectedButton)
  {
    if (m_selectedRadioButton != null)
      m_selectedRadioButton.Deselect ();
    m_selectedRadioButton = selectedButton;

    if (ButtonSelected != null)
    {
      ButtonSelected(selectedButton);
    }
  }
}