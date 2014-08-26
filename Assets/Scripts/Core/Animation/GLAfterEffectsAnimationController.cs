using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;
using GlassLab.Batch;

public class GLAfterEffectsAnimationController : MonoBehaviour {
  private AfterEffectAnimation m_controller;

  public string PrefabPrefix;

  public TextAsset[] AnimationFiles;
  public String[] PrecacheAnimationFileNames;

  public LayerMask SpriteRendererLayer;
  public string SortingLayerName = "Default";
  public int SortingOrder = 0;

  public bool PlayOnStart;

  public bool LoopOnStart = false;

  public bool ResetOnEnable = false;

  public bool ForceNGUI = false;

  private bool m_wasPlayingOnDisable = false;

  /*
   * List of events to play.
   * Note: This list gets wiped when the animation changes.
   */
  private Dictionary<int, List<Action>> m_frameEventCallbacks = new Dictionary<int, List<Action>>();

  public List<AnimFrame> CallbackFrames;

  public List<SoundForAnimation> SoundEvents; // sorted by name of sound to play

  private Dictionary<string, AnimFrame> m_frameLabels;

  public delegate void ControllerDel(GLAfterEffectsAnimationController controller);

  public ControllerDel AnimationFinished;
  public ControllerDel AnimationFinishedOnceOnly;
  public ControllerDel AnimationChanged;

  public bool PrecacheAnimations = true;
  public bool CacheAnimations = true;

  private bool m_asyncLoad = false;
  public bool UseAsyncLoad { set { m_asyncLoad = value; } }

  void Awake()
  {
    m_frameLabels = CallbackFrames.ToDictionary( x => x.Name ); // this should be fine in Awake
  }

  // NOTE: This cannot be "Awake" because whether this component is NGUI must be determined after instantiation.
  //      (Code might be changing parent of this object after prefab instantiation)
  void Start()
  {
    // Search for default animation
    string defaultAnimation = null;
    RandomAnimator ra = GetComponent<RandomAnimator>();
    if (ra != null)
    {
      defaultAnimation = ra.GetRandomAnimationName();
    }
    else
    {
      // No random animator, default to first animation
      if (AnimationFiles.Length > 0)
        defaultAnimation = AnimationFiles[0].name;
      else if (PrecacheAnimationFileNames.Length > 0)
        defaultAnimation = PrecacheAnimationFileNames[0];
        
    }
    if (PrecacheAnimations)
    {
      foreach (TextAsset data in AnimationFiles)
      {
        if (data != null && data.name != defaultAnimation) // Don't precache first animation, it'll get loaded as the first animation
        {
          string cacheName = PrefabPrefix + data.name;
          if (PoolManager.InstanceOrCreate.GetNumInstances(cacheName) == 0)
          {
            string animationName = data.name;
            if (!GLBatchManager.InstanceOrCreate.HasJob(cacheName + "Animation"))
            {
              GLBatchManager.InstanceOrCreate.AddJob(delegate()
              {
                PoolManager.InstanceOrCreate.CacheObject(getAEAnimation(animationName).gameObject, cacheName);
              }, cacheName + "Animation");
            }
          }
        }
      }
      foreach (string dataName in PrecacheAnimationFileNames)
      {
        if (dataName != defaultAnimation) // Don't precache first animation, it'll get loaded as the first animation
        {
          string cacheName = PrefabPrefix + dataName;
          if (PoolManager.InstanceOrCreate.GetNumInstances(cacheName) == 0)
          {
            string animationName = dataName;
            if (!GLBatchManager.InstanceOrCreate.HasJob(cacheName + "Animation"))
            {
              GLBatchManager.InstanceOrCreate.AddJob(delegate()
              {
                PoolManager.InstanceOrCreate.CacheObject(getAEAnimation(animationName).gameObject, cacheName);
              }, cacheName + "Animation");
            }
          }
        }
      }
    }

    if (m_controller == null && !string.IsNullOrEmpty(defaultAnimation))
    {
      PlayAnimation(defaultAnimation, PlayOnStart, LoopOnStart);
    }
  }

  private GameObject createAnimation(string animationName)
  {
    GameObject newAnimation = new GameObject();
    newAnimation.layer = gameObject.layer;
    AfterEffectAnimation aea = newAnimation.AddComponent<AfterEffectAnimation>();
    
    TextAsset data = getAssetByName(animationName);
    aea.dataFile = data;
    newAnimation.name = PrefabPrefix+animationName;

    return newAnimation;
  }

  public float GetRemainingAnimationTime()
  {
    if (m_controller != null)
    {
      return m_controller.totalFrames/30f/m_controller.timeScale;
    }
    else
    {
      return 0f;
    }
  }

  public int GetCurrentFrame()
  {
    if (m_controller != null)
    {
      return m_controller.currentFrame;
    }
    else return 0;
  }

  public bool IsInterrupted
  {
    get{
      return !IsPlaying && m_wasPlayingOnDisable;
    }
  }

  void OnDisable()
  {
    if (m_controller != null)
    {
      m_wasPlayingOnDisable = m_controller.isPlaying && (m_controller.currentFrame != m_controller.lastFrame || m_controller.Loop);
      m_controller.Stop();
    }
  }

  void OnEnable()
  {
    if (m_controller != null && m_wasPlayingOnDisable)
    {
      if (ResetOnEnable)
        m_controller.GoToAndPlay(m_controller.PlayBackwards ? m_controller.lastFrame : 0);
      else
        m_controller.Play();
    }
  }

  // HACK for combat calling "damage" etc. on bots without full argubot name
  private string getAnimationEndingWith(string animationName)
  {
    int i;
    for (i=AnimationFiles.Length-1; i>=0; i--) //TextAsset text in AnimationFiles)
    {
      TextAsset text = AnimationFiles[i];
      if (text.name.EndsWith(animationName))
      {
        return text.name;
      }
    }
    for (i=PrecacheAnimationFileNames.Length-1; i>=0; i--) //string dataName in PrecacheAnimationFileNames)
    {
      string dataName = PrecacheAnimationFileNames[i];
      if (dataName.EndsWith(animationName))
      {
        return dataName;
      }
    }

    return null;
  }

  private bool hasAnimation(string animationName)
  {
    foreach (TextAsset text in AnimationFiles)
    {
      if (text.name == animationName)
      {
        return true;
      }
    }
    foreach (string dataName in PrecacheAnimationFileNames)
    {
      if (dataName == animationName)
      {
        return true;
      }
    }
    
    return false;
  }
  
  private AfterEffectAnimation getAEAnimation(string animationName)
  {
    if (!hasAnimation(animationName) && getAnimationEndingWith(animationName) != null)
    {
      Debug.LogWarning("No animation for '"+animationName+"', do you mean '"+getAnimationEndingWith(animationName)+"'? Inferring animation name...");
      animationName = getAnimationEndingWith(animationName);
    }

    bool hierarchyDirty = false;
    GameObject newAnimation = PoolManager.InstanceOrCreate.GetObject(PrefabPrefix+animationName);
    if (newAnimation == null)
    {
      newAnimation = createAnimation(animationName);
      hierarchyDirty = true;
    }
    
    AfterEffectAnimation aea = newAnimation.GetComponent<AfterEffectAnimation>();
    if (aea == null)
    {
      Debug.LogError("Animation "+animationName+" has no AE Animation component!", this);
      if (newAnimation.GetComponentsInChildren<AfterEffectAnimation>(true).Length > 0)
      {
        Debug.LogWarning("Deeper search shows you have an AE animation. Perhaps it's in a child or gameobject is inactive?");
        aea = newAnimation.GetComponentsInChildren<AfterEffectAnimation>(true)[0];
      }
      else
      {
        return null;
      }
    }
    
    bool shouldBeNGUI = ForceNGUI || Utility.FirstAncestorOfType<UIRoot>(transform) != null;
    hierarchyDirty = hierarchyDirty || shouldBeNGUI != aea.IsNGUIAnimation;
    aea.IsNGUIAnimation = shouldBeNGUI;
    
    newAnimation.transform.parent = transform;
    newAnimation.transform.localPosition = Vector3.zero;
    newAnimation.transform.localScale = Vector3.one;//shouldBeNGUI ? Vector3.one : new Vector3(.01f, .01f);
    
    if (hierarchyDirty)
    {
      aea.gameObject.layer = gameObject.layer;

      if (m_asyncLoad)
      {
        aea.AsyncOnAnimationDataChange();
      }
      else
      {
        aea.OnAnimationDataChange();
      }
    }

    // We want to assign our sprites (if it's not NGUI) to correct layers, but delay one frame since the sprites are available immediately
    if (!shouldBeNGUI) {
      // Jerry set this up but it doesn't work. Leaving his code for future work.
      /*
      if (aea.IsLoadingData)
      {
        aea.onDataLoadCompleteOnceOnly += updateSpriteRendererSorting;
      }
      else
      {
        updateSpriteRendererSorting();
      }*/
      Utility.NextFrame(updateSpriteRendererSorting);
    }
    
    aea.pivotCenterX = .5f;
    aea.pivotCenterY = .5f;
    aea.Loop = false;
    aea.PlayOnStart = false;
    return aea;
  }

  private void updateSpriteRendererSorting()
  {
    if (m_controller != null)
    {
      SpriteRenderer[] spriteRenderers = m_controller.GetComponentsInChildren<SpriteRenderer>(true);
      for (int i=0; i < spriteRenderers.Length; i++)
      {
        spriteRenderers[i].sortingOrder = SortingOrder;
        spriteRenderers[i].sortingLayerName = SortingLayerName;
      }
    }
  }

  public bool IsPlaying { get { return m_controller != null && m_controller.isPlaying && m_controller.currentFrame != m_controller.lastFrame; } }

  /**
   * Check every frame if we have a callback on that frame
   */
  private void onFrame()
  {
    // Warning: Does not compensate for frame-skip (AE Plugin isn't skipping frames)
    for (int frameNum = m_controller.previousFrame+1; frameNum <= m_controller.currentFrame; frameNum++)
    if (m_frameEventCallbacks.ContainsKey(frameNum))
    {
      List<Action> callbacks = m_frameEventCallbacks[frameNum];
      foreach (Action a in callbacks)
      {
        a();
      }
    }
  }

  public void RegisterEventOnFrame(string frameName, Action callback)
  {
    if (m_frameLabels.ContainsKey(frameName))
    {
      RegisterEventOnFrame( m_frameLabels[frameName].Frame, callback );
    }
    else
    {
      Debug.LogWarning("There is no frame called "+frameName, this);
    }
  }

  /**
   * Registers a callback to a specific frame in the animation
   */
  public void RegisterEventOnFrame(int frameNum, Action callback)
  {
    if (!m_frameEventCallbacks.ContainsKey(frameNum))
    {
      m_frameEventCallbacks[frameNum] = new List<Action>();
    }
    else
    {
      // The way animations are set up now, it's possible the same callback is being set multiple times.
      // Remove this error when multiple callbacks are allowed per frame.
      Debug.LogError("Only 1 action allowed per frame!", this);
    }

    m_frameEventCallbacks[frameNum].Add(callback);
  }

  void OnDestroy()
  {
    if (m_controller != null)
    {
      m_controller.onComplete -= onAnimationFinished;
      m_controller.onFrame -= onFrame;
    }
  }

  public string GetCurrentAnimationName()
  {
    if (m_controller.dataFile != null)
    {
      return m_controller.dataFile.name;
    }
    else if (m_controller.dataFileName != null)
    {
      return m_controller.dataFileName;
    }
    return null;
  }

  private TextAsset getAssetByName(string name)
  {
    foreach (TextAsset asset in AnimationFiles)
    {
      if (asset.name == name)
      {
        return asset;
      }
    }
    
    return Resources.Load<TextAsset>(name);
  }

  public void Pause()
  {
    if (m_controller != null)
    {
      m_controller.Stop();
    }
  }

  public void Unpause()
  {
    if (m_controller != null)
    {
      m_controller.Play();
    }
  }

  public void PlayAnimation()
  {
    if (!gameObject.activeInHierarchy)
    {
      Debug.LogWarning("[GLAfterEffectsAnimationController] Play is ignored while object is inactive. Skipping play.", this);
      return;
    }

    if (m_controller != null)
    {
      m_controller.GoToAndPlay(0);
    }
  }
  
  public bool IsPlayingBackwards { get { return m_controller.PlayBackwards; } }

  public void PlayAnimation(int index, bool startAnimation = true, bool loop = false, bool reversed = false)
  {
    if (index >= AnimationFiles.Length) {
      Debug.LogWarning("[GLAfterEffectsAnimationController] There is no animation at index "+index+"!", this);
      return;
    }
    string name = AnimationFiles[index].name;
    PlayAnimation(name, startAnimation, loop, reversed);
  }

  // target = which controller to release (in between frames, GLAEA will have 2 controllers) 
  public void ReleaseController(AfterEffectAnimation target = null)
  {
    if (target == null) target = m_controller;
    if (PoolManager.Instance != null && CacheAnimations)
      PoolManager.Instance.CacheObject(target.gameObject, PrefabPrefix + (target.dataFile != null ? target.dataFile.name : target.dataFileName));//.SetActive(false);
    else
    {
      Destroy (target.gameObject);
    }
  }

  public void PlayAnimation(string name, bool startAnimation = true, bool loop = false, bool reversed = false)
  {
    if (!gameObject.activeInHierarchy)
    {
      Debug.LogWarning("[GLAfterEffectsAnimationController] Play is ignored while object is inactive. Skipping play of "+name, this);
      return;
    }

    if (m_controller == null || name != GetCurrentAnimationName())
    {
      // Disable previous animation
      if (m_controller != null)
      {
        AfterEffectAnimation oldController = m_controller; // must set temp reference or we'll have the wrong one next frame.
        StartCoroutine(disableLastNextFrame(oldController));
        m_controller.onComplete -= onAnimationFinished;
        m_controller.onFrame -= onFrame;
      }

      // Enable and Play new animation
      m_controller = getAEAnimation(name);
      m_controller.gameObject.SetActive(true);
      m_controller.onComplete += onAnimationFinished;
      m_controller.onFrame += onFrame;

      onAnimationChanged();
    }
    AnimationFinishedOnceOnly = null;
    m_controller.PlayBackwards = reversed;
    m_controller.Loop = loop;

    if (m_controller.IsLoadingData)
    {
      // TODO: ew, an anonymous function
      m_controller.onDataLoadCompleteOnceOnly += delegate() {
        doPlayAnimation(startAnimation, reversed);
      };
    }
    else
    {
      doPlayAnimation(startAnimation, reversed);
    }
  }

  public bool ControllerLoadingData { get { return m_controller != null && m_controller.IsLoadingData; } }

  private void doPlayAnimation(bool startAnimation, bool reversed)
  {
    if (startAnimation)
    {
      m_controller.GoToAndPlay(reversed ? m_controller.lastFrame : 0);
      
      if (Fabric.EventManager.Instance != null) {
        string animationName = GetCurrentAnimationName();
        foreach (SoundForAnimation sfa in SoundEvents)
        {
          string processedName = getAnimationEndingWith(sfa.AnimationName);
          if (sfa.AnimationName == animationName || processedName == animationName)
          {
            Fabric.EventManager.Instance.PostEvent(sfa.SoundEvent);
            break;
          }
        }
      }
    }
    else
    {
      m_controller.GoToAndStop(reversed ? m_controller.lastFrame : 0);
    }
  }

  private IEnumerator disableLastNextFrame(AfterEffectAnimation target)
  {
    yield return new WaitForEndOfFrame();
    ReleaseController(target);
  }

  public void ResetAnimation() {
    if (m_controller != null)
    {
      m_wasPlayingOnDisable = false;
      m_controller.GoToAndStop(0);
    }
  }

  private void onAnimationChanged()
  {
    m_frameEventCallbacks.Clear();
    if (AnimationChanged != null) AnimationChanged(this);
  }

  private void onAnimationFinished()
  {
    if (AnimationFinished != null)
    {
      AnimationFinished(this);
    }

    if (AnimationFinishedOnceOnly != null)
    {
      ControllerDel temp = AnimationFinishedOnceOnly;
      AnimationFinishedOnceOnly = null;
      temp(this);
    }
  }
}

// Identifies a frame by name. Could just use a dict if only we could edit dictionaries in the editor.
[System.Serializable]
public class AnimFrame {
  public string Name;
  public int Frame;
}

[System.Serializable]
public class SoundForAnimation {
  public string AnimationName;
  public Fabric.Event SoundEvent;
}
