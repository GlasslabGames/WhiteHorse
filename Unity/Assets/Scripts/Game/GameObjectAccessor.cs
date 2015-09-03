using UnityEngine;
using System.Collections;

public class GameObjectAccessor : SingletonBehavior<GameObjectAccessor> {
	public Player Player;
	public BudgetController Budget;
	public GameStateManager GameStateManager;
	public GameColorSettings GameColorSettings;
	public SpriteRenderer Background;
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
	public DetailView DetailView;
	public GameObject FlipStateParticleSystemRed;
	public GameObject FlipStateParticleSystemBlue;
	public GameObject FlipStateParticleSystemNeutral;
	public VoteCount PlayerVoteCount;
	public VoteCount OpponentVoteCount;
	public float VoteUpdateTime; // Note, this is only used for the deprecated OpinionMeter, VoteMeter, and WeekMeter
	public GameObject VictorySound;
	public GameObject DefeatSound;
	public GLButton EndTurnButton;
	public GLButton RestartButton;
	public GameObject WaitingIndicator;
	public UITexture HeaderInset;
	public UILabel WaitingText;
	public UILabel ResultText;
	public HeaderBackground HeaderBg;
	public OpponentAi OpponentAi;
}