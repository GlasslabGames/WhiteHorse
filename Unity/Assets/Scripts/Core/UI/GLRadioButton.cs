using UnityEngine;

public delegate void RadioButtonEvent(GLRadioButton sender);

[RequireComponent(typeof(BoxCollider))]
public class GLRadioButton : MonoBehaviour
{
  public RadioButtonEvent OnSelectHandler;
  public RadioButtonEvent OnDeselectHandler;
  public bool ClickCanDeselect = false;

  public bool IsSelected { get; private set; }

  public GLRadioButton ()
  {
    IsSelected = false;
  }

  void OnPress(bool isDown)
  {
    if (isDown)
    {
      if (IsSelected && ClickCanDeselect)
      {
        Deselect ();
      }
      else
      {
        Select ();
      }
    }
  }

  void OnClick()
  {
  }

  public void Select()
  {
    if (!IsSelected) {
      IsSelected = true;

      if (OnSelectHandler != null)
      {
        OnSelectHandler(this);
      }
    }
  }
  
  public void Deselect()
  {
    if (IsSelected) {
      IsSelected = false;
      
      if (OnDeselectHandler != null)
      {
        OnDeselectHandler(this);
      }
    }
  }
}