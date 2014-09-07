﻿using UnityEngine;
using System.Collections;

public class WeekMeter : MonoBehaviour {
	public UITexture m_fill;
	public UIGrid m_rulerGrid;
	public float m_rulerWidth;
	public float m_maxFillWidth;

	private int m_totalTurns;

	void Awake () {
		m_fill.color = (GameObjectAccessor.Instance.Player.m_leaning == Leaning.Blue) ?
			GameObjectAccessor.Instance.GameColorSettings.blueDarker : GameObjectAccessor.Instance.GameColorSettings.redDarker;

		m_totalTurns = GameObjectAccessor.Instance.GameStateManager.m_totalElectionWeeks;
		GameObject copy = m_rulerGrid.transform.GetChild (0).gameObject;
		while (m_rulerGrid.transform.childCount < m_totalTurns) {
			Utility.InstantiateAsChild(copy, m_rulerGrid.transform);
		}
		m_rulerGrid.cellWidth = m_rulerWidth / m_totalTurns;
		m_rulerGrid.Reposition ();

		Update (m_totalTurns);
	}

	public void Update(int weeksLeft) {
		float beginning = m_maxFillWidth - m_rulerWidth;
		m_fill.width = (int) ((m_rulerWidth / m_totalTurns) * (m_totalTurns - weeksLeft) + beginning); 
	}
}
