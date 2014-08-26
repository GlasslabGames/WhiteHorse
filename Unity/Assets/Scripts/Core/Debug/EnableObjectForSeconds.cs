using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnableObjectForSeconds : MonoBehaviour {

  public List<GameObject> EnableObjects;
  public UILabel MsgLabel;
  public float Duration = 2f;
  
  void OnEnable() {
    ObjectsSetActive(false);
  }
  
  void OnDisable() {
    StopAllCoroutines();
    ObjectsSetActive(false);
  }

  public void ShowObjects(string showMsg = "") {
    if (gameObject.activeInHierarchy)
    {
      StopAllCoroutines();
      ObjectsSetActive(true);
      if (MsgLabel != null)
        MsgLabel.text = showMsg;
      StartCoroutine(showingObjects());
    }
  }

  IEnumerator showingObjects() {
    yield return new WaitForSeconds(Duration);
    ObjectsSetActive(false);
  }

  void ObjectsSetActive(bool isActive) {
    if (EnableObjects != null)
      foreach (var o in EnableObjects)
        o.SetActive(isActive);
  }
}
