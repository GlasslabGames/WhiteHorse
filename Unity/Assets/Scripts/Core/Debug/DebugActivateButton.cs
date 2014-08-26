using UnityEngine;
using System.Collections;

public class DebugActivateButton : MonoBehaviour {

  public GameObject ActivateButton;
  private bool isActivated = false;
  public const string LOG_NUM_KEY = "DebugLogNum";

  void Awake() {
    if (ActivateButton != null)
      DontDestroyOnLoad(ActivateButton);
  }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

  public void Click ()
  {
    if (enabled && !isActivated)
    {
      StartDebugMenu();
      isActivated = true;
      if (ActivateButton != null)
        ActivateButton.SetActive(false);
    }
  }
  
  void OnClick () // NGUI
  {
    Click ();
  }
  
  void OnMouseUpAsButton () // non-NGUI
  {
    Click ();
  }

  void StartDebugMenu()
  {
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
  }
}
