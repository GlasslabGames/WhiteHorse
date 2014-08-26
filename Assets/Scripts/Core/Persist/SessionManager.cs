using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using MiniJSON;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SessionManager : SingletonBehavior<SessionManager>
{
  public const string SAVE_KEY = "HIRO_SAVE";
  public const string ACTIVE_KEY = "active";
  public const string VERSION_KEY = "__version__"; 
  public const string AVATAR_KEY = "__AVATAR__"; 
  public const string SAVETIME_KEY = "HIRO_SAVETIME";
  public const string SAVENOTE_KEY = "HIRO_SAVENOTE";

  public const int NONE_SELECT = -5;
  public static int DebugLoadFlag = NONE_SELECT;

  private bool m_didLoad = false;

  private bool m_busy = false;
  private bool m_doSave = false; // Used to limit saves to once per frame
  public bool m_ignoreSaves = false; // Used while debugging to continue playing w/o saving
  private float m_lastSaveTime; // time in seconds from when we started the game

  public float TimeSinceLastSave {
    get {
      Debug.Log ("currentTime: "+ Time.realtimeSinceStartup + "- LastSaveTime: "+m_lastSaveTime);
      return Time.realtimeSinceStartup - m_lastSaveTime;
    }
  }

  public Dictionary<string, object> m_currentSaveData;

  [HideInInspector]
  public bool InitComplete = false;

  private SessionManager()
  {
  }

  public bool IsBusy()
  {
    return m_busy;
  }
  
  // accountName null = current account
	private string getSaveKey(string accountName = null, int debugSaveNum = -1, string keyType = SAVE_KEY)
  {
		string saveKey = (accountName == null ? AccountManager.InstanceOrCreate.GetCurrentAccount() : accountName)
			+ ((debugSaveNum == -1) ? "" : ("_" + debugSaveNum.ToString()))
        + "_" + keyType;
		//Debug.Log("!!!!!!!!!!!!!!!!!![DebugSave] " + saveKey);
		return saveKey;
  }

  private string getSavePath(string accountName = null)
  {
    return Application.persistentDataPath + "/" + getSaveKey(accountName);
  }

  override protected void Awake()
   {
    if (m_ignoreSaves == true)
    {
      Debug.LogError("IgnoreSaves should not be set to true permanently. Only check the box during play if you can.");
    }
    Debug.Log("[SessionManager] Persisting data to "+ Application.persistentDataPath, this);
    if (DebugLoadFlag != NONE_SELECT)
    {
      int saveNum = DebugLoadFlag;
      if (saveNum != -1)
        m_ignoreSaves = true;
      DebugLoadFlag = NONE_SELECT;
      DebugLoad(saveNum);
    }
    else
      Load();
  }

  void Start()
  {
    if (m_didLoad)
    {
      List<PersistTarget> saveTargets = Utility.FindInstancesInScene<PersistTarget>();
      for (int i = saveTargets.Count-1; i >= 0; i--)
      {
        saveTargets[i].SendMessage("OnLoad", SendMessageOptions.DontRequireReceiver);
      }
    }
    else
    {
      if (SignalManager.NewGameStarted != null)
      {
        SignalManager.NewGameStarted();
      }
    }

    InitComplete = true;
  }

  [ContextMenu("Clear Save")]
  // accountName null = current account
  public void ClearSaves(string accountName = null)
  {
    Debug.Log ("Clearing save for "+getSaveKey(accountName));
    PlayerPrefs.DeleteKey(getSaveKey(accountName));

    // Clear save data server-side
    PegasusManager.Instance.GLSDK.DeleteSaveGame();
  }

  public bool IsSaveExists(string accountName = null, int debugSaveNum = -1)
  {
    string key = getSaveKey(accountName, debugSaveNum);
    if (PlayerPrefs.HasKey(key))
    {
      if (PlayerPrefs.GetString(key) == null || PlayerPrefs.GetString(key) == "")
        return false;
      return true;
    }
    return false;
  }

  // accountName null = current account
  public string GetSaveJSON(string accountName = null, int debugSaveNum = -1)
  {
    /*
    string saveString = null;
    string filePath = getSavePath(accountName);
    try
    {
      if (File.Exists(filePath))
      {
        saveString = File.ReadAllText(filePath);
      }
    }
    catch (Exception e)
    {
      Debug.LogError("[SessionManager] File error when getting save\n"+e.ToString(), this);
    }
  
    if (saveString != null)
    {
      return saveString;
    }
    else // backup
    {
    */
    Debug.Log ("Getting save for "+getSaveKey(accountName, debugSaveNum));
    if (PlayerPrefs.HasKey(getSaveKey(accountName, debugSaveNum)))
    {
      return PlayerPrefs.GetString(getSaveKey(accountName, debugSaveNum));
    }
    else
    {
      return null;
    }
    //}
  }

  public void SetSaveJSON(string saveString, string accountName = null, int debugSaveNum = -1)
  {
    PlayerPrefs.SetString(getSaveKey(accountName, debugSaveNum), saveString);
    PlayerPrefs.Save();
  }

  public string GetSaveTime(string accountName = null, int debugSaveNum = -1)
  {
    string key = getSaveKey(accountName, debugSaveNum, SAVETIME_KEY);
    if (PlayerPrefs.HasKey(key))
    {
      return PlayerPrefs.GetString(key);
    }
    return null;
  }

  public void SetSaveTime(string saveTime = null, string accountName = null, int debugSaveNum = -1)
  {
    string key = getSaveKey(accountName, debugSaveNum, SAVETIME_KEY);
    if (saveTime == null)
      saveTime = DateTime.Now.ToString();
    //Debug.Log("!!!!!!!!!!!!!!" + key + " save time: " + saveTime);
    PlayerPrefs.SetString(key, saveTime);
    PlayerPrefs.Save();
  }

  public string GetSaveNote(string accountName = null, int debugSaveNum = -1)
  {
    string key = getSaveKey(accountName, debugSaveNum, SAVENOTE_KEY);
    if (PlayerPrefs.HasKey(key))
    {
      return PlayerPrefs.GetString(key);
    }
    return null;
  }

  public void SetSaveNote(string note, string accountName = null, int debugSaveNum = -1)
  {
    string key = getSaveKey(accountName, debugSaveNum, SAVENOTE_KEY);
    //Debug.Log("!!!!!!!!!!!!!!" + key + " save note: " + note);
    PlayerPrefs.SetString(key, note);
    PlayerPrefs.Save();
  }

  public Dictionary<string, object> GetSave(string accountName = null)
  {
    return (Dictionary<string, object>) Json.Deserialize(GetSaveJSON(accountName));
  }

  void Update()
  {
    if (m_doSave)
    {
      doSave();
      m_doSave = false;
    }
  }

  public void DebugLoad(int saveNum)
  {
    Load(saveNum);
  }

  public void Load(int debugSaveNum = -1)
  {
    if (!m_busy)
    {
#if !UNITY_EDITOR
      try 
      {
#endif
      m_busy = true;
      float startTime = Time.realtimeSinceStartup;
      string saveString = GetSaveJSON(null, debugSaveNum);

      Debug.Log("[SessionManager] Load:\n" + saveString, this);

      if (saveString == null || saveString == "")
      {
        m_busy = false;
        return;
      }

      Dictionary<string, object> data = (Dictionary<string, object>) Json.Deserialize(saveString);
      string version = "";
      if (data.ContainsKey(VERSION_KEY))
      {
          version = Utility.GetMajorVersionFromVersion((string) data[VERSION_KEY]);
      }

      string currentVersion = GLResourceManager.InstanceOrCreate.GetMajorVersionString();
      bool didMigration = false;
      while (version != currentVersion)
      {
        // Do migrations
        if (SaveMigration.Migrations.ContainsKey(version))
        {
          SaveMigration migrationScript = SaveMigration.Migrations[version];
          saveString = migrationScript.MigrateSave(saveString);
          Debug.Log("[SessionManager] Migrated save from version "+version+" to version "+migrationScript.ToVersion, this);
          version = migrationScript.ToVersion;
          didMigration = true;
        }
        else
        {
          Debug.LogWarning("[SessionManager] Could not migrate save from version "+version+" to version "+currentVersion+". Final version migrated to is "+version, this);
          break;
        }
      }

      if (didMigration)
      {
        if (!string.IsNullOrEmpty(saveString))
          {
            data = (Dictionary<string, object>) Json.Deserialize(saveString);
          }
          else
          {
            // Migration wiped save, return and don't do anything
            m_busy = false;
            return;
          }
      }

        m_didLoad = true;

      PersistTarget[] saveTargets = Resources.FindObjectsOfTypeAll<PersistTarget>();
      for (int i = saveTargets.Length-1; i >= 0; i--)
      {
        PersistTarget saveTarget = saveTargets[i];
        
        if (Utility.IsPrefab(saveTarget.gameObject)) continue;

        if (data.ContainsKey(saveTarget.gameObject.name))
        {
          Dictionary<string, object> jsonData = (Dictionary<string, object>) data[saveTarget.gameObject.name];
          Deserialize(saveTarget.gameObject, jsonData);
        }
      }
    
      Debug.Log("[SessionManager] Load time: " + (Time.realtimeSinceStartup - startTime) * 1000 + "ms", this);
        
        #if !UNITY_EDITOR
      }
      catch (Exception e)
      {
        Debug.LogError("Error while loading game " + e.ToString(), this);
      }
      finally
      {
        #endif
        m_busy = false;
        #if !UNITY_EDITOR
      }
      #endif
    }
  }

  // UNTESTED
  public Dictionary<string, object> GetData(GameObject obj)
  {
    Dictionary<string, object> returnData = null;
    if (m_currentSaveData.ContainsKey(obj.name))
    {
      returnData = m_currentSaveData[obj.name] as Dictionary<string, object>;
    }

    return returnData;
  }

  private const string SPINNER_PREFAB_NAME = "SaveSpinner";
  private SaveSpinner m_spinner;
  private void ShowSpinner(float showTime = 4f)
  {
#if false
    if (IsInvoking("HideSpinner"))
    {
      CancelInvoke("HideSpinner");
    }

    if (m_spinner == null)
    {
      m_spinner = ((GameObject) Instantiate(Resources.Load(SPINNER_PREFAB_NAME))).GetComponent<SaveSpinner>();
    }
    else
    {
      m_spinner.gameObject.SetActive(true);
    }

    Invoke("HideSpinner", showTime);
#endif
  }

  private void HideSpinner()
  {
    m_spinner.gameObject.SetActive(false);
  }
  
  [ContextMenu("Toggle saving for now")]
  public bool ContextMenuToggleSaves() {
    m_ignoreSaves = !m_ignoreSaves;
    Debug.Log ("Saves are now turned "+((m_ignoreSaves)? "off" : "on" )+" for this play session.");
	return m_ignoreSaves;
  }

	public bool SyncContextMenuToggleSaves() {
		return m_ignoreSaves;
	}
  
  [ContextMenu("Save")]
  private void ContextMenuSave() {
    // in this case, always save despite m_ignoreSaves
    m_doSave = true;
  }

  public void Save()
  {
    if (!m_ignoreSaves) m_doSave = true;
  }

	public void DebugSave(int saveNum, string note = "")
	{
		doSave(saveNum, note);
	}

  private void doSave(int debugSaveNum = -1, string note = "")
  {
    if (InitComplete && !m_busy)
    {
      try
      {
        m_busy = true;
        ShowSpinner();
        float startTime = Time.realtimeSinceStartup;

        // Find all Save targets
        // TODO: Use a pre-constructed list that's marked dirty upon PersistTarget.Awake()
        List<PersistTarget> saveTargets = Utility.FindInstancesInScene<PersistTarget>();
        
        //PersistData saveBuffer = new PersistData(this);
        Dictionary<string, object> saveBuffer = new Dictionary<string, object>();
        saveBuffer["__bundleid__"] = GLResourceManager.InstanceOrCreate.GetProjectBundleID();
        saveBuffer[VERSION_KEY] = GLResourceManager.InstanceOrCreate.GetVersionString();
        saveBuffer[AVATAR_KEY] = AccountManager.InstanceOrCreate.GetAvatar().ToString();

        for (int i = saveTargets.Count-1; i >= 0; i--)
        {
          PersistTarget saveTarget = saveTargets[i];

          // Skip ones that aren't in the scene
          if (Utility.IsPrefab(saveTarget.gameObject)) continue;

          saveTarget.SendMessage("OnSave", SendMessageOptions.DontRequireReceiver);

          /*
        if (saveTarget.transform.parent != null) // Only top level objects?
          continue;
        */

          string objectName = saveTarget.gameObject.name;
          // Check if object already exists in blob
          if (saveBuffer.ContainsKey(objectName))
          {
            Debug.LogError("Multiple objects of name '" + objectName + "', previous object(s) will be overwritten.", this);
            Debug.LogError(Utility.GetHierarchyString(saveTarget.gameObject), this);
          }

          saveBuffer[objectName] = Serialize(saveTarget.gameObject); // Serialize object and add to save blob
        }

        string saveString = Json.Serialize (saveBuffer); // Serialize the save blob into JSON

        // Save to player prefs
        SetSaveJSON(saveString, null, debugSaveNum);
        // Save time to player prefs
        SetSaveTime(null, null, debugSaveNum);
        // Save note to player prefs
        SetSaveNote(note, null, debugSaveNum);

        // Save data server-side
        if( !PegasusManager.Instance.GLSDK.GetIsTutorialUser() ) {
          PegasusManager.Instance.GLSDK.SaveGame( saveString );
        }

        m_lastSaveTime = Time.realtimeSinceStartup;

        // Save to file
        //File.WriteAllText(getSavePath(), saveString);
        Debug.Log("[SessionManager] Save time: " + (Time.realtimeSinceStartup - startTime) * 1000 + "ms\n"+saveString, this); // how long it took to save

      }
      catch (Exception e)
      {
        Debug.LogError(e.ToString(), this);
      }
      finally
      {
        m_busy = false;
      }
    }
  }

  public void DeleteSave( string profileName ) {
    // Remove from player preferences and save file
    PlayerPrefs.SetString( getSaveKey( profileName ), "" );
    //File.WriteAllText( getSavePath( profileName ), "" );
  }
  
  /**
   * SERIALIZATION
   */
  
  public static Dictionary<string, object> Serialize(object target)
  {
    if (target is GameObject)
    {
      return SerializeGameObject((GameObject) target);
    }
    else
    {
      return SerializeObject(target); // We don't have deserialization for data classes yet
    }
  }
  /*
  // TODO: Overloads
  public static Dictionary<string, object> Serialize(GameObject target)
  {

  }
  */

  public static bool HasPersistAttributes(object target)
  {
    Type targetType = target.GetType();
    IEnumerable<PropertyInfo> props = targetType.GetProperties().Where(x => x.GetCustomAttributes(typeof(PersistAttribute), false).Length > 0);
    IEnumerable<FieldInfo> fields = targetType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetCustomAttributes(typeof(PersistAttribute), false).Length > 0);

    return fields.Count() > 0 || props.Count() > 0;
  }

  public static Dictionary<string, object> SerializeObject(object target)
  {
    Dictionary<string, object> data = new Dictionary<string, object>();

    Type targetType = target.GetType();
    data["__type__"] = targetType.FullName;

    IEnumerable<PropertyInfo> props = targetType.GetProperties().Where(
      x => x.GetCustomAttributes(typeof(PersistAttribute), false).Length > 0 // TODO: 2nd parameter of false should probably be true, check
      );
    foreach (PropertyInfo prop in props)
    {
      if (prop.PropertyType.IsEnum)
      {
        data[prop.Name] = prop.GetValue(target, null).ToString();
      }
      // MiniJSON already knows how to serialize these types - ICollections [Arrays, lists, dictionaries], value types, strings
      else if (prop.PropertyType == typeof(ICollection) || prop.PropertyType.IsSubclassOf(typeof(ICollection)) ||
               prop.PropertyType.GetInterfaces().Contains(typeof(ICollection)) ||
               prop.PropertyType.IsValueType || prop.PropertyType == typeof(string))
      {
        data[prop.Name] = prop.GetValue(target, null);
      }
      else // Recurse through non-primitive data classes
      {
        data[prop.Name] = Serialize(prop.GetValue(target, null));
      }
    }
    
    IEnumerable<FieldInfo> fields = targetType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(
      x => x.GetCustomAttributes(typeof(PersistAttribute), false).Length > 0
      );
    foreach (FieldInfo field in fields)
    {
      if (field.FieldType.IsEnum)
      {
        data[field.Name] = field.GetValue(target).ToString();
      }
      // MiniJSON already knows how to serialize these types - ICollections [Arrays, lists, dictionaries], value types, strings
      else if (field.FieldType.IsValueType || field.FieldType == typeof(string))
      {
        data[field.Name] = field.GetValue(target);
      }
      else if (field.FieldType == typeof(ICollection) || field.FieldType.IsSubclassOf(typeof(ICollection)) ||
               field.FieldType.GetInterfaces().Contains(typeof(ICollection)))
      {
        if (field.FieldType.IsGenericType)
        {
          Type genericType = field.FieldType.GetGenericTypeDefinition();
          if (genericType == typeof(List<>))
          {
            // TODO deserializeList
            Type valueType = field.FieldType.GetGenericArguments()[0];
            if (valueType.IsEnum)
            {
              data[field.Name] = field.GetValue(target);
            }
            else
            {
              data[field.Name] = field.GetValue(target);
            }
          }
          else if (genericType == typeof(Dictionary<,>))
          {
            // TODO deserializeDictionary
            data[field.Name] = field.GetValue(target);
          }
        }
        else
        {
          data[field.Name] = field.GetValue(target);
        }
      }
      else // Recurse through non-primitive data classes
      {
        data[field.Name] = Serialize(field.GetValue(target));
      }
    }

    return data;
  }

  private static Dictionary<string, object> SerializeGameObject(GameObject target)
  {
    Dictionary<string, object> data = new Dictionary<string, object>();

    data["__type__"] = typeof(GameObject).FullName;

    data[ACTIVE_KEY] = target.activeSelf;
    
    // Reflect Attributes
    MonoBehaviour[] components = target.GetComponents<MonoBehaviour>();
    
    for (int i=components.Length-1; i >= 0; i--)
    {
      MonoBehaviour component = components[i];
      data[component.GetType().FullName] = Serialize(component);
    }
    
    // Get children, store them
    
    return data;
  }

  /**
   * DESERIALIZATION
   */

  private static void DeserializeGameObject(GameObject target, Dictionary<string,object> objectData)
  {
    // Set active from save data
    target.SetActive((bool) objectData[ACTIVE_KEY]);


    MonoBehaviour[] components = target.GetComponents<MonoBehaviour>();
    // We assume the components already exist
    for (int i=components.Length-1; i >= 0; i--)
    {
      MonoBehaviour component = components[i];

      string componentName = component.GetType().FullName;
      if (!objectData.ContainsKey(componentName))
      {
        continue;
      }
      Dictionary<string, object> componentData = (Dictionary<string, object>) objectData[componentName];
      Deserialize(component, componentData);
    }

  }

  public static void Deserialize(object target, Dictionary<string,object> data)
  {
    if (target is GameObject)
    {
      DeserializeGameObject((GameObject) target, data);
    }
    else // Custom data class
    {
      DeserializeObject(target, data);
    }
  }

  public static object DeserializeNew(object data, Type targetType)
  {
    if (data == null)
    {
      return null;
    }

    if (targetType.IsArray)
    {
      return deserializeArray(data, targetType);
    }
    else if (targetType.IsGenericType)
    {
      Type genericType = targetType.GetGenericTypeDefinition();
      if (genericType == typeof(List<>))
      {
        return deserializeList(data, targetType);
      }
      else if (genericType == typeof(Dictionary<,>))
      {
        return DeserializeDictionary(data, targetType);
      }
    }
    else if (targetType.IsEnum)
    {
      return Enum.Parse(targetType, (string) data);
    }
    else
    {
      if (targetType.IsValueType || targetType == typeof(string))
      {
        if (data.GetType() != targetType)
        {
          return Convert.ChangeType(data, targetType);
        }
        else
        {
          return data;
        }
      }
      else
      {
        // Data classes
        Dictionary<string, object> objectData = (Dictionary<string, object>) data;
        Type objectType = Type.GetType((string) objectData["__type__"]);
        ConstructorInfo valueConstructor = objectType.GetConstructor(Type.EmptyTypes);
        object deserializedValue = valueConstructor.Invoke(new object[] { });
        //DeserializeJSON(deserializedValue, (string) data);
        Deserialize(deserializedValue, objectData);
        
        return deserializedValue;
      }
    }

    // Unreachable, but compiler can't tell for some reason
    return null;
  }

  public static void DeserializeObject(object target, Dictionary<string,object> componentData)
  {
    Type targetType = target.GetType();
    
    // basic declared variables
    foreach (string fieldName in componentData.Keys)
    {
      // Skip typeinfo data
      if (fieldName == "__type__")
      {
        continue;
      }

      object data = componentData[fieldName];

      // Try putting in a field
      FieldInfo field = targetType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      if (field != null)
      {
        field.SetValue(target, DeserializeNew(data, field.FieldType));
      }
      else
      {
        // If no field, put in a property
        PropertyInfo prop = targetType.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop != null)
        {
          prop.SetValue(target, DeserializeNew(data, prop.PropertyType), null);
        }
        else
        {
          Debug.LogError("[SessionManager] Could not find field or property '"+fieldName+"' in "+targetType.FullName);
          continue;
        }
      }
    }
  }
  
  private static IList deserializeList(object data, Type fieldType)
  {
    Type valueType = fieldType.GetGenericArguments()[0];
    List<object> list = (List<object>) data;
    
    Type genericList = typeof(List<>); // Generic list type
    Type specificList = genericList.MakeGenericType(valueType); // Specific list type
    ConstructorInfo ci = specificList.GetConstructor(Type.EmptyTypes); // Reflect constructor for specific list type (List <t>)
    IList newList = (IList) ci.Invoke(new object[] { }); // Instantiate list
    
    // Re-add elements to list
    int listCount = list.Count;

    for (int j = 0; j < listCount; j++)
    {
      newList.Add(DeserializeNew(list[j], valueType));
    }
    /*
    if (valueType.IsValueType || valueType == typeof(string))
    {
      for (int j = 0; j < listCount; j++)
      {
        newList.Add(Convert.ChangeType(list[j], valueType));
      }
    }
    else
    {
      for (int j = 0; j < listCount; j++)
      {
        Dictionary<string, object> objectData = (Dictionary<string, object>) list[j];
        string typeString = (string) objectData["__type__"];
        Type objectType = Type.GetType(typeString);
        ConstructorInfo valueConstructor = objectType.GetConstructor(Type.EmptyTypes);
        object deserializedValue = valueConstructor.Invoke(new object[] { });
        //DeserializeJSON(deserializedValue, (string) list[j]);
        Deserialize(deserializedValue, objectData);
        newList.Add(deserializedValue);
      }
    }
    */
    
    return newList;
  }
  
  private static Array deserializeArray(object data, Type fieldType)
  {
    List<object> array = (List<object>) data;
    Type t = fieldType.GetElementType();
    int numElements = array.Count;
    var convertedArray = Array.CreateInstance(t, numElements);
    for (int j=0; j < numElements; j++)
    {
      convertedArray.SetValue(DeserializeNew(array[j], t), j);
    }

    return convertedArray;
  }

  // Refactor to take (object target, Dictionary<string, object> data)
  public static IDictionary DeserializeDictionary(object data, Type fieldType)
  {
    Type keyType = fieldType.GetGenericArguments()[0];
    Type valueType = fieldType.GetGenericArguments()[1];
    Dictionary<string,object> dict = (Dictionary<string,object>) data;
    
    Type genericDictionary = typeof(Dictionary<,>); // Generic type
    Type specificDictionary = genericDictionary.MakeGenericType(keyType, valueType); // Specific type
    ConstructorInfo ci = specificDictionary.GetConstructor(Type.EmptyTypes); // Reflect constructor for specific type
    IDictionary newDict = (IDictionary) ci.Invoke(new object[] { }); // Instantiate
    
    // Re-add elements to list
    List<string> keys = new List<string>(dict.Keys);
    int keyCount = keys.Count;
    string key;

    for (int j=0; j < keyCount; j++)
    {
      key = keys[j];
      newDict[Convert.ChangeType(key, keyType)] = DeserializeNew(dict[key], valueType);
    }
    /*
    // Could be optimized by running through the types below, but using DeserializeNew for code cleanliness.
    // TODO: Write an optimized function for this case
    if (valueType.IsArray)
    {
      for (j = 0; j < keyCount; j++)
      {
        key = keys[j];
        newDict[Convert.ChangeType(key, keyType)] = deserializeArray(dict[key], valueType);
      }
    }
    else if (valueType.IsGenericType)
    {
      Type genericType = valueType.GetGenericTypeDefinition();
      if (genericType == typeof(List<>))
      {
        for (j = 0; j < keyCount; j++)
        {
          key = keys[j];
          newDict[Convert.ChangeType(key, keyType)] = deserializeList(dict[key], valueType);
        }
      }
      else if (genericType == typeof(Dictionary<,>))
      {
        for (j = 0; j < keyCount; j++)
        {
          key = keys[j];
          newDict[Convert.ChangeType(key, keyType)] = DeserializeDictionary(dict[key], valueType);
        }
      }
      else
      {
        // Dunno? (Unhandled generics case)
      }
      
    }
    else if (valueType.IsValueType || valueType == typeof(string))
    {
      for (j = 0; j < keyCount; j++)
      {
        key = keys[j];
        newDict[Convert.ChangeType(key, keyType)] = Convert.ChangeType(dict[key], valueType);
      }
    }
    else
    {
      for (j = 0; j < keyCount; j++)
      {
        key = keys[j];
        Dictionary<string, object> objectData = (Dictionary<string, object>) dict[key];
        string typeString = (string) objectData["__type__"];
        Type objectType = Type.GetType(typeString);
        ConstructorInfo valueConstructor = objectType.GetConstructor(Type.EmptyTypes);
        object deserializedValue = valueConstructor.Invoke(new object[] { });
        //DeserializeJSON(deserializedValue, (string) dict[key]);
        Deserialize(deserializedValue, objectData);
        newDict[Convert.ChangeType(key, keyType)] = deserializedValue;
      }
    }
    */
    return newDict;
  }
}
