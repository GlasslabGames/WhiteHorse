/**
 * Singleton Monobehavior.
 * http://wiki.unity3d.com/index.php/Singleton
 * 
 * Note: Some code might bypass the singleton behavior, although you kind of have to try. Still, be aware.
 * Reference: http://stackoverflow.com/questions/380755/a-generic-singleton
 */
using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// 
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>


public class SingletonBehavior<T> : MonoBehaviour where T : MonoBehaviour
{
  private static T _instance;
  
  protected bool m_awakeInit = false;
  protected bool m_startInit = false;
  
  private static object _lock = new object(); // Lock prevents conflicts in multi-threaded environments

  public static T Instance
  {
    get
    {
      lock(_lock)
      {
        if (_instance == null)
        {
          List<T> instances = Utility.FindInstancesInScene<T>();
          if (instances.Count > 0)
          {
            string className = typeof(T).ToString();
            GameObject prefab = Resources.Load(className) as GameObject;
            if (prefab == null)
            {
              T[] prefabs = Resources.LoadAll<T>("Singletons/");
              if (prefabs != null && prefabs.Length > 0)
              {
                prefab = prefabs[0].gameObject;
                if (prefabs.Length > 1)
                  Debug.LogError("[SingletonBehavior("+className+")] There are more than one prefab in resources with "+className+"!");
              }
            }
            for (int i=instances.Count-1; i>=0; i--)
            {
              T instance = instances[i];
              // Check if there's a prefab of this singleton loaded and ensure we're not grabbing it
              if (prefab == null || !instance.gameObject.Equals(prefab))
              {
                if (_instance == null)
                {
									_instance = instance;
								}
                else
                {
                  Debug.LogError("[SingletonBehavior("+className+")] Something went really wrong - there should never be more than 1 singleton!");
                  break;
                }
              } // end prefab check
            } // end for i loop
          }
        } // end _instance == null check

        return _instance;
      } // end lock
    } // end get
  }

  // Due to the setup nature of most managers and their dependency on their scene,
  // you probably don't want to use this getter unless you know there won't be dependencies.
  public static T InstanceOrCreate
  {
    get
    {
      if (applicationIsQuitting) {
        Debug.LogWarning("[SingletonBehavior] Instance '"+ typeof(T) +
                         "' already destroyed on application quit." +
                         " Won't create again - returning null.");
        return null;
      }
      
      lock(_lock)
      {
        if (_instance == null)
        {
          _instance = Instance; // Calls getter function

          // If still null, create one
          if (_instance == null)
          {
            GameObject singleton;

            string className = typeof(T).ToString();
            GameObject prefab = Resources.Load(className) as GameObject;
            if (prefab != null)
            {
              // If prefab exists, use it
              singleton = (GameObject) Instantiate(prefab);
              _instance = singleton.GetComponent<T>();
            }
            else
            {
              T[] prefabs = Resources.LoadAll<T>("Singletons/");
              if (prefabs != null && prefabs.Length > 0)
              {
                prefab = prefabs[0].gameObject;
                if (prefabs.Length > 1)
                  Debug.LogError("[SingletonBehavior("+className+")] There are more than one prefab in resources with "+className+"!");
                singleton = (GameObject) Instantiate(prefab);
                _instance = singleton.GetComponent<T>();
              }
              else
              {
                // If prefab doesn't exist, create a basic game object with the script attached
                singleton = new GameObject();
                _instance = singleton.AddComponent<T>();
              }
            }

            singleton.name = className;
          } // end 2nd if null check
        } // end initial if null check
        
        return _instance;
      }
    }
  }

  protected virtual void Awake()
  {
    if (_instance != null && _instance != this)
    {
      Debug.LogWarning("[SingletonBehavior] There shouldn't be more than one singleton! Destroying previous instance.", this);

      SingletonBehavior<MonoBehaviour>[] sbs = this.GetComponentsInChildren<SingletonBehavior<MonoBehaviour>>();
      if (sbs.Length > 1)
      {
        // Only destroy component if there are other managers
        Destroy(this);
      }
      else
      {
        // Destroy entire gameobject if there are no other managers
        Destroy(this.gameObject);
      }

      return;
    }

    _instance = this as T;

    m_awakeInit = true;
  }

  protected virtual void Start()
  {
    m_startInit = true;
  }


  protected virtual void OnDestroy()
  {
    if (!applicationIsQuitting)
    {
      Debug.LogWarning("[SingletonBehavior] " + GetType().Name + " destroyed.", this);
    }

    if (_instance == this)
    {
      _instance = null;
    }
  }
  
  private static bool applicationIsQuitting = false;
  /// <summary>
  /// When Unity quits, it destroys objects in a random order.
  /// In principle, a Singleton is only destroyed when application quits.
  /// If any script calls Instance after it have been destroyed, 
  ///   it will create a buggy ghost object that will stay on the Editor scene
  ///   even after stopping playing the Application. Really bad!
  /// So, this was made to be sure we're not creating that buggy ghost object.
  /// </summary>
  protected virtual void OnApplicationQuit()
  {
    applicationIsQuitting = true;
  }
}
