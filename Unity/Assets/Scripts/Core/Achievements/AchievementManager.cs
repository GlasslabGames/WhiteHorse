using UnityEngine;
using System.Collections.Generic;
using MiniJSON;
using GlassLab.Core.Serialization;

public class AchievementManager : SingletonBehavior<AchievementManager> 
{
  private const string INIT_DATA_PATH = "Achievements";

  private Dictionary<string, Achievement> m_achievements = new Dictionary<string, Achievement>();

  // Dictionary of completed achievements
  [PersistAttribute]
  private Dictionary<string, bool> m_completedAchievements = new Dictionary<string, bool>();

  private AchievementManager() {}

  override protected void Awake()
  {
    TextAsset data = Resources.Load<TextAsset>(INIT_DATA_PATH);
    loadAchievements(data);

    SignalManager.AchievementUnlocked += onAchievementUnlocked;

    base.Awake();
  }

  private void loadAchievements(TextAsset sourceData)
  {
    Dictionary<string, object> ddata = (Dictionary<string, object>) Json.Deserialize(sourceData.text);
    m_achievements = (Dictionary<string, Achievement>) SessionDeserializer.DeserializeDictionary(ddata, m_achievements.GetType());

    foreach (string key in m_achievements.Keys)
    {
      if (m_completedAchievements.ContainsKey(key))
      {
        m_achievements[key].IsCompleted = m_completedAchievements[key];
      }

      m_achievements[key].Init();
    }
  }

  private void onAchievementUnlocked(Achievement a)
  {
    //Debug.Log("Achievement unlocked: "+a.Name + ", "+a.Group + ", " + a.SubGroup);
    GlasslabSDK.Instance.SaveAchievement(a.Name, a.Group, a.SubGroup);
  }

  override protected void OnDestroy()
  {
    SignalManager.AchievementUnlocked -= onAchievementUnlocked;

    base.OnDestroy();
  }
}
