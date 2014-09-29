using UnityEngine;
using System;
using System.Collections.Generic;
using MiniJSON;

namespace GlassLab.Core.Serialization
{
  public class SaveMigration
  {
    private delegate string MigrationDelegate(string saveData);
    public static Dictionary<string, SaveMigration> Migrations = new Dictionary<string, SaveMigration>()
    {
      /*
      {"EXAMPLE", new SaveMigration("BEFORE_VERSION", "AFTER_VERSION", delegate(string saveData) {
          Dictionary<string, object> data = (Dictionary<string, object>) Json.Deserialize(saveData);
          return saveData;
        })
      }
      */
    };

    private static bool hasDataAtPath(string path, Dictionary<string, object> data)
    {
      string[] pathLevels = path.Split(new char[] { '/' });
      int pathIndex = 0;
      Dictionary<string, object> currentLevel = data;
      while (pathIndex < pathLevels.Length)
      {
        string currentKey = pathLevels[pathIndex];
        if (currentLevel.ContainsKey(currentKey))
        {
          if (pathIndex == pathLevels.Length - 1)
          {
            return true;
          }
          else
          {
            currentLevel = (Dictionary<string, object>)currentLevel[currentKey];
            pathIndex++;
          }
        }
        else
        {
          return false;
        }
      }
      // this shouldn't be reachable
      Debug.LogError("[SaveMigration] Something went wrong.");
      return false;
    }

    private static void moveData(string srcPath, string dstPath, Dictionary<string, object> data)
    {
      object movedData = getData<object>(srcPath, data);
      if (movedData != null)
      {
        setData(dstPath, data, movedData);
        deleteData(srcPath, data);
      }
      else
      {
        Debug.LogError("[SaveMigration] No data found at " + srcPath);
      }
    }

    private static void setData(string path, Dictionary<string, object> targetData, object data)
    {
      string[] pathLevels = path.Split(new char[] { '/' });
      int pathIndex = 0;
      Dictionary<string, object> currentLevel = targetData;
      while (pathIndex < pathLevels.Length)
      {
        string currentKey = pathLevels[pathIndex];
        if (currentLevel.ContainsKey(currentKey))
        {
          if (pathIndex == pathLevels.Length - 1)
          {
            currentLevel[currentKey] = data;
            return;
          }
          else
          {
            currentLevel = (Dictionary<string, object>)currentLevel[currentKey];
            pathIndex++;
          }
        }
        else
        {
          if (pathIndex == pathLevels.Length - 1)
          {
            currentLevel[currentKey] = data;
            return;
          }
          else
          {
            Debug.LogWarning("[SaveMigration] Could not find '" + currentKey + "' in " + path + ", creating and continuing...");

            Dictionary<string, object> newLevel = new Dictionary<string, object>();
            currentLevel[currentKey] = newLevel;
            currentLevel = newLevel;
            pathIndex++;
          }
        }
      }

      // this shouldn't be reachable
      Debug.LogError("[SaveMigration] Something went wrong.");
    }

    private static T getData<T>(string path, Dictionary<string, object> data)
    {
      string[] pathLevels = path.Split(new char[] { '/' });
      int pathIndex = 0;
      Dictionary<string, object> currentLevel = data;
      while (pathIndex < pathLevels.Length)
      {
        string currentKey = pathLevels[pathIndex];
        if (currentLevel.ContainsKey(currentKey))
        {
          if (pathIndex == pathLevels.Length - 1)
          {
            return (T)currentLevel[currentKey];
          }
          else
          {
            currentLevel = (Dictionary<string, object>)currentLevel[currentKey];
            pathIndex++;
          }
        }
        else
        {
          Debug.LogError("[SaveMigration] Could not find data at " + path);
          return default(T);
        }
      }

      // this shouldn't be reachable
      Debug.LogError("[SaveMigration] Something went wrong.");
      return default(T);
    }
    private static bool deleteData(string path, Dictionary<string, object> data)
    {
      string[] pathLevels = path.Split(new char[] { '/' });
      int pathIndex = 0;
      Dictionary<string, object> currentLevel = data;
      while (pathIndex < pathLevels.Length)
      {
        string currentKey = pathLevels[pathIndex];
        if (currentLevel.ContainsKey(currentKey))
        {
          if (pathIndex == pathLevels.Length - 1)
          {
            currentLevel.Remove(currentKey);
            return true;
          }
          else
          {
            currentLevel = (Dictionary<string, object>)currentLevel[currentKey];
            pathIndex++;
          }
        }
        else
        {
          return false;
        }
      }

      // this shouldn't be reachable
      Debug.LogError("[SaveMigration] Something went wrong.");
      return false;
    }


    public string FromVersion { get; private set; }

    public string ToVersion { get; private set; }

    private MigrationDelegate m_migrationFunction;

    private SaveMigration(string fromVersion, string toVersion, MigrationDelegate migrationFunction)
    {
      FromVersion = fromVersion;
      ToVersion = toVersion;
      m_migrationFunction = migrationFunction;
    }

    public string MigrateSave(string saveJson)
    {
      Debug.Log("Migrating \n" + saveJson);
      saveJson = m_migrationFunction(saveJson);

      if (!string.IsNullOrEmpty(saveJson))
      {
        Dictionary<string, object> data = (Dictionary<string, object>)Json.Deserialize(saveJson);
        data[SessionManager.VERSION_KEY] = ToVersion;
        saveJson = Json.Serialize(data);
      }

      Debug.Log("After migration from " + FromVersion + " to " + ToVersion + ",\n " + saveJson);
      return saveJson;
    }
  }
}