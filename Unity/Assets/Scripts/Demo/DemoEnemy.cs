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
  private int m_convoIndex = 0;

  public void Refresh() {
    transform.localPosition = new Vector3( Model.Position[0], Model.Position[1], 0);
    transform.localScale    = new Vector3( Model.Scale[0], Model.Scale[1], 1);
  }

	public void Interact() {
    if (Model.Conversations != null && Model.Conversations.Count > m_convoIndex) {
      Utility.NextFrame( delegate() {
        GLDialogueManager.Instance.StartConversation(Model.Conversations[m_convoIndex]);
        m_convoIndex ++;
      }
        );
    } else {
      DemoManager.Instance.Dialog.Show(transform.position, Model.Name, DemoBarkModel.GetModel(Model.Bark).Text);
    }
	}
}
