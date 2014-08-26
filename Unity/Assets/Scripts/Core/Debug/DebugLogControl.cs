using UnityEngine;
using System.Collections;

public class DebugLogControl : MonoBehaviour {

  public GameObject InputLogPanel;
  public UIInput InputLog;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

  public void ShowInputLogPanel()
  {
    if (InputLogPanel != null)
      InputLogPanel.SetActive(true);
  }
  
  public void HideInputLogPanel()
  {
    if (InputLog != null) 
      InputLog.value = InputLog.defaultText;
    if (InputLogPanel != null)
      InputLogPanel.SetActive(false);
  }
  
  public void AddLog()
  {
    string note = "";
    if (InputLog != null) 
      note = InputLog.value;
    if (note == null) note = "";
    HideInputLogPanel();
    Debug.Log(note);
  }
}
