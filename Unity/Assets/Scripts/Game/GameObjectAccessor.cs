using UnityEngine;
using System.Collections;

public class GameObjectAccessor : SingletonBehavior<GameObjectAccessor>
{
  public Player Player;
  public BudgetController Budget;
  public GameStateManager GameStateManager;
	public GameColorSettings GameColorSettings;
  public GameObject StatesContainer;
	public GameObject FloatingTextContainer;
	public GameObject StateLabelContainer;
  public GameObject SupporterPrefab;
  public GameObject RedPlayerMarker;
  public GameObject BluePlayerMarker;
  public GameObject FloatingTextPrefab;
  public GameObject PulseTextPrefab;
  public Camera MainCamera;
  public Camera UICamera;
  public UILabel PlayerVotesLabel;
  public UILabel OpponentVotesLabel;
  public UILabel WeekCounter;
  public GameObject ToggleWorkerButton;
  public GameObject GameOverScreen;
  public UILabel GameOverRedVotes;
  public UILabel GameOverBlueVotes;
  public DetailView DetailView;
	public WeekMeter WeekMeter;
	public VoteMeter ElectoralVoteMeter;
	public OpinionMeter PopularVoteMeter;
  public GameObject TitleScreen;
  public GameObject FlipStateParticleSystemRed;
  public GameObject FlipStateParticleSystemBlue;
  public GameObject FlipStateParticleSystemNeutral;
  public bool UseAI;
}