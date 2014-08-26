using UnityEngine;
using System.Collections.Generic;

public class PoolManager : SingletonBehavior<PoolManager> {

  private Dictionary<string, List<GameObject>> m_objectPools = new Dictionary<string, List<GameObject>>();

  private PoolManager() {}

  // CLASS IS INCOMPLETE, VARIABLES AND FUNCTIONS BELOW ARE A HACK
  private const string CURTAIN_CAMERA_PREFAB_NAME = "CurtainCamera";
  private const string CURTAIN_RENDER_TEXTURE_PREFAB_NAME = "CurtainCameraTexturePlane";
  private const string CURTAIN_MATERIAL_NAME = "CurtainRenderMaterial";
  private const string CURTAIN_TEXTURE_NAME = "CurtainRenderTexture";
  public const string CURTAIN_OBJECT_NAME = "_CURTAINS_";

// TODO: Generalize this manager
  void Awake()
  {
    Transform[] children = GetComponentsInChildren<Transform>(true);
    foreach (Transform child in children)
    {
      if (child.parent == transform)
      {
        CacheObject(child.gameObject);
      }
    }
  }


  // TODO: Consolidate this with the generalized version of object caching
  public GLCurtainController GetCurtain()
  {
    GameObject returnObj = GetObject(CURTAIN_OBJECT_NAME);
    GLCurtainController curtains;
    if (returnObj == null)
    {
      curtains = CreateCurtain();
    }
    else
    {
      curtains = returnObj.GetComponentInChildren<GLCurtainController>();
    }

    return curtains;
  }

  private static int renderCamIndex = 0;
  private GLCurtainController CreateCurtain()
  {
    GameObject camObj = (GameObject) Instantiate(Resources.Load<GameObject>(CURTAIN_CAMERA_PREFAB_NAME));
    Camera cam = camObj.GetComponent<Camera>();
    RenderTexture renderTexture = new RenderTexture(512, 512, 0);
    renderTexture.filterMode = FilterMode.Bilinear;
    cam.targetTexture = renderTexture;

    camObj.transform.parent = UICamera.currentCamera.transform;
    camObj.transform.localPosition = new Vector3(10000 + renderCamIndex * 1000, 0, 0);
    camObj.transform.localScale = Vector3.one;
    
    UITexture texture = null;
    UITexture[] textures = camObj.GetComponentsInChildren<UITexture>(true);
    for (int i=0; i < textures.Length; i++)
    {
      if (textures[i].name == "CurtainCameraTexturePlane")
      {
        texture = textures[i];
        break;
      }
    }
    GameObject camTexturePlane = texture.gameObject;
    texture.mainTexture = renderTexture;
    texture.material = Resources.Load<Material>(CURTAIN_MATERIAL_NAME);
    camTexturePlane.transform.parent = camObj.transform;
    camTexturePlane.transform.localScale = Vector3.one;

    renderCamIndex++;

    return camObj.GetComponentInChildren<GLCurtainController>();
  }

  public GameObject GetObject(string objName)
  {
    if (m_objectPools.ContainsKey(objName))
    {
      List<GameObject> pool = m_objectPools[objName];
      int lastIndex = pool.Count-1;
      if (lastIndex >= 0)
      {
        GameObject returnObj = pool[lastIndex];
        pool.RemoveAt(lastIndex);
        returnObj.SetActive(true);
        return returnObj;
      }
      else
      {
        return createPrefabObject(objName);
      }
    }
    else
    {
      //Debug.LogWarning("[PoolManager] Could not find '"+objName+"', creating new one", this);

      return createPrefabObject(objName);
    }
  }

  public int GetNumInstances(string objName)
  {
    if (m_objectPools.ContainsKey(objName))
    {
      List<GameObject> pool = m_objectPools[objName];
      return pool.Count;
    }
    else
    {
      return 0;
    }
  }

  private GameObject createPrefabObject(string prefabName)
  {
    GameObject prefab = Resources.Load(prefabName) as GameObject;
    if (prefab != null)
    {
      return (GameObject) Instantiate(prefab);
    }
    else
    {
      //Debug.Log("[PoolManager] Could not find prefab '"+prefabName+"'", this);
      return null;
    }
  }

  public void PrecachePrefabObject(string prefabName, int totalToCreate = 1)
  {
    if (m_objectPools.ContainsKey(prefabName))
    {
      totalToCreate -= m_objectPools[prefabName].Count; // Only cache up to the amount desired
    }

    for (int i=0; i < totalToCreate; i++)
    {
      GameObject obj = createPrefabObject(prefabName);
      if (obj == null)
      {
        break;
      }

      CacheObject(obj);
    }
  }

  public void CacheObject(GameObject obj, string objName = null)
  {
    if (objName == null)
    {
      objName = obj.name;

      if (objName.IndexOf("(Clone)") != -1)
      {
        objName = objName.Substring(0,objName.IndexOf("(Clone)"));
      }
    }

    List<GameObject> pool;
    if (!m_objectPools.ContainsKey(objName))
    {
      m_objectPools[objName] = new List<GameObject>();
    }
    pool = m_objectPools[objName];

    pool.Add(obj);

    obj.transform.parent = this.transform;
    obj.SetActive(false);
  }

  public void PurgeCache()
  {
    Debug.Log("[PoolManager] Purging cache...", this);
    List<string> keys = new List<string>(m_objectPools.Keys);
    for (int i=keys.Count-1; i>=0; i--)
    {
      string prefabName = keys[i];
      List<GameObject> pool = m_objectPools[prefabName];
      for (int j=pool.Count-1; j>=0; j--)// obj in pool)
      {
        if (pool[j] != null) Destroy(pool[j]);
      }
      pool.Clear();
      m_objectPools.Remove(prefabName);
    }
  }
  
  public void DebugDump()
  {
    string returnString = "---- PoolManager Dump ----";
    foreach (string key in m_objectPools.Keys)
    {
      returnString += "\n" + key + " - " + m_objectPools[key].Count;
    }

    Debug.Log(returnString, this);
  }
}
