
using UnityEngine;
using GlassLab.Core.Serialization;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory("SessionManager")]
  [Tooltip("Tells SessionManager to clear all save games.")]
  public class SessionClear : FsmStateAction
  {
    public override void OnEnter()
    {
      SessionManager.InstanceOrCreate.ClearSaves ();
      
      Finish ();
    }
  }
}