
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using MiniJSON;

public class CustomBuilder : EditorWindow {
  private const string RESOURCES_FOLDER = "Resources";
  private const string MANIFEST_SCENE_KEY = "__Scenes__";
  private const string MANIFEST_BUNDLE_KEY = "__Bundles__";
  private const string MANIFEST_RESOURCE_KEY = "__Resources__";
  private const string MANIFEST_VERSION = "__Version__";
  private const string MANIFEST_BUNDLE_ID = "__BundleID__";
  public const string MANIFEST_FILE_NAME = "manifest.json";

  [MenuItem("Edit/GlassLab Tools/Build/iOS/Release")]
  public static void BuildIOS()
  {
    Build (BuildTarget.iPhone, BuildOptions.None);
  }
  
  [MenuItem("Edit/GlassLab Tools/Build/iOS/Debug")]
  public static void BuildIOSDebug()
  {
    Build (BuildTarget.iPhone, BuildOptions.AllowDebugging | BuildOptions.Development | BuildOptions.ConnectWithProfiler);
  }

  [MenuItem("Edit/GlassLab Tools/Build/Android/Release")]
  public static void BuildAndroid()
  {
    Build (BuildTarget.Android, BuildOptions.None);
  }
  
  [MenuItem("Edit/GlassLab Tools/Build/Android/Debug")]
  public static void BuildAndroidDebug()
  {
    Build (BuildTarget.Android, BuildOptions.AllowDebugging | BuildOptions.Development | BuildOptions.ConnectWithProfiler);
  }

  public static void Build(BuildTarget target, BuildOptions options) {


    string path = EditorUtility.SaveFolderPanel(
      "GlassLab Build", // title
      "", // folder
      "" // defaultName
      );
    //"Assets/myAssetBundle.unity3d";
    
    EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
    List<string> scenePaths = new List<string>();
    for (int i=0; i < scenes.Length; i++)
    {
      if (scenes[i].enabled)
      {
        scenePaths.Add(scenes[i].path);
      }
    }
    BuildPipeline.BuildPlayer(scenePaths.ToArray(), path, target, options);
    //Object[] selection =  Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
    
    //BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, target);
    
  }
  
  [MenuItem("Edit/GlassLab Tools/Build Bundle Manifest")]
  public static void BuildManifestFile()
  {
    Debug.Log ("[CustomBuilder] Creating resource manifest...");
    double time = EditorApplication.timeSinceStartup;
    Dictionary<string, object> jsonData = new Dictionary<string, object>();
    Dictionary<string, string> resourcesData = new Dictionary<string, string>(); // <ResourceName, Path>
    Dictionary<string, string> bundleData = new Dictionary<string, string>(); // <ResourceName, BundleName>
    List<string> sceneData = new List<string>(); // List of scene names
    string[] assetPaths = AssetDatabase.GetAllAssetPaths();
    int i;

    jsonData[MANIFEST_VERSION] = PlayerSettings.bundleVersion;
    jsonData[MANIFEST_BUNDLE_ID] = PlayerSettings.bundleIdentifier;

    // Go through all asset bundles
    for (i=assetPaths.Length-1; i >= 0; i--)
    {
      string path = assetPaths[i];
      if (Directory.Exists(path)) // Skip directories
      {
        continue;
      }

      string fileName = Path.GetFileNameWithoutExtension(path);
      string directory = Path.GetDirectoryName(path);
      int directoryResourcesIndex = directory.LastIndexOf(RESOURCES_FOLDER);
      if (directoryResourcesIndex != -1)
      {
        string directoryFromResources = directory.Substring(Mathf.Min (directoryResourcesIndex + RESOURCES_FOLDER.Length+1, directory.Length));
        if (Path.GetExtension(path).ToLower() == ".unity3d")
        {
          // Load bundle
          string _url = string.Concat("file:///", Application.dataPath, "/../", path);
          WWW _loader = new WWW(_url);
          AssetBundle bundle = _loader.assetBundle;

          // Load all objects, add their names
          Object[] obj = bundle.LoadAll();
          foreach (Object o in obj)
          {
            if (bundleData.ContainsKey(o.name) && bundleData[o.name] != fileName)
            {
              Debug.LogError("[CustomBuilder] Multiple objects of name '"+o.name+"' @Bundle \n'" + fileName + "'\n'"+resourcesData[o.name]+"'");
            }
            else
            {
              bundleData[o.name] = fileName;
            }
          }

          // Release bundle asset memory
          bundle.Unload(true);
        }
        
        if (resourcesData.ContainsKey(fileName) && resourcesData[fileName] != directoryFromResources)
        {
          Debug.LogWarning("[CustomBuilder] Multiple objects of name '"+fileName+"' @\n'" + directoryFromResources + "'\n'"+resourcesData[fileName]+"'");
        }
        else
        {
          resourcesData[fileName] = directoryFromResources;
        }
      }
    }
    jsonData[MANIFEST_RESOURCE_KEY] = resourcesData;
    jsonData[MANIFEST_BUNDLE_KEY] = bundleData;

    // Go through scenes
    EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
    for (i=scenes.Length-1; i>=0; i--)
    {
      sceneData.Add(Path.GetFileNameWithoutExtension(scenes[i].path));
    }
    jsonData[MANIFEST_SCENE_KEY] = sceneData;

    string json = Json.Serialize(jsonData);

    File.WriteAllText(Application.dataPath + "/"+RESOURCES_FOLDER+"/" + MANIFEST_FILE_NAME, json);
    AssetDatabase.Refresh();
    Debug.Log ("[CustomBuilder] Complete, "+ Mathf.Round((float)(EditorApplication.timeSinceStartup - time) * 1000f) + "ms\n" + json);
  }
}