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

namespace GlassLab.Core.Serialization
{
  public class SessionManager : SingletonBehavior<SessionManager>
  {
    public const string SAVE_KEY = "HIRO_SAVE";
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

    public float TimeSinceLastSave
    {
      get
      {
        return Time.realtimeSinceStartup - m_lastSaveTime;
      }
    }

    public Dictionary<string, object> m_currentSaveData;

    [HideInInspector]
    public bool InitComplete = false;

    private SessionManager() {}

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
        Debug.LogError("SessionManager.IgnoreSaves should not be set to true permanently. Only check the box during play so it isn't saved into the scene.", this);
      }

      if (DebugLoadFlag != NONE_SELECT)
      {
        int saveNum = DebugLoadFlag;
        if (saveNum != -1)
          m_ignoreSaves = true;
        DebugLoadFlag = NONE_SELECT;
        DebugLoad(saveNum);
      }
      else
      {
        Load();
      }
    }

    override protected void Start()
    {
      base.Start();
      if (m_didLoad)
      {
        List<PersistTarget> saveTargets = Utility.FindInstancesInScene<PersistTarget>();
        for (int i = saveTargets.Count - 1; i >= 0; i--)
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
      if (PlayerPrefs.HasKey(getSaveKey(accountName, debugSaveNum)))
      {
        return PlayerPrefs.GetString(getSaveKey(accountName, debugSaveNum));
      }
      else
      {
        return null;
      }
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

        Dictionary<string, object> data = (Dictionary<string, object>)Json.Deserialize(saveString);
        string version = "";
        if (data.ContainsKey(VERSION_KEY))
        {
          version = Utility.GetMajorVersionFromVersion((string)data[VERSION_KEY]);
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
            Debug.Log("[SessionManager] Migrated save from version " + version + " to version " + migrationScript.ToVersion, this);
            version = migrationScript.ToVersion;
            didMigration = true;
          }
          else
          {
            Debug.LogWarning("[SessionManager] Could not migrate save from version " + version + " to version " + currentVersion + ". Final version migrated to is " + version, this);
            break;
          }
        }

        if (didMigration)
        {
          if (!string.IsNullOrEmpty(saveString))
          {
            data = (Dictionary<string, object>)Json.Deserialize(saveString);
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
        for (int i = saveTargets.Length - 1; i >= 0; i--)
        {
          PersistTarget saveTarget = saveTargets[i];

          if (Utility.IsPrefab(saveTarget.gameObject)) continue;

          if (data.ContainsKey(saveTarget.gameObject.name))
          {
            Dictionary<string, object> jsonData = (Dictionary<string, object>)data[saveTarget.gameObject.name];
            SessionDeserializer.Deserialize(saveTarget.gameObject, jsonData);
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

    [ContextMenu("Toggle saving for now")]
    public bool ContextMenuToggleSaves()
    {
      m_ignoreSaves = !m_ignoreSaves;
      Debug.Log("Saves are now turned " + ((m_ignoreSaves) ? "off" : "on") + " for this play session.");
      return m_ignoreSaves;
    }

    public bool SyncContextMenuToggleSaves()
    {
      return m_ignoreSaves;
    }

    [ContextMenu("Save")]
    private void ContextMenuSave()
    {
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
          float startTime = Time.realtimeSinceStartup;

          // Find all Save targets
          // TODO: Use a pre-constructed list that's marked dirty upon PersistTarget.Awake()
          List<PersistTarget> saveTargets = Utility.FindInstancesInScene<PersistTarget>();

          //PersistData saveBuffer = new PersistData(this);
          Dictionary<string, object> saveBuffer = new Dictionary<string, object>();
          saveBuffer["__bundleid__"] = GLResourceManager.InstanceOrCreate.GetProjectBundleID();
          saveBuffer[VERSION_KEY] = GLResourceManager.InstanceOrCreate.GetVersionString();
          saveBuffer[AVATAR_KEY] = AccountManager.InstanceOrCreate.GetAvatar().ToString();

          for (int i = saveTargets.Count - 1; i >= 0; i--)
          {
            PersistTarget saveTarget = saveTargets[i];

            // Skip ones that aren't in the scene
            if (Utility.IsPrefab(saveTarget.gameObject)) continue;

            saveTarget.SendMessage("OnSave", SendMessageOptions.DontRequireReceiver);

            string objectName = saveTarget.gameObject.name;
            // Check if object already exists in blob
            if (saveBuffer.ContainsKey(objectName))
            {
              Debug.LogError("Multiple objects of name '" + objectName + "', previous object(s) will be overwritten.", this);
              Debug.LogError(Utility.GetHierarchyString(saveTarget.gameObject), this);
            }

            saveBuffer[objectName] = SessionSerializer.Serialize(saveTarget.gameObject); // Serialize object and add to save blob
          }

          string saveString = Json.Serialize(saveBuffer); // Serialize the save blob into JSON

          // Save to player prefs
          SetSaveJSON(saveString, null, debugSaveNum);
          // Save time to player prefs
          SetSaveTime(null, null, debugSaveNum);
          // Save note to player prefs
          SetSaveNote(note, null, debugSaveNum);

          // Save data server-side
          if (!PegasusManager.Instance.GLSDK.GetIsTutorialUser())
          {
            PegasusManager.Instance.GLSDK.SaveGame(saveString);
          }

          m_lastSaveTime = Time.realtimeSinceStartup;

          // Save to file
          //File.WriteAllText(getSavePath(), saveString);
          Debug.Log("[SessionManager] Save time: " + (Time.realtimeSinceStartup - startTime) * 1000 + "ms\n" + saveString, this); // how long it took to save

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
  }
}