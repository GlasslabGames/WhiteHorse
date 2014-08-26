//#define AUTO_ADD_DISPOSE_COMPONENT

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MiniJSON;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class GLResourceManager : SingletonBehavior<GLResourceManager> 
{
  private const string STAGE_SERVER_ROOT = "http://marsaa.glgames.org/dlc/";

  // -- Keep these in sync with CustomBuilder.cs (cannot access editor code in game)
  private const string MANIFEST_SCENE_KEY = "__Scenes__";
  private const string MANIFEST_BUNDLE_KEY = "__Bundles__";
  private const string MANIFEST_RESOURCE_KEY = "__Resources__";
  private const string MANIFEST_VERSION = "__Version__";
  private const string MANIFEST_BUNDLE_ID = "__BundleID__";
  // -------

  public bool PreloadScenes = true;

  private Dictionary<string, object> m_manifest;
  private List<string> m_manifest_scenes = new List<string>();
  private Dictionary<string, string> m_manifest_resources = new Dictionary<string, string>();
  private Dictionary<string, string> m_manifest_bundles = new Dictionary<string, string>();

  private GLResourceManager() {
  }

  private void preloadScenes()
  {
    List<GLSceneLoader> loaders = Utility.FindInstancesInScene<GLSceneLoader> ();
    for (int i=loaders.Count-1; i >= 0; i--)
    {
      GLSceneLoader loader = loaders[i];
      if (loader.PreloadScene && loader.gameObject.hideFlags == HideFlags.None)
      {
        loader.LoadScene();
      }
    }
  }

  override protected void Awake()
  {
    if (PreloadScenes) preloadScenes();
    refreshSprites ();

    loadManifest();
  }

  public bool AssetExists(string name)
  {
    loadManifest();
    return m_manifest_resources.ContainsKey(name) || m_manifest_bundles.ContainsKey(name) || m_manifest_scenes.Contains(name);
  }

  public string GetResourceLocation(string name)
  {
    loadManifest();
    if (!string.IsNullOrEmpty(m_manifest_resources[name]))
    {
      return m_manifest_resources[name] + "/" + name;
    }
    else
    {
      return name;
    }
  }
  
  public T[] PreloadAssets<T> (params string[] list) where T : UnityEngine.Object
  {
    int listLength = list.Length;
    T[] returnArray = new T[listLength];
    for (int i=0; i < listLength; i++)
    {
      returnArray[i] = Resources.Load<T>(GetResourceLocation(Utility.GetHalfSizeName(list[i])));
    }
    
    return returnArray;
  }
  
  public UnityEngine.Object[] PreloadAssets(params string[] list)
  {
    int listLength = list.Length;
    UnityEngine.Object[] returnArray = new UnityEngine.Object[listLength];
    for (int i=0; i < listLength; i++)
    {
      returnArray[i] = Resources.Load(list[i]);
    }
    
    return returnArray;
  }
  
  public string GetVersionString()
  {
    loadManifest();
    return (string) m_manifest[MANIFEST_VERSION];
  }

  public string GetMajorVersionString()
  {
    return Utility.GetMajorVersionFromVersion(GetVersionString());
  }

  public string GetProjectBundleID()
  {
    loadManifest();
    return (string) m_manifest[MANIFEST_BUNDLE_ID];
  }

  public T GetResource<T>(string name)
  {
    return default(T);
  }

  private void loadManifest()
  {
    if (m_manifest == null)
    {
      TextAsset manifest = Resources.Load<TextAsset>("manifest");
      m_manifest = (Dictionary<string, object>) Json.Deserialize(manifest.text);
      Dictionary<string,object> tempDic;
      List<object> tempList;

      tempDic = (Dictionary<string,object>) m_manifest[MANIFEST_BUNDLE_KEY];
      foreach (string key in tempDic.Keys)
      {
        m_manifest_bundles[key] = (string) tempDic[key];
      }

      tempDic = (Dictionary<string,object>) m_manifest[MANIFEST_RESOURCE_KEY];
      foreach (string key in tempDic.Keys)
      {
        m_manifest_resources[key] = (string) tempDic[key];
      }
      
      tempList = (List<object>) m_manifest[MANIFEST_SCENE_KEY];
      for (int i=tempList.Count-1; i>=0; i--)
      {
        m_manifest_scenes.Add((string) tempList[i]);
      }
    }
  }

  void OnEnable()
  {
    SignalManager.SceneLoaded += onSceneLoaded;
  }

  void OnDisable()
  {
    SignalManager.SceneLoaded -= onSceneLoaded;
  }

  private void onSceneLoaded(string sceneName, GameObject gameObject)
  {
    // TODO: run through passed in level object instead of entire scene
    refreshSprites ();

    Resources.UnloadUnusedAssets();
  }

  void OnLevelWasLoaded(int levelID)
  {
    refreshSprites ();
  }

  public void ForceRefresh()
  {
    refreshSprites ();
  }

  private void refreshSprites()
  {
    float startTime = Time.realtimeSinceStartup;
    int i;
    SpriteRenderer[] renderers = Resources.FindObjectsOfTypeAll<SpriteRenderer> ();
    
    for (i=renderers.Length-1; i >= 0; i--)
    {
      SpriteRenderer spriteRenderer = renderers [i];

      // Skip ones that aren't in the scene
      if (spriteRenderer.gameObject.hideFlags != HideFlags.None) continue;
      
      if (Utility.IsPrefab(spriteRenderer.gameObject)) continue;
      
      Transform spriteTransform = spriteRenderer.transform;
      DisposableSprite[] disposableSprites = spriteRenderer.GetComponentsInChildren<DisposableSprite> (true);
      DisposableSprite ds = null;
      for (int j=disposableSprites.Length-1; j>=0; j--)
      {
        if (disposableSprites[j].transform == spriteTransform)
        {
          ds = disposableSprites[j];
        }
      }
      
      #if AUTO_ADD_DISPOSE_COMPONENT
      if (ds == null)
      {
        ds = spriteRenderer.gameObject.AddComponent<DisposableSprite> ();
      }
      #endif

      if (ds != null)
      {
        if (!ds.gameObject.activeInHierarchy &&
            spriteRenderer.sprite != null &&
            ds.DisposeOnDisable)
        {
          ds.RemoveAndSaveSprite ();
        }
      }
    }

    UITexture[] uiTextures = Resources.FindObjectsOfTypeAll<UITexture> ();
    
    for (i=uiTextures.Length-1; i >= 0; i--)
    {
      UITexture texture = uiTextures [i];
      
      if (Utility.IsPrefab(texture.gameObject)) continue;

      Transform textureTransform = texture.transform;
      DisposableSprite[] disposableSprites = texture.GetComponentsInChildren<DisposableSprite> (true);
      DisposableSprite ds = null;
      for (int j=disposableSprites.Length-1; j>=0; j--)
      {
        if (disposableSprites[j].transform == textureTransform)
        {
          ds = disposableSprites[j];
        }
      }
      
      #if AUTO_ADD_DISPOSE_COMPONENT
      if (ds == null)
      {
        ds = texture.gameObject.AddComponent<DisposableSprite> ();
      }
      #endif

      if (ds != null)
      {
        if (!ds.gameObject.activeInHierarchy &&
            texture.mainTexture != null &&
            ds.DisposeOnDisable)
        {
          ds.RemoveAndSaveSprite ();
        }
      }
    }
    Debug.Log ("[GLResourceManager] Sprite traversal time: " + (Time.realtimeSinceStartup - startTime)*1000 + "ms");
  }

  /**
   * Memory Management
   */
  private Action m_asyncUnloadCallback;
  public void AsyncUnload(Action callback)
  {
    if (m_asyncUnloadCallback != null)
    {
      Debug.LogError("[GLResourceManager] Something is already waiting for unload callback. Aborting unload call", this);
      return;
    }

    m_asyncUnloadCallback = callback;

    StartCoroutine("doAsyncUnload");
  }
  
  private IEnumerator doAsyncUnload()
  {
    GC.Collect();

    yield return Resources.UnloadUnusedAssets ();
    
    m_asyncUnloadCallback();
    m_asyncUnloadCallback = null;
  }

  private float _nextCollectionTime = 0f;
  public void _iOS_ReceivedMemoryWarning(string message)
  {
    if (Time.time > _nextCollectionTime)
    {
      GC.Collect();

      Resources.UnloadUnusedAssets ();

      if (PoolManager.Instance != null)
      {
        PoolManager.Instance.PurgeCache();
      }
      _nextCollectionTime = Time.time + 15f; // Limit garbage collection to every 15 sec max
    }
  }
  
  /**
   * LEVEL LOADING
   */
  
  Queue<KeyValuePair<string, Action>> m_loadJobs = new Queue<KeyValuePair<string, Action>>(); // SceneName, Callback
  Coroutine m_asyncLoadCoroutine;
  KeyValuePair<string, Action> m_currentJob; // sceneName, callback
  bool m_currentJobCanceled = false;
  public void AsyncLoadScene(string sceneName, Action callback = null)
  {
    string resolutionSpecificName = Utility.GetResolutionSpecificName(sceneName);
    if (m_manifest_scenes.Contains(resolutionSpecificName))
    {
      sceneName = resolutionSpecificName;
    }

    KeyValuePair<string, Action> job = new KeyValuePair<string, Action>(sceneName, callback);
    if(!m_currentJob.Equals(default(KeyValuePair<string, Action>)))
    {
      m_loadJobs.Enqueue(job);
    }
    else
    {
      m_currentJob = job;
      
      // Start jobs
      m_asyncLoadCoroutine = StartCoroutine("LoadAsync");
    }
  }

  public void CancelLoadScene(string sceneName)
  {
    string resolutionSpecificName = Utility.GetResolutionSpecificName(sceneName);
    if (m_manifest_scenes.Contains(resolutionSpecificName))
    {
      sceneName = resolutionSpecificName;
    }

    if (m_currentJob.Key == sceneName)
    {
      m_currentJobCanceled = true;
    }
    else
    {
      Debug.LogError("[GLResourceManager] -----------------------------------", this);
      Debug.LogError("[GLResourceManager] Tried to cancel a job that wasn't running. You're going to have issues.", this);
      Debug.LogError("[GLResourceManager] Current job: "+m_currentJob.Key+",\tCanceled job: "+sceneName, this);
      Debug.LogError("[GLResourceManager] -----------------------------------", this);
    }
  }
  
  private IEnumerator LoadAsync()
  {
    AsyncOperation ao = Application.LoadLevelAdditiveAsync (m_currentJob.Key);
    ao.allowSceneActivation = true;
    yield return ao; // Wait for async to complete

    yield return 0; // Wait another frame to avoid chugging

    onLoadComplete();
  }
  
  private void onLoadComplete()
  {
    m_asyncLoadCoroutine = null;
    GameObject sceneObject = GameObject.Find (m_currentJob.Key.Replace("_halfSize", ""));
    if (m_currentJobCanceled && sceneObject != null)
    {
      Destroy(sceneObject); // throw it away
      Debug.LogWarning("[GLResourceManager] Threw away a scene '"+m_currentJob.Key+"' immediately after load (due to cancellation). Try to avoid this.", this);
    }
    else if (SignalManager.SceneLoaded != null)
    {
      SignalManager.SceneLoaded (m_currentJob.Key, sceneObject);
    }

    if (m_currentJob.Value != null)
    {
      m_currentJob.Value(); // callback
    }
    
    if(m_loadJobs.Count > 0)
    {
      // If we have more jobs, start them
      m_currentJob = m_loadJobs.Dequeue();
      m_asyncLoadCoroutine = StartCoroutine("LoadAsync");
    }
    else
    {
      // No more jobs? stop
      m_currentJob = default(KeyValuePair<string, Action>);
    }
    
    m_currentJobCanceled = false;
  }

  public bool IsLoadingScenes
  {
    get
    {
      return !m_currentJob.Equals(default(KeyValuePair<string, Action>));
    }
  }

  /**
   * Download queue
   */
  
  private Queue<KeyValuePair<string, Action<AssetBundle>>> m_downloadJobs = new Queue<KeyValuePair<string, Action<AssetBundle>>>(); // SceneName, Callback
  private Dictionary<string, AssetBundle> m_downloadCache = new Dictionary<string, AssetBundle>();
  private KeyValuePair<string, Action<AssetBundle>> m_currentDownloadJob;
  public void DownloadBundle(string bundlePath, Action<AssetBundle> callback)
  {
    if (m_downloadCache.ContainsKey(bundlePath))
    {
      Debug.LogWarning("[GLResourceManager] Already downloading "+bundlePath+", skipping", this);
      return;
    }
    KeyValuePair<string, Action<AssetBundle>> downloadJob = new KeyValuePair<string, Action<AssetBundle>>(bundlePath, callback);
    m_downloadCache[bundlePath] = null; // set to null to indicate request has started
    if (m_currentDownloadJob.Equals(default(KeyValuePair<string, Action<AssetBundle>>)))
    {
      m_currentDownloadJob = downloadJob;
      StartCoroutine(doDownload(m_currentDownloadJob.Key, m_currentDownloadJob.Value));
    }
    else
    {
      m_downloadJobs.Enqueue(downloadJob);
    }
  }

  private IEnumerator doDownload(string address, Action<AssetBundle> callback)
  {
    string actualAddress = STAGE_SERVER_ROOT + address + ".unity3d";

    float time = Time.time;
    WWW request = WWW.LoadFromCacheOrDownload(actualAddress, 1);
    yield return request;
    
    if (!string.IsNullOrEmpty(request.error))
    {
      Debug.LogError(request.error, this);
    }
    else
    {
      Debug.Log("[GLResourceManager] Download "+actualAddress + " - "+(Time.time-time) + " sec", this);
    }

    m_downloadCache[address] = request.assetBundle;

    callback(request.assetBundle);

    if (m_downloadJobs.Count > 0)
    {
      m_currentDownloadJob = m_downloadJobs.Dequeue();
      StartCoroutine(doDownload(m_currentDownloadJob.Key, m_currentDownloadJob.Value));
    }
    else
    {
      m_currentDownloadJob = default(KeyValuePair<string, Action<AssetBundle>>);
    }
  }

  public AssetBundle GetBundle(string address)
  {
    return m_downloadCache[address];
  }
}