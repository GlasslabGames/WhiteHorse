using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UILabel))]
public class LabelColorController : MonoBehaviour {
  private static Color COLOR = new Color();
  public BoxCollider PressContainer;
  public GLDragDropContainer DragContainer;

  public bool ChangeOnPress = false;
  
  public bool ChangeOnDragOver = false;
  public Color DragChangeColor = Color.white;

  public bool ChangeOnRadioSelect = false;
  public Color RadioSelectChangeColor = Color.green;
  public GLRadioButton TargetRadioButton;

  private UILabel m_ownerUILabel;
  private Color m_ownerOriginalColor;

  public void Start()
  {
    // Subscribe to drag event if possible
    if (ChangeOnDragOver) {
      if (DragContainer != null)
      {
        DragContainer.ItemDragEnter += OnDragEnter;
        DragContainer.ItemDragExit += OnDragExit;
      } else {
        Debug.LogError ("[LabelColorController] Selected change on drag over but no drag container found!");
      }
    }

    if (ChangeOnRadioSelect) {
      if (TargetRadioButton != null)
      {
        TargetRadioButton.OnSelectHandler += onRadioSelect;
        TargetRadioButton.OnDeselectHandler += onRadioDeselect;
      } else {
        Debug.LogError("[LabelColorController] Selected change on radio select but no radio button found!", this);
      }
    }

    m_ownerUILabel = GetComponent<UILabel> ();
    m_ownerOriginalColor = m_ownerUILabel.color;
  }

  public void OnDestroy()
  {
    if (ChangeOnDragOver) {
      if (DragContainer != null)
      {
        DragContainer.ItemDragEnter -= OnDragEnter;
        DragContainer.ItemDragExit -= OnDragExit;
      }
    }

    if (ChangeOnRadioSelect) {
      if (TargetRadioButton != null)
      {
        TargetRadioButton.OnSelectHandler -= onRadioSelect;
        TargetRadioButton.OnDeselectHandler -= onRadioDeselect;
      }
    }
  }

  public void OnDragEnter(GLDragEventArgs eventArgs)
  {
    eventArgs.Consume ();
    changeColor ();
  }

  public void OnDragExit(GLDragEventArgs eventArgs)
  {
    eventArgs.Consume ();
    revertColor ();
  }

  private void onRadioSelect(GLRadioButton button)
  {
    if (DragContainer == null || !DragContainer.IsOver)
    {
      COLOR.r = RadioSelectChangeColor.r;
      COLOR.g = RadioSelectChangeColor.g;
      COLOR.b = RadioSelectChangeColor.b;
      COLOR.a = m_ownerUILabel.alpha;
      m_ownerUILabel.color = COLOR;
    }
  }

  private void onRadioDeselect(GLRadioButton button)
  {
    if (DragContainer == null || !DragContainer.IsOver) {
      COLOR.r = m_ownerOriginalColor.r;
      COLOR.g = m_ownerOriginalColor.g;
      COLOR.b = m_ownerOriginalColor.b;
      COLOR.a = m_ownerUILabel.alpha;
      m_ownerUILabel.color = COLOR;
    }
  }

  void OnPress (bool pressed)
  {
    if (pressed)
      changeColor ();
    else
      revertColor ();
  }

  private void changeColor()
  {
    COLOR.r = DragChangeColor.r;
    COLOR.g = DragChangeColor.g;
    COLOR.b = DragChangeColor.b;
    COLOR.a = m_ownerUILabel.alpha;
    m_ownerUILabel.color = COLOR;
  }

  private void revertColor()
  {
    if (ChangeOnRadioSelect && TargetRadioButton.IsSelected) {
      COLOR.r = RadioSelectChangeColor.r;
      COLOR.g = RadioSelectChangeColor.g;
      COLOR.b = RadioSelectChangeColor.b;
      COLOR.a = m_ownerUILabel.alpha;
      m_ownerUILabel.color = COLOR;
    } else {
      COLOR.r = m_ownerOriginalColor.r;
      COLOR.g = m_ownerOriginalColor.g;
      COLOR.b = m_ownerOriginalColor.b;
      COLOR.a = m_ownerUILabel.alpha;
      m_ownerUILabel.color = COLOR;
    }
  }
}
