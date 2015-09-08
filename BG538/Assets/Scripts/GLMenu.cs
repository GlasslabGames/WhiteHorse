#if UNITY_EDITOR 

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GLMenu : MonoBehaviour {

	[MenuItem("GLMenu/SelectMissingComponents")]
	static void SelectMissing(MenuCommand command)
	{
		List<GameObject> selection = new List<GameObject> ();

		foreach (Object o in Selection.objects) {
			if (o is GameObject) {
				GameObject go = o as GameObject;
				Transform[] ts = go.GetComponentsInChildren<Transform> ();
				foreach (Transform t in ts) {
					Component[] cs = t.gameObject.GetComponents<Component> ();
					foreach (Component c in cs) {
						if (c == null) {
							selection.Add (t.gameObject);
						}
					}
				}
			}
		}
		Selection.objects = selection.ToArray();
	}

	[MenuItem("GLMenu/AddStateComponent")]
	static void AddStateComponent(MenuCommand command)
	{
		foreach (Object o in Selection.objects) {
			if (o is GameObject) {
				GameObject go = o as GameObject;
				State s = go.AddComponent<State>() as State;
				s.m_abbreviation = go.name;
			}
		}
	}

	[MenuItem("GLMenu/Anchors to Corners %[")]
	static void AnchorsToCorners(){
		RectTransform t = Selection.activeTransform as RectTransform;
		RectTransform pt = Selection.activeTransform.parent as RectTransform;
		
		if(t == null || pt == null) return;
		
		Vector2 newAnchorsMin = new Vector2(t.anchorMin.x + t.offsetMin.x / pt.rect.width,
		                                    t.anchorMin.y + t.offsetMin.y / pt.rect.height);
		Vector2 newAnchorsMax = new Vector2(t.anchorMax.x + t.offsetMax.x / pt.rect.width,
		                                    t.anchorMax.y + t.offsetMax.y / pt.rect.height);
		
		t.anchorMin = newAnchorsMin;
		t.anchorMax = newAnchorsMax;
		t.offsetMin = t.offsetMax = new Vector2(0, 0);
	}
	
	[MenuItem("GLMenu/Corners to Anchors %]")]
	static void CornersToAnchors(){
		RectTransform t = Selection.activeTransform as RectTransform;
		
		if(t == null) return;
		
		t.offsetMin = t.offsetMax = new Vector2(0, 0);
	}
}

#endif
