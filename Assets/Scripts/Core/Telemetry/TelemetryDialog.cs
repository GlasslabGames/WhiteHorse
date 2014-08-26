using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TelemetryDialog : MonoBehaviour {
  private Queue<string> m_messages = new Queue<string>();
  public UILabel Label;
  public GameObject DialogParent;
  public int MaxMessages;

	void Awake() {
    TapSequenceListener listener = GetComponent<TapSequenceListener>();
    if (listener != null) { listener.Callback = Open; }

    bool open;
    if (GlobalData.ValueExists<bool>("DEBUG_TELEMETRY_OPEN")) {
      open = GlobalData.GetValue<bool>("DEBUG_TELEMETRY_OPEN");
    } else {
      open = DialogParent.gameObject.activeSelf;
    }

    if (open) Open ();
    else Close ();
  }

  public void Open() {
    GlobalData.SetValue("DEBUG_TELEMETRY_OPEN", true); 
    if (DialogParent != null) {
      DialogParent.gameObject.SetActive(true);
    }
  }

  public void Close() {
    GlobalData.SetValue("DEBUG_TELEMETRY_OPEN", false); 
    if (DialogParent != null) {
      DialogParent.gameObject.SetActive(false);
    }
  }

  void OnEnable() {
    GlasslabSDK.TelemetryOutput += OnTelemOutput;
  }

  void OnDisable() {
    GlasslabSDK.TelemetryOutput -= OnTelemOutput;
  }

  void OnTelemOutput(string output) {
    Write (output);
  }

  public void Write(string message) {
    m_messages.Enqueue(message);
    if (m_messages.Count > MaxMessages) {
      m_messages.Dequeue();
    }

    if (Label != null) {
      Label.text = string.Join("\n", m_messages.ToArray());
    }
  }
}
