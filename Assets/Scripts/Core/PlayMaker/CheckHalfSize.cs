
using UnityEngine;
using HutongGames.PlayMaker;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory("ResourceManager")]
  [Tooltip("Check if we should be using half-size assets")]
  public class CheckHalfSize : FsmStateAction
  {
    public FsmEvent FullSizeEvent;
    public FsmEvent HalfSizeEvent;
    
    public override void Reset()
    {
      FullSizeEvent = null;
      HalfSizeEvent = null;
    }
    
    public override void OnEnter()
    {
      if (ExplorationManager.ScreenHalfSize)
      {
        Fsm.Event(HalfSizeEvent);
      }
      else
      {
        Fsm.Event(FullSizeEvent);
      }
      Finish ();
    }
  }
}