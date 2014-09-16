using System;
using UnityEngine;

namespace GlassLab.Core.Conditional
{
  class SceneExistsConditional : Conditional
  {
    [SerializeField]
    public string SceneName;

    protected override bool CalculateIsSatisfied()
    {
      return GLResourceManager.InstanceOrCreate.AssetExists(SceneName) || 
        GLResourceManager.InstanceOrCreate.AssetExists(SceneName + ".unity");
    }
  }
}