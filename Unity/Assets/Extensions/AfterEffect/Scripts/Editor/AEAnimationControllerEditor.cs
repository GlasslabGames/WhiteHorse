using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;


[CustomEditor(typeof(AEAnimationController))]
public class AEAnimationControllerEditor : Editor {
	
	//protected SerializedProperty _PlayOnStart;
	
	
	protected virtual void OnEnable () {
		//_PlayOnStart = serializedObject.FindProperty ("PlayOnStart");
	}
	
	public override void OnInspectorGUI() {
		
		if(!Application.isPlaying) {
			SearchLayers();
			controller.CleanUpLayers();
		}
		
		controller.PlayOnStart = EditorGUILayout.Toggle("Play On Start", controller.PlayOnStart);

		
		foreach(AEClipTemplate tpl in controller.clips) {
			
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			
			EditorGUILayout.LabelField(tpl.name, new GUILayoutOption[]{GUILayout.Width(95)});
			
			
			if(tpl.IsEditMode) {
				EditorGUI.BeginChangeCheck();
				string name =  EditorGUILayout.TextField(tpl.name, new GUILayoutOption[]{GUILayout.Width(95)});
				if(EditorGUI.EndChangeCheck()) {
					controller.SetClipName(name, tpl.anim);
				}
			}
			
		
			EditorGUILayout.ObjectField(tpl.anim.gameObject, typeof(GameObject), true);
			string buttontext = "Edit";
			if(tpl.IsEditMode) {
				buttontext = "Done";
			}
			
			if(GUILayout.Button(buttontext)) {
				tpl.IsEditMode = !tpl.IsEditMode;
			}
			
			
			EditorGUILayout.EndHorizontal();

			
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("", new GUILayoutOption[]{GUILayout.Width(95)});
			tpl.wrapMode = (AEWrapMode) EditorGUILayout.EnumPopup("Wrap Mode:", tpl.wrapMode);
			if(tpl.wrapMode == AEWrapMode.Loop) {
				tpl.anim.Loop = true;
			} else {
				tpl.anim.Loop = false;
			}
			
			EditorGUILayout.EndHorizontal();
			
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("", new GUILayoutOption[]{GUILayout.Width(95)});
				EditorGUI.BeginChangeCheck();
			   		EditorGUILayout.Toggle("Default Clip", tpl.defaultClip);
				if(EditorGUI.EndChangeCheck()) {
						controller.SetDefaultClip(tpl);
				}
			EditorGUILayout.EndHorizontal();
			
		}
	}
	
	private void SearchLayers() {
		foreach(Transform tr in controller.transform) {
			AfterEffectAnimation anim = tr.GetComponent<AfterEffectAnimation> ();
			if(anim != null) {
				controller.RegisterClip(anim);
			}
		}

    controller.clips.TrimExcess(); // HACK memory optimization
	}
	
	//--------------------------------------
	// GET / SET
	//--------------------------------------
	
	public AEAnimationController controller {
		get {
			return target as AEAnimationController;
		}
	}
}
