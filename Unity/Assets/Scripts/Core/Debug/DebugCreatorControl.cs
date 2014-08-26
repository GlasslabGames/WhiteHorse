//#define GAME_RELEASE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugCreatorControl : MonoBehaviour {

	//public GameObject DebugManager;
  //public const string LOG_NUM_KEY = "DebugLogNum";
  public const string ACTIVE_BUTTON_NAME = "DebugActivateButton";
#if UNITY_EDITOR
  public bool DebugMenuSwitch = false;
#endif

  void Awake()
  {
    #if GAME_RELEASE
    gameObject.SetActive(false);
    return;
    #endif

    /*
    GameObject debugManager = GameObject.FindGameObjectWithTag(DebugManager.tag);
    if (debugManager == null)
    {
      if (PlayerPrefs.HasKey(LOG_NUM_KEY))
      {
        int logNum = PlayerPrefs.GetInt(LOG_NUM_KEY);
        DebugSystemManager.DebugLogNum = logNum;
        PlayerPrefs.SetInt(LOG_NUM_KEY, logNum + 1);
        PlayerPrefs.Save();
      }
      else
      {
        DebugSystemManager.DebugLogNum = 0;
        PlayerPrefs.SetInt(LOG_NUM_KEY, 0);
        PlayerPrefs.Save();
      }
      Instantiate(DebugManager);
    }
    else {
      DebugOpenButton openButton = debugManager.GetComponentInChildren<DebugOpenButton>();
      if (openButton != null)
        openButton.Close();
      DebugSystemManager.Instance.CurrentSaveNum = -1;
    }
    */
    /*
    if (DebugSystemManager.Instance == null)
    {
      int logNum = 0;
      if (PlayerPrefs.HasKey(LOG_NUM_KEY))
        logNum = PlayerPrefs.GetInt(LOG_NUM_KEY);
      DebugSystemManager.DebugLogNum = logNum;
      PlayerPrefs.SetInt(LOG_NUM_KEY, logNum + 1);
      PlayerPrefs.Save();
    }
    DebugSystemManager.InstanceOrCreate.CurrentSaveNum = -1;
    DebugOpenButton openButton = DebugSystemManager.InstanceOrCreate.gameObject.GetComponentInChildren<DebugOpenButton>();
    if (openButton != null)
      openButton.Close();
    */
    TapSequenceListener tapListener = GetComponent<TapSequenceListener>();
    tapListener.Callback = SwitchDebug;
  }

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
#if UNITY_EDITOR
    if (DebugMenuSwitch) {
      DebugMenuSwitch = false;
      SwitchDebug();
    }
#endif
	}

  public void SwitchDebug() {
    // if first time, create Activate button
    List<DebugActivateButton> instances = Utility.FindInstancesInScene<DebugActivateButton>();
    if (instances == null || instances.Count == 0)
    {
      GameObject prefab = Resources.Load(ACTIVE_BUTTON_NAME) as GameObject;
      if (prefab != null)
      {
        Instantiate(prefab);
      }
    } else if (DebugSystemManager.Instance == null) {
      foreach (var instance in instances)
        instance.gameObject.SetActive(!instance.gameObject.activeSelf); // should only be one button
    }
    // if created before, switch active
    if (DebugSystemManager.Instance != null)
    {
      bool wasActive = DebugSystemManager.Instance.OpenButton.gameObject.activeSelf;
      if (wasActive)
        DebugSystemManager.Instance.OpenButton.Close();
      DebugSystemManager.Instance.OpenButton.gameObject.SetActive(!wasActive);
    }
  }
}
