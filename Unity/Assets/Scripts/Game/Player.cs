using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	public Leaning m_leaning;
	public Leaning leaning {
		get { return m_leaning; }
		set { m_leaning = value; }
	}

	public bool IsRed { get { return m_leaning == Leaning.Red; } }
	public bool IsBlue { get { return m_leaning == Leaning.Blue; } }

	public void Start() {}
}