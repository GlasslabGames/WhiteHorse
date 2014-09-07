using UnityEngine;
using System.Collections;


public class Player : MonoBehaviour
{
  private Color m_playerColor;
  private Color m_opponentColor;

  public Leaning m_leaning;
  public Leaning m_opponentLeaning;

  public bool m_campaignWorkerSelected;


  public Color PlayerColor
  {
    get { return m_playerColor; }
  }
  public Color OpponentColor
  {
    get { return m_opponentColor; }
  }


  public void Start()
  {
    if( m_leaning == Leaning.Red )
    {
      m_playerColor = Color.red;
      m_opponentColor = Color.blue;
      GameObjectAccessor.Instance.RedPlayerMarker.SetActive( true );
			GameObjectAccessor.Instance.ToggleWorkerButton.GetComponent< UITexture >().color = GameObjectAccessor.Instance.GameColorSettings.redStateDark;
      GameObjectAccessor.Instance.ToggleWorkerButton.GetComponent< UITexture >().alpha = 0.5f;
    }
    else
    {
      m_playerColor = Color.blue;
      m_opponentColor = Color.red;
      GameObjectAccessor.Instance.BluePlayerMarker.SetActive( true );
			GameObjectAccessor.Instance.ToggleWorkerButton.GetComponent< UITexture >().color = GameObjectAccessor.Instance.GameColorSettings.blueStateDark;
			GameObjectAccessor.Instance.ToggleWorkerButton.GetComponent< UITexture >().alpha = 0.5f;
    }
  }

  public void ToggleCampaignWorker( bool forceUntoggled = false )
  {
    m_campaignWorkerSelected = !m_campaignWorkerSelected;

    if( forceUntoggled )
    {
      m_campaignWorkerSelected = false;
    }

    if( m_leaning == Leaning.Red )
    {
      GameObjectAccessor.Instance.ToggleWorkerButton.GetComponent< UITexture >().alpha = m_campaignWorkerSelected ? 1 : 0.5f;
    }
    else
    {
      GameObjectAccessor.Instance.ToggleWorkerButton.GetComponent< UITexture >().alpha = m_campaignWorkerSelected ? 1 : 0.5f;
    }
  }
}