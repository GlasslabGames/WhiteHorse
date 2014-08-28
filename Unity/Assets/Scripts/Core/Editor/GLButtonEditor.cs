//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GLButton))]
public class GLButtonEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		GLButton button = target as GLButton;

		button.debug = EditorGUILayout.Toggle("Debug", button.debug);
    button.UseDragProof = EditorGUILayout.Toggle("UseDragProof", button.UseDragProof);
		
		GUILayout.Space(3f);

		NGUIEditorTools.DrawEvents("On Click", button, button.onClick);
	}
}
