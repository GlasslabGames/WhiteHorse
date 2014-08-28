//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GLMouseEventHandler))]
public class GLMouseEventHandlerEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		GLMouseEventHandler button = target as GLMouseEventHandler;

		button.debug = EditorGUILayout.Toggle("Debug", button.debug);
		
		GUILayout.Space(3f);

		NGUIEditorTools.DrawEvents("On Mouse Down", button, button.onMouseDown);

		GUILayout.Space(3f);
		
		NGUIEditorTools.DrawEvents("On Mouse Up", button, button.onMouseUp);
	}
}
