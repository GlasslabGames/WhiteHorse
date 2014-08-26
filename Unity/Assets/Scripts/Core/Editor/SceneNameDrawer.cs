using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(SceneNameAttribute))]
public class SceneNameDrawer : PropertyDrawer {

  public static string IsolateSceneName(string scenePath)
  {
    string name = scenePath.Substring(scenePath.LastIndexOf('/')+1);
    name = name.Substring(0, name.Length-6);
    return name;
  }

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    // Get the attribute for its argument information
    SceneNameAttribute sceneNameAttribute = attribute as SceneNameAttribute;
    SortedDictionary<int, string> states = new SortedDictionary<int, string> ();

    // If the scene allows a "none" choice, make it first
    if (sceneNameAttribute.AllowNone) {
      states.Add(-1, "<None>");
    }

    // Get the (real) list of scene names
    int? selectedSceneId = null; // Default scene is always set to the first one.
    string currentSceneName = IsolateSceneName(EditorApplication.currentScene);
    for (int i = 0; i < UnityEditor.EditorBuildSettings.scenes.Length; ++i)
    {
      UnityEditor.EditorBuildSettingsScene scene = UnityEditor.EditorBuildSettings.scenes[i];
      if (scene.enabled) {
        string name = IsolateSceneName(scene.path);
        if (!(sceneNameAttribute.HideCurrent && name.Equals(currentSceneName))) {
          states.Add(i, name);
        }

        if (name.Equals(property.stringValue)) {
          selectedSceneId = i;  // Check if this scene is our currently selected one (need the ID for the popup)
        }

      }
    }

    int[] ids = new int[states.Count];
    string[] names = new string[states.Count];
    states.Keys.CopyTo (ids, 0);
    states.Values.CopyTo (names, 0);

    // If we didn't find our state, or our state is invalid (not selectable), default it!
     if (selectedSceneId == null || (!states.ContainsKey(selectedSceneId.Value))) {
      // We couldn't find our scene ID... reset the scene to the first one.
      selectedSceneId = ids[0];
    }

    label = EditorGUI.BeginProperty(position, label, property);
    position = EditorGUI.PrefixLabel (position, GUIUtility.GetControlID (FocusType.Passive), label);
    selectedSceneId = EditorGUI.IntPopup(position, selectedSceneId.Value, names, ids);
    property.stringValue = (selectedSceneId.Value == -1) ? null : states[selectedSceneId.Value];
    EditorGUI.EndProperty();
  }
}