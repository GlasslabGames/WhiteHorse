using UnityEngine;
using GlassLab.Core.Serialization;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory("SessionManager")]
  [Tooltip("Tells SessionManager to save the game.")]
  public class SessionSave : FsmStateAction
  {
    public override void OnEnter()
    {
      SessionManager.InstanceOrCreate.Save();
      
      Finish ();
    }
  }
}