using UnityEngine;
using System.Collections;

public class GameObjectAccessor : SingletonBehavior<GameObjectAccessor> {
	public Player Player;
	public BudgetController Budget;
	public GameStateManager GameStateManager;
	public GameColorSettings GameColorSettings;
	public GameObject StatesContainer;
	public GameObject FloatingTextContainer;
	public ShowStateLabels StateLabelShower;
	public GameObject SupporterPrefab;
	public GameObject FloatingTextPrefab;
	public GameObject PulseTextPrefab;
	public Camera MainCamera;
	public Camera UICamera;
	public UILabel PlayerVotesLabel;
	public UILabel OpponentVotesLabel;
	public UILabel WeekCounter;
	public GameObject GameOverScreen;
	public UILabel GameOverRedVotes;
	public UILabel GameOverBlueVotes;
	public DetailView DetailView;
	public GameObject TitleScreen;
	public GameObject FlipStateParticleSystemRed;
	public GameObject FlipStateParticleSystemBlue;
	public GameObject FlipStateParticleSystemNeutral;
	public VoteCount PlayerVoteCount;
	public VoteCount OpponentVoteCount;
	public bool UseAI;
	public float VoteUpdateTime;
	public GameObject VictorySound;
	public GameObject DefeatSound;
	public UITexture EndTurnButton;
	public UILabel WaitingText;
	public OpponentAi OpponentAi;
}