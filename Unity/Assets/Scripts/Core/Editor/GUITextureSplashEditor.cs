using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(GUITextureSplash))] 
public class GUITextureSplashEditor : Editor {
  public override void OnInspectorGUI() {
    GUITextureSplash script = (GUITextureSplash)target;

    // Get the game tab view size.
    System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
    System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Vector2 Res = (Vector2)GetSizeOfMainGameView.Invoke(null,null);

    // Ensure our local transform is 0ed out
    if (!script.transform.position.Equals (Vector3.zero)) {
      script.transform.position = Vector3.zero;
    }

    // Ensure our rotation is clear
    if (!script.transform.rotation.Equals (Quaternion.identity)) {
      script.transform.rotation = Quaternion.identity;
    }

    // Ensure our scale is set to 0s (this is appropraite for GUITextures)
    if (!script.transform.localScale.Equals (Vector3.zero)) {
      script.transform.localScale = Vector3.zero;
    }

    script.FixTransform ((int)Res.x, (int)Res.y);

    // Do our normal editor stuff
    base.OnInspectorGUI ();
  }

}
