using UnityEngine;
using System.Collections;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory("Resources")]
  [Tooltip("Unload unused resources.")]
  public class UnloadUnusedResources : FsmStateAction
  {
    [RequiredField]
    [Tooltip("The game object that owns the Behaviour.")]
    public FsmOwnerDefault gameObject;

    public bool WaitForComplete = false;
    
    public override void Reset()
    {
      gameObject = null;
      WaitForComplete = false;
    }

    public override void OnEnter()
    {
      if (!WaitForComplete)
      {
        Resources.UnloadUnusedAssets ();
        Finish();
      }
      else
      {
        GLResourceManager.Instance.AsyncUnload(onUnloadFinish);
      }
    }

    private void onUnloadFinish()
    {
      Finish();
    }
  }
}