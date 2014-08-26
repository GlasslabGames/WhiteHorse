using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory("After Effects Animation")]
  [Tooltip("Plays an Animation on a Game Object.")]
  public class PlayGLAEAnimation : FsmStateAction
  {
    [TooltipAttribute("Target Object")]
    public GLAfterEffectsAnimationController target;

    public FsmString animationName;

    public FsmEvent onAnimationComplete;
    
    public override void Reset()
    {
      target = null;
      animationName = null;
      onAnimationComplete = null;
    }
    
    public override void OnEnter()
    {
      target.PlayAnimation(animationName.Value);
      if (onAnimationComplete != null)
      {
        target.AnimationFinished += animationFinishedCallback;
      }
      else
      {
        Finish();
      }
    }

    private void animationFinishedCallback(GLAfterEffectsAnimationController glaea)
    {
      target.AnimationFinished -= animationFinishedCallback;

      Fsm.BroadcastEvent(onAnimationComplete);

      Finish();
    }
  }
}
