using UnityEngine;
using System.Collections;

public class DemoEnemy : MonoBehaviour {
	private DemoEnemyModel m_model;
  public DemoEnemyModel Model {
    get { return m_model; }
    set {
      m_model = value;
      Utility.NextFrame( Refresh );
    }
  }

  public void Refresh() {
    transform.localPosition = new Vector3( Model.Position[0], Model.Position[1], 0);
    transform.localScale    = new Vector3( Model.Scale[0], Model.Scale[1], 1);
  }

	public void Interact() {
    DemoManager.Instance.Dialog.Show(transform.position, Model.Name, DemoBarkModel.GetModel(Model.Bark).Text);
	}
}
