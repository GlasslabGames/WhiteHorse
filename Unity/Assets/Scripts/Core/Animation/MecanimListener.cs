
using UnityEngine;
using System;

public class MecanimListener : MonoBehaviour 
{
  private Animator m_animator;
  public int CurrentAnimationHash { get; private set; }
  private bool m_eventSignaled;

  public event Action AnimationFinishedOnceOnly; // ONCE-ONLY EVENT (listeners are cleared after event occurs)
  public event Action AnimationFinished;

//  public const string MAP_IDLE_ANIMATION = "Map_roomIdle";

  void Awake()
  {
    m_animator = GetComponentInChildren<Animator>();

    if (m_animator == null)
    {
      Debug.LogError("[MecanimListener] Could not find Animator component", this);
      enabled = false;
    }
  }

  public void Play(string animation, Action callback = null)
  {
    if (m_animator == null)
    {
      m_animator = GetComponentInChildren<Animator>();
    }

//    m_animator.Play (MAP_IDLE_ANIMATION);
//    m_animator.enabled = false;
//    m_animator.enabled = true;
//    // If same animation
//    if (Animator.StringToHash(animation) == CurrentAnimationHash)
//    {
//      m_animator.playbackTime = 0f;
//    }
//    else
//    {
    m_animator.Play( animation );
//    }

    AnimationFinishedOnceOnly = callback;
  }

  public void PlayFromStart(string animation, Action callback = null)
  {
    if (m_animator == null)
    {
      m_animator = GetComponentInChildren<Animator>();
    }

    m_animator.Play( animation, -1, 0);
    
    AnimationFinishedOnceOnly = callback;
  }

  // HACK - Check on update to see whether the animation has completed
  // WARNING: AnimationFinished events aren't cleared when animation is changed. Due to black-box nature of mecanim,
  //          you must clear the event list yourself when making a call to play an animation.
  void Update()
  {
    AnimatorStateInfo currentAnimatorState = m_animator.GetCurrentAnimatorStateInfo(0);

    if (currentAnimatorState.nameHash != CurrentAnimationHash)
    {
      CurrentAnimationHash = currentAnimatorState.nameHash;
      m_eventSignaled = false;
    }

    if (!m_eventSignaled)
    {
      if (currentAnimatorState.normalizedTime >= 1f)
      {
        m_eventSignaled = true;
        if (AnimationFinished != null)
        {
          AnimationFinished();
        }

        if (AnimationFinishedOnceOnly != null)
        {
          AnimationFinishedOnceOnly();
          AnimationFinishedOnceOnly = null;
        }
      }
    }
  }

  void OnDisable()
  {
    m_animator.enabled = false;
  }

  void OnEnable()
  {
    m_animator.enabled = true;
  }
}