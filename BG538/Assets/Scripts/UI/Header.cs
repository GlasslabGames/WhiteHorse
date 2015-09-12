﻿using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using System.Text.RegularExpressions;

public class Header : MonoBehaviour {
	public Text playerVotesLabel;
	public Text opponentVotesLabel;
	public Text playerMoneyLabel;

	public Image insetBackground;
	public Text resultLabel;

	public Image divider;
	public Image opponentBackground;

	private BudgetController playerBudgetController;

	void Awake() {
		// Listen for player or opponent votes to change
		SignalManager.PlayerVotesChanged += OnPlayerVotesChanged;
		SignalManager.OpponentVotesChanged += OnOpponentVotesChanged;
		SignalManager.BudgetChanged += OnBudgetChanged;
	}

	void Start() {}

	void OnBudgetChanged(BudgetController budget, float amount) {
		if (budget == GameManager.Instance.PlayerBudget) { // we only care about changes to the player's budget
			int n = 0;
			string t = Regex.Replace(playerMoneyLabel.text, "\\D", "");
			if (System.Int32.TryParse(t, out n)) {
				DOTween.To (x => playerMoneyLabel.text = "$"+Mathf.Round(x).ToString(), n, amount, GameSettings.Instance.VoteUpdateTime / 2);
			}
		}
	}

	void OnPlayerVotesChanged(int votes, bool isUpdate) {
		SetVoteLabel(playerVotesLabel, votes, isUpdate);
	}

	void OnOpponentVotesChanged(int votes, bool isUpdate) {
		SetVoteLabel(opponentVotesLabel, votes, isUpdate);
	}

	void SetVoteLabel(Text label, int votes, bool isUpdate) {
		if (!isUpdate) {
			label.text = votes.ToString();
		} else {
			int n = 0;
			if (System.Int32.TryParse(label.text, out n)) {
				DOTween.To (x => label.text = Mathf.Round(x).ToString(), n, votes, GameSettings.Instance.VoteUpdateTime);
			}
		}
	}

	void TweenLabel(Text label, int votes, bool isUpdate) {
	}

	public void ShowGameOver(bool win, Action callback = null) {
		divider.gameObject.SetActive (false);
		opponentBackground.DOFillAmount((win) ? 0 : 1, GameSettings.Instance.VoteUpdateTime).OnComplete (() => CompleteGameOver(win, callback));
	}

	void CompleteGameOver(bool win, Action callback = null) {
		if (resultLabel) {
			resultLabel.gameObject.SetActive (true);
			resultLabel.text = win ? "YOU WIN!" : "YOU LOSE!";
			resultLabel.transform.DOPunchScale(new Vector3(1.25f, 1.25f, 1f), 1f, 2);
		}
		
		Color c = (win ^ GameManager.Instance.PlayerIsBlue)? GameSettings.Instance.Colors.lightRed : GameSettings.Instance.Colors.lightBlue;
		if (ObjectAccessor.Instance.Background) ObjectAccessor.Instance.Background.color = c;

		if (callback != null) callback();
	}

	public void Reset() {
		opponentBackground.fillAmount = 0.195f;
		divider.gameObject.SetActive(true);
	}
}
