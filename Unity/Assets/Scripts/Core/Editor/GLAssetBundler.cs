
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class GLAssetBundler : EditorWindow {
  public static void CreateBundle(BuildTarget target) {
    string path = EditorUtility.SaveFilePanelInProject(
      "Create Asset Bundle", // title
      "assetBundle.unity3d", // defaultName
      "unity3d", // extension
      "Choose a location for the asset bundle" // message
      );
      //"Assets/myAssetBundle.unity3d";


    Object[] selection =  Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

    //BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, target);
    BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path);
    //Debug.Log(".");
  }
  
  [MenuItem("Edit/GlassLab Tools/Create AssetBundle from Selection/iOS")]
  public static void CreateIOSBundle()
  {
    CreateBundle (BuildTarget.iPhone);
  }
  [MenuItem("Edit/GlassLab Tools/Create AssetBundle from Selection/Android")]
  public static void CreateAndroidBundle()
  {
    CreateBundle (BuildTarget.Android);
  }
}