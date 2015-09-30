using System;
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

	void OnDestroy() {
		SignalManager.PlayerVotesChanged -= OnPlayerVotesChanged;
		SignalManager.OpponentVotesChanged -= OnOpponentVotesChanged;
		SignalManager.BudgetChanged -= OnBudgetChanged;
	}

	void OnBudgetChanged(BudgetController budget, float amount) {
		if (budget == GameManager.Instance.PlayerBudget) { // we only care about changes to the player's budget
			int n = 0;
			string t = Regex.Replace(playerMoneyLabel.text, "\\D", "");
			if (System.Int32.TryParse(t, out n)) {
				DOTween.To (x => playerMoneyLabel.text = "$"+Mathf.Round(x).ToString(), n, amount, GameSettings.InstanceOrCreate.VoteUpdateTime / 2);
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
				DOTween.To (x => label.text = Mathf.Round(x).ToString(), n, votes, GameSettings.InstanceOrCreate.VoteUpdateTime);
			} else {
				Debug.LogError("Existing vote text "+label.text + " isn't a number!");
			}
		}
	}

	public void ShowGameOver(bool win, Action callback = null) {
		divider.gameObject.SetActive (false);
		opponentBackground.DOFillAmount((win) ? 0 : 1, GameSettings.InstanceOrCreate.VoteUpdateTime).OnComplete (() => CompleteGameOver(win, callback));
	}

	void CompleteGameOver(bool win, Action callback = null) {
		if (resultLabel) {
			resultLabel.gameObject.SetActive (true);
			resultLabel.text = win ? "YOU WIN!" : "YOU LOSE!";
			resultLabel.transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 1f, 3);
		}

		if (insetBackground) insetBackground.gameObject.SetActive (true);

		if (callback != null) callback();
	}

	public void Reset() {
		opponentBackground.fillAmount = 0.195f;
		divider.gameObject.SetActive(true);
		if (insetBackground) insetBackground.gameObject.SetActive (false);
		if (resultLabel) resultLabel.gameObject.SetActive(false);

		if (playerVotesLabel) playerVotesLabel.text = "000";
		if (opponentVotesLabel) opponentVotesLabel.text = "000";
		if (playerMoneyLabel) playerMoneyLabel.text = "$00";
	}
}
