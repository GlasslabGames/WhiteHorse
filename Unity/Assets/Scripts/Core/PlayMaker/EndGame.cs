using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory("ExplorationManager")]
  [Tooltip("End the game")]
  public class EndGame : FsmStateAction
  {
    public override void OnEnter()
    {
      ExplorationManager.Instance.EndGame();
      Finish ();
    }
  }
}
