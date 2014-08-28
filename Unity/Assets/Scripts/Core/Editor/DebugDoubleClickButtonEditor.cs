//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DebugDoubleClickButton))]
public class DebugDoubleClickButtonEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI();

		DebugDoubleClickButton button = target as DebugDoubleClickButton;

		NGUIEditorTools.DrawEvents("On Click", button, button.onClick);
	}
}
