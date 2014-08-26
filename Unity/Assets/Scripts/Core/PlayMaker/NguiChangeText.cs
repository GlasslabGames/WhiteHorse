using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory("NGUI")]
  [Tooltip("Change the text on an NGUI Label")]
  public class NguiChangeText : FsmStateAction
  {
    [RequiredField]
    [Tooltip("Target Label")]
    public UILabel label;

    [Tooltip("The text to change the label to")]
    public FsmString text;
    
    public override void Reset()
    {
      label = null;
      text = null;
    }
    
    public override void OnEnter()
    {
      label.text = text.Value;

      Finish();
    }
  }
}