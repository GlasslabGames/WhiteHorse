using System.Collections.Generic;
using System;
using MiniJSON;
using UnityEngine;

public class PersistData {
  private Dictionary<string, object> m_dataStore;
  private const string OWNER_TYPE_ID = "__type__";
  public PersistData(object owner) : this(owner, new Dictionary<string, object>()) {}

  public PersistData (object owner, Dictionary<string, object> data)
  {
    m_dataStore = data;

    if (owner != null)
    {
      m_dataStore [OWNER_TYPE_ID] = owner.GetType ().FullName;
    }
  }

  public List<string> GetKeys()
  {
    return new List<string> (m_dataStore.Keys);
  }

  public void Store(string key, object data)
  {
    m_dataStore [key] = data;
  }

  public T Retrieve<T>(string key)
  {
    if (m_dataStore.ContainsKey(key))
    {
      return (T) Convert.ChangeType(m_dataStore[key], typeof(T));
      //return (T)m_dataStore [key];
    }
    else
      return default(T);
  }

  public bool HasData(string key)
  {
    return m_dataStore.ContainsKey (key);
  }

  public object Retrieve(string key)
  {
    if (HasData (key))
    {
      return m_dataStore [key];
    } else
    {
      Debug.LogWarning("[PersistData("+m_dataStore [OWNER_TYPE_ID]+")] Tried to find "+key+" but key does not exist");
      return null;
    }
  }

  public string SerializeJSON()
  {
    return Json.Serialize (m_dataStore);
  }

  public static PersistData DeserializeJSON(object owner, string json)
  {
    return new PersistData(owner, (Dictionary<string, object>) Json.Deserialize(json));
  }
}