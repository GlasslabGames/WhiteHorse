using UnityEngine;
using System;

public class ParentChangedListener : MonoBehaviour
{
  public event Action<Transform,Transform> OnParentChangedLastFrame; // args => [Previous parent, new parent]
  private Transform m_parent;

  void Awake()
  {
    m_parent = transform.parent;
  }

  void Update()
  {
    if (m_parent != transform.parent)
    {
      if (OnParentChangedLastFrame != null) OnParentChangedLastFrame(m_parent, transform.parent);
      m_parent = transform.parent;
    }
  }
}

