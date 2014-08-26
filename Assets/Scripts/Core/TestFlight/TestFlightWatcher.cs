using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SleepQuitWatcher))]
public class TestFlightWatcher : MonoBehaviour {
  // We're using this instead of doing it ourselves because of its editor integration
  protected SleepQuitWatcher m_sleepQuitWatcher;
	
	void Start () {
    m_sleepQuitWatcher = GetComponent<SleepQuitWatcher>();
    m_sleepQuitWatcher.OnApplicationPauseReceived += OnApplicationPauseReceived;
    m_sleepQuitWatcher.OnApplicationQuitReceived += OnApplicationQuitReceived;
    TestFlightBinding.FlushSecondsInterval (30);
  }
	
	// Update is called once per frame
	void Update () {
	  
	}

  void OnApplicationPauseReceived(bool paused) {
    TestFlightBinding.Log("Session " + ((paused) ? "not" : "") + " paused");
  }

  void OnApplicationQuitReceived() {
    TestFlightBinding.Log("Session terminated");
  }

  public void OnMemoryWarning(string message) {
    TestFlightBinding.Log("Memory warning received");
  }
}
