using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(GLAfterEffectsAnimationController))]

/// <summary>
/// Tells an animator to play random animations with a random interval between them (e.g. for idle animations.)
/// </summary>
public class RandomAnimator : MonoBehaviour {
  private GLAfterEffectsAnimationController m_animationController;
  public List<string> m_animationNames; // list of all the states we want to randomly choose between
  public float m_minWaitTime; // minimum amount of time to wait before playing the next anim
  public float m_maxWaitTime; // maximum amount of ""
  public bool StartActive = false;

  private bool m_active;
  public bool Active {
    get { return m_active; }
    set {
      if (value != m_active) {
        if (value)
        {
          // start random playing
          StartCoroutine(doPlayAnimation());
        }
        else
        {
          // if we're setting active to false, don't animate any more
          StopAllCoroutines();
        }

        m_active = value;
      }
    }
  }

	void Awake () {
    m_animationController = GetComponent<GLAfterEffectsAnimationController>();
	}

  void OnEnable()
  {
    if (StartActive)
    {
      Active = true;
    }
  }

  void OnDisable()
  {
    StartActive = Active; // So we start again when we come back if necessary
    Active = false;
  }

  void PlayAnimation() {
    if (Active) {
      if (m_animationController.IsPlaying)
      {
        Debug.LogWarning("RandomAnimator on '"+gameObject.name+"' trying to play something while another animation is playing!", this);
        return;
      }

      m_animationController.PlayAnimation(GetRandomAnimationName());
    }
  }

  private IEnumerator doPlayAnimation()
  {
    yield return new WaitForSeconds(UnityEngine.Random.Range(0, m_maxWaitTime));

    while (true)
    {
      yield return null; // Skip a frame since animation may be finishing this frame
      PlayAnimation();
      yield return new WaitForSeconds(UnityEngine.Random.Range (m_minWaitTime, m_maxWaitTime) + m_animationController.GetRemainingAnimationTime());
    }
  }

  public string GetRandomAnimationName()
  {
    int r = UnityEngine.Random.Range(0, m_animationNames.Count);
    return m_animationNames[r];
  }
}
