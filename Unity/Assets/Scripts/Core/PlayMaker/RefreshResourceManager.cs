
using System.Collections;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
  [ActionCategory("GLResourceManager")]
  [Tooltip("Forces a refresh in the GLResourceManager. This will allow it to add DisposableSprite components to UITextures and SpriteRenderers.")]
  public class RefreshResourceManager : FsmStateAction
  {
    public override void OnEnter()
    {
      GLResourceManager.InstanceOrCreate.ForceRefresh ();

      Finish ();
    }
  }
}