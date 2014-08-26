using UnityEngine;
using System.Collections;

public class PlayAnimationWithInterval : MonoBehaviour {

  public Animator AnimatorPlayed;
  public float FirstWaitingTime = 5f;
  public float Interval = 3f;
  public bool StopWhenDragBegin = false;

  public bool m_isPlaying = false;
  private float m_currentTime;
  private float m_currentInterval;

  private GLDragDropItem m_dragDrop;
  
  void Awake() {
    m_dragDrop = GetComponent<GLDragDropItem>();
  }

  void OnEnable() {
    m_isPlaying = false;
    m_currentTime = 0f;
    m_currentInterval = FirstWaitingTime;
    if (m_dragDrop != null) {
      m_dragDrop.OnDragStarted += OnDragBegin;
    }
  }  

  void OnDisable() {
    if (m_dragDrop != null) {
      m_dragDrop.OnDragStarted -= OnDragBegin;
    }
  }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
    if (m_isPlaying && AnimatorPlayed != null)
    {
      m_currentTime += Time.deltaTime;
      if (m_currentTime > m_currentInterval) Play (true);
    }
	}

  public void Play(bool now = false){
    m_isPlaying = true;
    if (now && AnimatorPlayed != null) {
      AnimatorPlayed.SetTrigger("PLAY");
      m_currentInterval = Interval;
      m_currentTime = 0;
    }
  }

  public void Stop(){
    m_isPlaying = false;
  }

  protected void OnDragBegin(GLDragEventArgs args = null) {
    if (StopWhenDragBegin)
      Stop();
  }
}
