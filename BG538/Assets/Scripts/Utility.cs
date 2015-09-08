using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Useful static functions. It's a Monobehavior so we can use coroutine.
/// </summary>
using System.Collections;


public class Utility : SingletonBehavior<Utility>
{
	public static Vector3 _VECTOR3 = new Vector3 ();

	public static bool IsPrefab (GameObject go)
	{
		#if UNITY_EDITOR
    if (go.hideFlags != HideFlags.None || AssetDatabase.Contains(go))
      return true;
		#endif

		return false;
	}

	// Finds all instances of T in the scene, including disabled objects
	public static List<T> FindInstancesInScene<T> () where T : MonoBehaviour
	{
		List<T> instanceList = new List<T> ();
		T[] instances = Resources.FindObjectsOfTypeAll<T> ();
		for (int i=0; i < instances.Length; i++) {
			T instance = instances [i];
			if (instance.hideFlags == HideFlags.NotEditable || instance.hideFlags == HideFlags.HideAndDontSave)
				continue;

			if (IsPrefab (instance.gameObject))
				continue;

			instanceList.Add (instance);
		}

		return instanceList;
	}

	public static string GetMajorVersionFromVersion (string input)
	{
		string version = input;
		string[] versionComponents = version.Split (new char[]{'.'});
		if (versionComponents.Length >= 2) {
			version = versionComponents [0] + "." + versionComponents [1];
		}

		return version;
	}

	public static string GetHierarchyString (GameObject target)
	{
		Transform obj = target.transform;
		string hierarchy = obj.name;
		obj = obj.transform.parent;
		while (obj != null) {
			hierarchy = obj.name + "/" + hierarchy;
			obj = obj.transform.parent;
		}

		return hierarchy;
	}

	public static int GetNumChildren (GameObject target, bool immediateOnly = false)
	{
		if (immediateOnly) {
			return GetImmediateChildren (target).Count ();
		} else {
			return target.GetComponentsInChildren<Transform> ().Length - 1;
		}
	}

	public static IEnumerable<Transform> GetImmediateChildren (GameObject target)
	{
		return target.GetComponentsInChildren<Transform> ().Where (x => x.parent == target.transform);
	}

	public static T[] GetInterfacesInScene<T> ()
	{
		if (!typeof(T).IsInterface)
			throw new SystemException ("Specified type is not an interface!");
    
		MonoBehaviour[] mObjs = Resources.FindObjectsOfTypeAll<MonoBehaviour> ();
    
		return (from a in mObjs where a.GetType ().GetInterfaces ().Any (k => k == typeof(T)) select (T)(object)a).ToArray ();
	}

	public static float crossProduct (Vector2 a, Vector2 b)
	{
		return a.x * b.y - a.y * b.x;
	}

	// apply a formula to calculate the radius of an ellipse at an angle off the x-axis (a), also giving the y-alligned radius (b)
	public static float EllipseRadius (float a, float b, float angle)
	{
		float sine = Mathf.Sin (angle);
		float cosine = Mathf.Cos (angle);
		return (a * b) / Mathf.Sqrt (a * a * sine * sine + b * b * cosine * cosine);
	}

	/*
	// Moves an object on the UI layer to match the position of a game world object
	public static Vector3 ConvertFromGameToUiPosition (Vector3 pos, Camera worldCamera = null, Camera uiCamera = null)
	{
		if (worldCamera == null)
			worldCamera = Camera.main;
		if (uiCamera == null)
			uiCamera = UICamera.mainCamera;
		//Debug.Log ("Convert game position from worldCamera "+worldCamera, worldCamera);
		//Debug.Log ("Convert game position to uiCamera "+uiCamera, uiCamera);
		pos = worldCamera.WorldToViewportPoint (pos);
		pos = uiCamera.ViewportToWorldPoint (pos);
		pos.z = 0;
		return pos;
	}

	// Moves and scales object with a box collider to match a collider in the game world
	public static bool SetUiColliderOverGameObject (BoxCollider uiObjCollider, Collider gameObjCollider)
	{
		if (!gameObjCollider.enabled)
			return false; // Ignore on colliders that are disabled
		Bounds bounds = gameObjCollider.bounds;
		Vector3 center = Camera.main.WorldToViewportPoint (bounds.center);
		Vector3 min = Camera.main.WorldToViewportPoint (bounds.min);
		Vector3 max = Camera.main.WorldToViewportPoint (bounds.max);
		
		// Check if it's onscreen & large enough to use. If so, align the container
		//if (max.x > 0 && min.x < 1 && max.y > 0 && min.y < 1) {
			
		// Position over the obj
		center = UICamera.mainCamera.ViewportToWorldPoint (center);
		center.z = 0;
		uiObjCollider.transform.position = center;
			
		// Size the container to match
		min = UICamera.mainCamera.ViewportToWorldPoint (min);
		max = UICamera.mainCamera.ViewportToWorldPoint (max);

		// Adjust for UI Root scale
		Vector3 scale = uiObjCollider.transform.parent.lossyScale;

		_VECTOR3.Set ((max.x - min.x) / scale.x,
                   (max.y - min.y) / scale.y,
                   0);
		uiObjCollider.size = _VECTOR3;

		return true; // successful positioning
	}
	*/

	public static void Delay (System.Action action, float secs)
	{
		InstanceOrCreate.StartCoroutine (DelayCoroutine (action, secs));
	}

	private static IEnumerator DelayCoroutine (System.Action action, float secs)
	{
		yield return new WaitForSeconds (secs);
		action ();
	}
  
	public static void NextFrame (System.Action action)
	{
		InstanceOrCreate.StartCoroutine (NextFrameCoroutine (action));
	}
  
	private static IEnumerator NextFrameCoroutine (System.Action action)
	{
		yield return null;
		action ();
	}

	// DFS for a transform in the heirarchy under the given transform (Transfrom.Find only looks at direct children)
	public static Transform FindInChildren (Transform t, string name)
	{
		foreach (Transform child in t) {
			if (child.name == name)
				return child;
			else {
				Transform result = FindInChildren (child, name);
				if (result != null)
					return result;
			}
		}
		return null;
	}

	public static T FirstAncestorOfType<T> (Transform transform) where T : Component
	{
		var t = transform.parent;
		T component = null;
		while (t != null && (component = t.GetComponent<T>()) == null) {
			t = t.parent;
		}
		return component;
	}

	// like FirstAncestor, but it can be bounded to only look up to a certain level (1 is parent, 2 is grandparent, etc)
	public static T GetComponentInAncestor<T> (Transform transform, int level = 0) where T : Component
	{
		var t = transform;
		T component = null;
		while (t != null && (component = t.GetComponent<T>()) == null && level-- > 0) {
			t = t.parent;
		}
		return component;
	}

	public static string CapitalizeFirstOnly (string s)
	{
		string first = s.Substring (0, 1);
		string rest = s.Substring (1);
		return first.ToUpper () + rest.ToLower ();
	}

	// takes a multiword string and returns it with no spaces and all lowercase, except an uppercase letter at the word boundaries
	public static string CamelCase (string s)
	{
		string[] parts = s.Split (' ');
		string result = parts [0].ToLower ();
		for (int i = 1; i < parts.Length; i++) {
			result += CapitalizeFirstOnly (parts [i]);
		}
		return result;
	}

	// split a string around commas, trimming whitespace
	public static List<string> SplitString (string input)
	{
		if (input == null || input.Length <= 0)
			return new List<string> (); // return an empty list
		List<string> list = new List<string> (input.Split (','));
		list = list.Where (s => s.Length > 0).ToList (); // remove empty strings that were added to the list
		for (int i = 0; i < list.Count; i++) {
			list [i] = list [i].Trim ();
		}
		return list;
	}

	// Instantiate a prefab and add it to the specified parent, then reset its position and scale to their original values
	public static Transform InstantiateAsChild (UnityEngine.Object prefab, Transform parent)
	{
		if (prefab == null)
			return null;
		GameObject gameObj = (GameObject)Instantiate (prefab);
		if (gameObj == null)
			return null;
		Transform child = gameObj.transform;
		Vector3 pos = child.localPosition;
		Vector3 scale = child.localScale;

		child.parent = parent;
		child.localPosition = pos;
		child.localScale = scale;

		return child;
	}

	// Convenience function to add a new empty gameObject and attach it to the parent with blank position/scale
	public static Transform NewChild (Transform parent, string name = "")
	{
		Transform t = (new GameObject ()).transform;

		t.name = (name.Length > 0) ? name : "NewChild";
		t.parent = parent;
		t.localScale = Vector3.one;
		t.localPosition = Vector3.zero;

		return t;
	}

	public static Color HashToColor (string s, float minHue, float maxHue, float minSat, float maxSat,
                                  float minVal, float maxVal)
	{ 
		int hash = s.GetHashCode (); // get 32-bit number based on the description
		// grab distinct parts of the hash to use for each value.
		// 255 doesn't have a meaning, it's just based on an example I'm using for this
		float x = (hash & 0x0000FF); // 0 - 255
		float y = (hash & 0x00FF00) >> 8; // 0 - 255 from a different part of the hash
		float z = (hash & 0xFF0000) >> 16; // 0 - 255 from a different part of the hash
    
		float hue = Mathf.Lerp (minHue, maxHue, (x / 225f));
		float sat = Mathf.Lerp (minSat, maxSat, (y / 225f));
		float val = Mathf.Lerp (minVal, maxVal, (z / 225f));
    
		//Debug.Log ("With HSV: "+hue+", "+sat+", "+val);
		Color c = Utility.ColorFromHSV (hue, sat, val);
		//Debug.Log ("Getting color "+c);
    
		return c;
	}

	// hue: 0-360, sat: 0-1, val: 0-1
	public static Color ColorFromHSV (float hue, float saturation, float value)
	{
		// I have no idea how this works >_>
		int hi = Convert.ToInt32 (Mathf.Floor (hue / 60)) % 6;
		float f = hue / 60 - Mathf.Floor (hue / 60);
    
		//value = value * 255;
		float v = value;
		float p = value * (1 - saturation);
		float q = value * (1 - f * saturation);
		float t = value * (1 - (1 - f) * saturation);

		if (hi == 0)
			return new Color (v, t, p);
		else if (hi == 1)
			return new Color (q, v, p);
		else if (hi == 2)
			return new Color (p, v, t);
		else if (hi == 3)
			return new Color (p, q, v);
		else if (hi == 4)
			return new Color (t, p, v);
		else
			return new Color (v, p, q);
	}
}
