using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.NGUI {

	/// <summary>
	/// This is an implementation of the abstract QuestLogWindow class for NGUI.
	/// </summary>
	public class NGUIQuestLogWindow : QuestLogWindow {

		/// <summary>
		/// The UI Root containing the quest log window.
		/// </summary>
		public UIRoot uiRoot;

		/// <summary>
		/// The main quest log window panel.
		/// </summary>
		public GameObject mainPanel;

		/// <summary>
		/// The active quests button.
		/// </summary>
		public UIButton activeQuestsButton;

		/// <summary>
		/// The completed quests button.
		/// </summary>
		public UIButton completedQuestsButton;

		/// <summary>
		/// The quest table.
		/// </summary>
		public UITable questTable;

		/// <summary>
		/// The quest template.
		/// </summary>
		public NGUIQuestTemplate questTemplate;

		/// <summary>
		/// The confirmation popup to use if the player clicks the abandon quest button.
		/// It should send ClickConfirmAbandonQuest if the player confirms, or
		/// ClickCancelAbandonQuest if the player cancels.
		/// </summary>
		public GameObject abandonPopup;

		/// <summary>
		/// The quest title label to set in the abandon quest dialog popup.
		/// </summary>
		public UILabel abandonQuestTitle;

		/// <summary>
		/// This handler is called if the player confirms abandonment of a quest.
		/// </summary>
		private Action confirmAbandonQuestHandler = null;

		/// <summary>
		/// Hide the main panel and all of the templates.
		/// </summary>
		void Start() {
			NGUITools.SetActive(mainPanel, false);
			NGUITools.SetActive(abandonPopup, false);
			if (questTemplate != null) NGUITools.SetActive(questTemplate.gameObject, false);
			if (DialogueDebug.LogWarnings) {
				if (mainPanel == null) Debug.LogWarning(string.Format("{0}: {1} Main Panel is unassigned", DialogueDebug.Prefix, name));
				if (questTable == null) Debug.LogWarning(string.Format("{0}: {1} Quest Table is unassigned", DialogueDebug.Prefix, name));
				if (questTemplate == null) Debug.LogWarning(string.Format("{0}: {1} Quest Template is unassigned", DialogueDebug.Prefix, name));
			}
		}

		/// <summary>
		/// Open the window by showing the main panel. The bark UI may conflict with the quest
		/// log window, so temporarily disable it.
		/// </summary>
		/// <param name="openedWindowHandler">Opened window handler.</param>
		public override void OpenWindow(Action openedWindowHandler) {
			NGUITools.SetActive(mainPanel, true);
			openedWindowHandler();
		}

		/// <summary>
		/// Close the window by hiding the main panel. Re-enable the bark UI.
		/// </summary>
		/// <param name="closedWindowHandler">Closed window handler.</param>
		public override void CloseWindow(Action closedWindowHandler) {
			NGUITools.SetActive(mainPanel, false);
			closedWindowHandler();
		}

		/// <summary>
		/// Whenever the quest list is updated, repopulate the scroll panel.
		/// </summary>
		public override void OnQuestListUpdated() {
			ClearQuestTable();
			if (Quests.Length == 0) {
				AddQuestToTable(new QuestInfo(string.Empty, new FormattedText(NoQuestsMessage), FormattedText.empty, new FormattedText[0], new QuestState[0], false, false, false));
			} else {
				AddQuestsToTable();
			}
			if (activeQuestsButton != null) activeQuestsButton.isEnabled = !IsShowingActiveQuests;
			if (completedQuestsButton != null) completedQuestsButton.isEnabled = IsShowingActiveQuests;
		}

		private void ClearQuestTable() {
			if (questTable == null) return;
			List<Transform> delete = new List<Transform>();
			foreach (Transform child in questTable.transform) {
				if (child.gameObject.activeSelf) delete.Add(child);
			}
			delete.ForEach(c => NGUITools.Destroy(c.gameObject));
		}

		private void AddQuestsToTable() {
			if (questTable == null) return;
			foreach (var questInfo in Quests) {
				AddQuestToTable(questInfo);
			}
			questTable.Reposition();
		}

		/// <summary>
		/// Adds a quest to the table using the template.
		/// </summary>
		/// <param name="questInfo">Quest info.</param>
		private void AddQuestToTable(QuestInfo questInfo) {
			if ((questTable == null) || (questTemplate == null)) return;
			GameObject child = NGUITools.AddChild(questTable.gameObject, questTemplate.gameObject);
			NGUIQuestTemplate item = child.GetComponent<NGUIQuestTemplate>();
			if ((item == null) || !item.ArePropertiesAssigned) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: {1} Not all properties are assigned in the template", DialogueDebug.Prefix, name));
				return;
			}
			NGUITools.SetActive(child, true);
			item.heading.text = questInfo.Heading.text;
			string fullDescription = string.Empty;
			if (questHeadingSource == QuestHeadingSource.Name) fullDescription += questInfo.Description.text;
			for (int i = 0; i < questInfo.Entries.Length; i++) {
				if (questInfo.EntryStates[i] != QuestState.Unassigned) {
					fullDescription += "\n" + questInfo.Entries[i].text;
				}
			}
			item.description.text = fullDescription;
			if (item.description.transform.parent != child) {
				TweenScale tweenScale = item.description.transform.parent.GetComponent<TweenScale>();
				if (tweenScale != null) NGUITools.SetActive(tweenScale.gameObject, false);
			}
			NGUIQuestTitle nguiQuestTitle = item.trackButton.gameObject.AddComponent<NGUIQuestTitle>();
			nguiQuestTitle.questTitle = questInfo.Title;
			UIEventListener.Get(item.trackButton.gameObject).onClick += OnTrackButtonClicked;
			NGUITools.SetActive(item.trackButton.gameObject, questInfo.Trackable);
			nguiQuestTitle = item.abandonButton.gameObject.AddComponent<NGUIQuestTitle>();
			nguiQuestTitle.questTitle = questInfo.Title;
			UIEventListener.Get(item.abandonButton.gameObject).onClick += OnAbandonButtonClicked;
			NGUITools.SetActive(item.abandonButton.gameObject, questInfo.Abandonable);
		}

		/// <summary>
		/// Track button clicked event that sets SelectedQuest first.
		/// </summary>
		/// <param name="button">Button.</param>
		private void OnTrackButtonClicked(GameObject button) {
			SelectedQuest = button.GetComponent<NGUIQuestTitle>().questTitle;
			ClickTrackQuest(SelectedQuest);
		}

		/// <summary>
		/// Abandon button clicked event that sets SelectedQuest first.
		/// </summary>
		/// <param name="button">Button.</param>
		private void OnAbandonButtonClicked(GameObject button) {
			SelectedQuest = button.GetComponent<NGUIQuestTitle>().questTitle;
			ClickAbandonQuest(SelectedQuest);
		}

		/// <summary>
		/// Opens the abandon confirmation popup.
		/// </summary>
		/// <param name="title">Quest title.</param>
		/// <param name="confirmAbandonQuestHandler">Confirm abandon quest handler.</param>
		public override void ConfirmAbandonQuest(string title, Action confirmAbandonQuestHandler) {
			this.confirmAbandonQuestHandler = confirmAbandonQuestHandler;
			OpenAbandonPopup(title);
		}

		/// <summary>
		/// Opens the abandon popup modally if assigned; otherwise immediately confirms.
		/// </summary>
		/// <param name="title">Quest title.</param>
		private void OpenAbandonPopup(string title) {
			if (abandonPopup != null) {
				NGUITools.SetActive(abandonPopup, true);
				if (abandonQuestTitle != null) abandonQuestTitle.text = title;
			} else {
				this.confirmAbandonQuestHandler();
			}
		}

		/// <summary>
		/// Closes the abandon popup.
		/// </summary>
		private void CloseAbandonPopup() {
			NGUITools.SetActive(abandonPopup, false);
		}

		public void ClickConfirmAbandonQuestButton() {
			CloseAbandonPopup();
			confirmAbandonQuestHandler();
		}

		public void ClickCancelAbandonQuestButton() {
			CloseAbandonPopup();
		}

	}

}
