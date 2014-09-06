using UnityEngine;
using System.Collections;

public class GameObjectAccessor : SingletonBehavior<GameObjectAccessor>
{
  public Player Player;
  public BudgetController Budget;
  public GameObject SupporterPrefab;
  public GameObject RedPlayerMarker;
  public GameObject BluePlayerMarker;
}