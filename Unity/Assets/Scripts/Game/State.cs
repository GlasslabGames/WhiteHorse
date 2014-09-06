using UnityEngine;
using System;


public enum Leaning
{
  Neutral,
  Red,
  Blue
}

public class State : MonoBehaviour
{
  private Color blueStateColor = new Color( 79.0f / 255.0f, 131.0f / 255.0f, 255.0f / 255.0f );
  private Color redStateColor = new Color( 255.0f / 255.0f, 79.0f / 255.0f, 79.0f / 255.0f );
  private Color undiscoveredStateColor = new Color( 79.0f / 255.0f, 79.0f / 255.0f, 79.0f / 255.0f );
  private Color neutralStateColor = new Color( 156.0f / 255.0f, 103.0f / 255.0f, 152.0f / 255.0f );


  public Leaning m_stateLeaning;

  public float m_electoralCount;
  public float m_populationInMillions;

  public string m_name;
  public string m_abbreviation;

  public bool m_hidden;
  public bool m_inPlay;

  private float m_playerSupporterCount;
  private float m_opponentSupporterCount;


  public void Start()
  {
    m_playerSupporterCount = 0;
    m_opponentSupporterCount = 0;

    UpdateColor();
  }

  public void UpdateColor()
  {
    if( m_inPlay && m_hidden )
    {
      gameObject.GetComponentInChildren<SpriteRenderer>().color = undiscoveredStateColor;
      return;
    }

    switch( m_stateLeaning )
    {
    case Leaning.Blue:
      gameObject.GetComponentInChildren<SpriteRenderer>().color = blueStateColor;
      break;

    case Leaning.Red:
      gameObject.GetComponentInChildren<SpriteRenderer>().color = redStateColor;
      break;

    default:
      gameObject.GetComponentInChildren<SpriteRenderer>().color = neutralStateColor;
      break;
    }
  }

  [RPC]
  public void PlayerPlaceSupporter()
  {
    if( !m_inPlay )  return;

    if( m_playerSupporterCount < m_populationInMillions )
    {
      m_playerSupporterCount++;
      CreateSupporterPrefab( true );

      GameObjectAccessor.Instance.Budget.ConsumeAmount( 30.0f );

      networkView.RPC( "OpponentPlaceSupporter", RPCMode.Others );
    }
  }

  [RPC]
  public void OpponentPlaceSupporter()
  {
    if( !m_inPlay )  return;

    if( m_opponentSupporterCount < m_populationInMillions )
    {
      m_opponentSupporterCount++;
      CreateSupporterPrefab( false );
    }
  }

  public void CreateSupporterPrefab( bool isPlayer )
  {
    Vector3 supporterPosition = gameObject.transform.position + ( Vector3.right * ( isPlayer ? ( m_playerSupporterCount * 0.5f ) : ( m_opponentSupporterCount * 0.5f ) ) );
    supporterPosition.y += isPlayer ? 0 : 1;
    GameObject newSupporter = GameObject.Instantiate( GameObjectAccessor.Instance.SupporterPrefab, supporterPosition, Quaternion.identity ) as GameObject;
    newSupporter.GetComponent<SpriteRenderer>().color = isPlayer ? GameObjectAccessor.Instance.Player.PlayerColor : GameObjectAccessor.Instance.Player.OpponentColor;
  }
}