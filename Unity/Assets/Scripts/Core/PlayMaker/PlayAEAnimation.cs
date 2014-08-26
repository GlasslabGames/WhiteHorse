using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory("After Effects Animation")]
  [Tooltip("Plays an Animation on a Game Object.")]
  public class PlayAEAnimation : FsmStateAction
  {
    [TooltipAttribute("Target Object")]
    public FsmGameObject target;

    public override void Reset()
    {
    }
    
    public override void OnEnter()
    {

      AfterEffectAnimation aea = target.Value.GetComponent<AfterEffectAnimation>();

      aea.Play();

      Finish();
    }
  }
}