using UnityEngine;

public class GLChildCount : MonoBehaviour
{
  public UILabel CountLabel;

  public bool ImmediateChildrenOnly = true;

  void Awake()
  {
    refresh();
  }

  private void refresh()
  {
    if (CountLabel != null)
    {

      CountLabel.text = Utility.GetNumChildren(gameObject, ImmediateChildrenOnly).ToString();
    }
  }
}