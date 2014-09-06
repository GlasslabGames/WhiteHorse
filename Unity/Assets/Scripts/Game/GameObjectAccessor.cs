using UnityEngine;
using System.Collections;

public class GameObjectAccessor : SingletonBehavior<GameObjectAccessor>
{
  public Player Player;
  public BudgetController Budget;
  public GameStateManager GameStateManager;
  public GameObject StatesContainer;
  public GameObject SupporterPrefab;
  public GameObject RedPlayerMarker;
  public GameObject BluePlayerMarker;
  public GameObject FloatingTextPrefab;
  public GameObject PulseTextPrefab;
  public Camera MainCamera;
  public Camera UICamera;
}