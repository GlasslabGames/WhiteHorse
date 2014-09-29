using UnityEngine;
using System.Collections;

public class DemoManager : SingletonBehavior<DemoManager> {
  public TextPopup Dialog;

  override protected void Start() {
    GameObject prefab = Resources.Load("DemoEnemy") as GameObject;
    foreach (DemoEnemyModel enemy in DemoEnemyModel.Models) {
      GameObject go = GameObject.Instantiate(prefab) as GameObject;
      go.name = enemy.Name;
      DemoEnemy de = go.GetComponent<DemoEnemy>();
      if (de != null) de.Model = enemy;
    }
  }
	
}
