// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

#if !UNITY_FLASH

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory(ActionCategory.Animation)]
	[Tooltip("Play an animation by going to that state of the attached Animator. Use this instead of PlayAnimation for sprite animations.")]
	public class PlayAnimatorState : FsmStateAction
	{
    [RequiredField]
    [CheckForComponent(typeof(Animator))]
    [Tooltip("Game Object to play the animation on.")]
    public FsmOwnerDefault gameObject;

    [Tooltip("The name of the animation to play.")]
    public FsmString animName;

		public override void Reset()
		{
			gameObject = null;
      animName = null;
		}

		public override void OnEnter()
		{
      GameObject go = Fsm.GetOwnerDefaultTarget(gameObject);
      if (go == null || string.IsNullOrEmpty(animName.Value))
      {
        Finish();
        return;
      }
      Debug.Log ("Trying to PlayAnimatorState on "+Owner.name+" with "+go.name);

      
      Animator anim = go.GetComponent<Animator>();
      if (anim == null) {
        LogWarning("Missing animator component!");
      } else {
        anim.Play(animName.Value);
      }

      Finish ();
		}
	}
}

#endif