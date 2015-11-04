﻿using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;

public class MatchmakerEntry : PrefabEntry {
	public Text PlayerLabel;
	public Text ScenarioLabel;
	public Image ColorIndicator;

	[HideInInspector]
	public string RoomName;

	public void Set(RoomInfo room) {
		RoomName = room.name;

		Hashtable props = room.customProperties;
		if (props == null || !props.ContainsKey("s")) return;
		ScenarioModel scenario = ScenarioModel.GetModel((int) props["s"]);
		Set((string) props["n"], scenario.Name, (Leaning) props["c"]);
	}

	public void Set(string playerName, string scenarioName, Leaning color) {
		base.CheckToggleGroup();

		PlayerLabel.text = playerName;
		ScenarioLabel.text = scenarioName;
		ColorIndicator.color = ColorSwapper.GetColor(color == Leaning.Blue, ColorSwapper.ColorChoice.med);
	}

	public void OnToggle(bool value) {
		if (value) SoundController.Play("ListSelect");
	}
}
