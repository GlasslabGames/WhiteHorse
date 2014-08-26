using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DisableComponentsOnDrag : MonoBehaviour {
  public List<Component> Components;

  private void setTargetsEnable(bool state)
  {
    for (int i=Components.Count-1; i>=0; i--)
    {
      Component c = Components[i];
      System.Type type = c.GetType();
      
      // there's no common ancestor that lets us set .enabled for different types of components
      if (type.IsSubclassOf(typeof(Behaviour))) {
        (c as Behaviour).enabled = state;
      } else if (type.IsSubclassOf(typeof(Collider))) {
        (c as Collider).enabled = state;
      } else if (type.IsSubclassOf(typeof(Renderer))) {
        (c as Renderer).enabled = state;
      } else {
        Debug.LogWarning("Can't disable Component with type "+type+" on pause.", this);
        
      }
    }
  }
  
  void OnEnable()
  {
    SignalManager.ItemDragStarted += onDragStart;
    SignalManager.ItemDragStopped += onDragStop;
  }
  
  private void onDragStart(GLDragDropItem item)
  {
    setTargetsEnable(false);
  }
  
  private void onDragStop(GLDragDropItem item)
  {
    setTargetsEnable(true);
  }
  
  void OnDisable()
  {
    SignalManager.ItemDragStarted -= onDragStart;
    SignalManager.ItemDragStopped -= onDragStop;
  }
}
