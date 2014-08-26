using UnityEngine;
using System;
using System.Collections;

/**
 * GLSceneLoader - Loads a scene and places it as a child of this component's owner.
 */
public class GLSceneLoader : MonoBehaviour {
  public string SceneName;

  public bool PreloadScene = true;

  public bool LoadOnStart = true;

  public bool Async = true;

  public bool ResetChildPos = false;

  public bool HideAfterLoad = false;

  public bool DestroyOnDisable = true;

  private GameObject m_sceneObject;

  private bool m_isLoading = false;

  public bool IsLoaded {
    get {
      return m_sceneObject != null;
    }
  }

  public bool IsLoading {
    get {
      return m_isLoading;
    }
  }

  public delegate void SceneDelegate(GLConsumableEventArgs args, GameObject scene);
  public event SceneDelegate Loaded;
  public event SceneDelegate Close;

  void OnEnable ()
  {
    if (LoadOnStart && !IsLoaded)
    {
      LoadScene();
    }
  }

  void OnDisable ()
  {
    CloseScene ();
  }

  public void LoadScene()
  {
    if (IsLoaded || IsLoading)
    {
      Debug.LogWarning("Scene is already loaded or loading. Ignoring command to load!", this);
      return;
    }

    m_isLoading = true;

    GLResourceManager.InstanceOrCreate.AsyncLoadScene(SceneName, onLoadComplete);
  }

  // Convenient function - if we already loaded a child and it's just hidden, show it instead of loading a new one
  public void LoadOrReopenScene()
  {
    if (!gameObject.activeSelf)
    {
      gameObject.SetActive(true);
    }

    if (m_sceneObject != null) m_sceneObject.SetActive(true);
    else LoadScene ();
  }

  public void CloseScene()
  {
    if (IsLoading)
    {
      GLResourceManager.Instance.CancelLoadScene(SceneName);
      return;
    }

    GLConsumableEventArgs args = new GLConsumableEventArgs();
    if (Close != null) Close(args, m_sceneObject);
    if (DestroyOnDisable && !args.isConsumed && m_sceneObject != null) {
      Destroy(m_sceneObject);
    }
  }

  private void onLoadComplete()
  {
    m_isLoading = false;

    m_sceneObject = GameObject.Find (SceneName);

    if (m_sceneObject == null) Debug.LogError("Couldn't find object with name "+SceneName+" in the loaded scene!", this);
    else {
      m_sceneObject.transform.parent = this.transform;
      
      if (ResetChildPos) {
        m_sceneObject.transform.localPosition = Vector3.zero;
        m_sceneObject.transform.localScale = Vector3.one;
      }

      if (HideAfterLoad) {
        m_sceneObject.SetActive(false);
      }

      if (Loaded != null) Loaded(new GLConsumableEventArgs(), m_sceneObject);
    }
  }
}
