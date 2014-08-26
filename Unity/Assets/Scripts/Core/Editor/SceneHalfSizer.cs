
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System;

public class SceneHalfSizer : EditorWindow {
  private const string SUBFOLDER = "_halfSize";
  private const string FILE_SUFFIX = "_halfSize";
  private static readonly string[] UNITY_PLATFORMS = {"Web", "Standalone", "iPhone", "Android", "FlashPlayer"};

  private static bool SCRIPT_MAY_CHANGE_IMPORT_SETTINGS = false;


  public static void CreateHalfsizeScene(string scenePath, bool forceChangeSettings = false)
  {
    if (EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isPaused)
    {
      Debug.LogError("[SceneHalfSizer] Cannot create half-size scene while playing or compiling");
      return;
    }

    string originalScenePath = EditorApplication.currentScene;
    if (originalScenePath != scenePath)
    {
      EditorApplication.OpenScene(scenePath);
    }

    SpriteRenderer[] sprites = Resources.FindObjectsOfTypeAll<SpriteRenderer>();
    int spriteIndex;
    bool importSettingsChanged = false;
    for (spriteIndex=sprites.Length-1; spriteIndex>=0; spriteIndex--)
    {
      SpriteRenderer s = sprites[spriteIndex];
      if (AssetDatabase.Contains(s))
      {
        // This skips assets loaded in by Resources.FindObjectsOfTypeAll that are actually not in scene but loaded for the scene (ex. prefabs).
        //Debug.Log ("[SceneHalfSizer] " +s.name + " - is in asset database (probably prefab), skipping.");
        continue;
      }
      
      if (s.sprite == null || s.sprite.texture == null)
      {
        Debug.Log ("[SceneHalfSizer] " +s.name + " - does not contain sprite or texture, skipping.");
        continue;
      }
      
      string filePath = AssetDatabase.GetAssetPath(s.sprite.texture);
      string halfSizeFilePath = GetHalfsizeAssetPath(filePath);
      
      // Conform half-size import settings to full size import settings
      if (!File.Exists(halfSizeFilePath))
      {
        Debug.LogWarning ("[SceneHalfSizer] Could not find replacement asset at "+halfSizeFilePath+", skipping.");
        continue;
      }
      
      if (CheckSettingsDiffer(filePath, halfSizeFilePath))
      {
        if (forceChangeSettings || CheckChangeSettingsPermission())
        {
          ConformSettings(filePath, halfSizeFilePath);
          importSettingsChanged = true;
        }
        else
        {
          Debug.Log ("[SceneHalfSizer] Import settings differ but no permissions to change, reverting...");
          EditorApplication.OpenScene(EditorApplication.currentScene);
          
          Debug.Log ("[SceneHalfSizer] Complete.");
          return;
        }
      }
      
      Sprite newAsset = AssetDatabase.LoadAssetAtPath(halfSizeFilePath, typeof(Sprite)) as Sprite;
      // Set new asset
      s.sprite = newAsset;
    }
    
    UITexture[] uitextures = Resources.FindObjectsOfTypeAll<UITexture>();
    for (spriteIndex=uitextures.Length-1; spriteIndex>=0; spriteIndex--)
    {
      UITexture t = uitextures[spriteIndex];
      if (AssetDatabase.Contains(t))
      {
        // This skips assets loaded in by Resources.FindObjectsOfTypeAll that are actually not in scene but loaded for the scene (ex. prefabs).
        //Debug.Log ("[SceneHalfSizer] " +s.name + " - is in asset database (probably prefab), skipping.");
        continue;
      }
      
      if (t.mainTexture == null)
      {
        Debug.Log ("[SceneHalfSizer] " +t.name + " - does not contain sprite or texture, skipping.");
        continue;
      }
      
      string filePath = AssetDatabase.GetAssetPath(t.mainTexture);
      string halfSizeFilePath = GetHalfsizeAssetPath(filePath);
      
      // Conform half-size import settings to full size import settings
      if (!File.Exists(halfSizeFilePath))
      {
        Debug.LogWarning ("[SceneHalfSizer] Could not find replacement asset at "+halfSizeFilePath+", skipping.");
        continue;
      }
      
      if (CheckSettingsDiffer(filePath, halfSizeFilePath))
      {
        if (forceChangeSettings || CheckChangeSettingsPermission())
        {
          ConformSettings(filePath, halfSizeFilePath);
          importSettingsChanged = true;
        }
        else
        {
          Debug.Log ("[SceneHalfSizer] Import settings differ but no permissions to change, reverting...");
          EditorApplication.OpenScene(EditorApplication.currentScene);
          
          Debug.Log ("[SceneHalfSizer] Complete.");
          return;
        }
      }
      
      Sprite newAsset = AssetDatabase.LoadAssetAtPath(halfSizeFilePath, typeof(Sprite)) as Sprite;
      if (newAsset != null)
      {
        // Set new asset
        t.mainTexture = newAsset.texture;
      }
      else
      {
        Texture newAssetTexture = AssetDatabase.LoadAssetAtPath(halfSizeFilePath, typeof(Texture)) as Texture;
        if (newAssetTexture != null)
        {
          t.mainTexture = newAssetTexture;
        }
        else
        {
          Debug.LogError("[SceneHalfSizer] Asset we're trying to use is not a texture or sprite: "+halfSizeFilePath);
        }
      }
    }

    string sceneDirectory = Path.GetDirectoryName(scenePath) + "/" + SUBFOLDER;
    string newScenePath = GetHalfsizeAssetPath(scenePath);
    
    if (!Directory.Exists(sceneDirectory))
    {
      Debug.Log ("[SceneHalfSizer] Couldn't find scene halfsize folder. Creating...");
      Directory.CreateDirectory(sceneDirectory);
    }
    
    if (importSettingsChanged)
    {
      AssetDatabase.Refresh();
    }
    
    Debug.Log ("[SceneHalfSizer] Save to: "+newScenePath);
    EditorApplication.SaveScene(newScenePath,true);

    // Add new scene to build targets if its not already there
    EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
    bool targetSceneInBuild = false;
    bool targetSceneEnabled = false;
    bool createdSceneInBuild = false;
    for (int i=buildScenes.Length-1; i>=0; i--)
    {
      if (buildScenes[i].path == scenePath)
      {
        targetSceneInBuild = true;
        targetSceneEnabled = buildScenes[i].enabled;
        if (createdSceneInBuild) break;
      }
      else if (buildScenes[i].path == newScenePath)
      {
        createdSceneInBuild = true;
        if (targetSceneInBuild) break;
      }
    }

    if (targetSceneInBuild && !createdSceneInBuild)
    {
      Debug.Log ("[SceneHalfSizer] Source scene found in build settings, created scene added (" + (targetSceneEnabled ? "enabled" : "disabled") + ").");
      EditorBuildSettingsScene[] newBuildScenes = new EditorBuildSettingsScene[buildScenes.Length+1];
      buildScenes.CopyTo(newBuildScenes,0);
      newBuildScenes[newBuildScenes.Length-1] = new EditorBuildSettingsScene(newScenePath, targetSceneEnabled);
    }
    
    Debug.Log ("[SceneHalfSizer] Reloading original scene...");
    EditorApplication.OpenScene(originalScenePath);
    
    Debug.Log ("[SceneHalfSizer] Complete.");
  }

  [MenuItem("Edit/GlassLab Tools/Create Half-Sized Scene")]
  public static void CreateHalfsizeCurrentScene() {
    if (EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isPaused)
    {
      Debug.LogError("[SceneHalfSizer] Cannot create half-size scene while playing or compiling");
      return;
    }

    if (!EditorUtility.DisplayDialog(
      "Scene Half-Sizer", // title
      "This will save the current scene before executing. Continue?", // description
      "Save and Run", // Ok string
      "Cancel")) // Cancel string
    {
      return;
    }

    SCRIPT_MAY_CHANGE_IMPORT_SETTINGS = false; // Reset permissions check

    // Back-up scene save
    EditorApplication.SaveScene();

    CreateHalfsizeScene(EditorApplication.currentScene);
  }

  public static string GetHalfsizeAssetPath(string srcPath)
  {
    string directory = Path.GetDirectoryName(srcPath) + "/" + SUBFOLDER;
    string fileName = Path.GetFileNameWithoutExtension(srcPath);
    string extension = Path.GetExtension(srcPath);

    return directory + "/" + fileName + FILE_SUFFIX + extension;
  }

  public static bool CheckSettingsDiffer(string srcPath, string dstPath)
  {
    TextureImporter dst = (TextureImporter) TextureImporter.GetAtPath(dstPath);
    TextureImporter src = (TextureImporter) TextureImporter.GetAtPath(srcPath);
    int settingsIndex;
    TextureImporterSettings srcSettings = new TextureImporterSettings(); // Used later to handle texture settings
    TextureImporterSettings dstSettings = new TextureImporterSettings(); // Used later to handle texture settings
    
    dst.ReadTextureSettings(dstSettings);
    src.ReadTextureSettings(srcSettings);
    
    bool settingsDiffer = dstSettings.compressionQuality != srcSettings.compressionQuality ||
      dstSettings.filterMode != srcSettings.filterMode ||
        dstSettings.maxTextureSize != srcSettings.maxTextureSize ||
        dstSettings.spriteAlignment != srcSettings.spriteAlignment ||
        dstSettings.spriteMode != srcSettings.spriteMode ||
        dstSettings.spritePivot != srcSettings.spritePivot ||
        dstSettings.textureFormat != srcSettings.textureFormat ||
        dstSettings.wrapMode != srcSettings.wrapMode ||
        dstSettings.spritePixelsToUnits != srcSettings.spritePixelsToUnits/2f;
    
    int fullSizeTextureSize = 0, halfSizeTextureSize = 0;
    int fullSizeCompressionQuality = 0, halfSizeCompressionQuality = 0;
    TextureImporterFormat fullSizeFormat = 0, halfSizeFormat = 0;
    for (settingsIndex=UNITY_PLATFORMS.Length-1; !settingsDiffer && settingsIndex>=0; settingsIndex--)
    {
      
      src.GetPlatformTextureSettings(UNITY_PLATFORMS[settingsIndex], out fullSizeTextureSize, out fullSizeFormat);
      dst.GetPlatformTextureSettings(UNITY_PLATFORMS[settingsIndex], out halfSizeTextureSize, out halfSizeFormat);
      
      settingsDiffer = (fullSizeTextureSize != halfSizeTextureSize) ||
        (fullSizeCompressionQuality != halfSizeCompressionQuality) ||
          (fullSizeFormat != halfSizeFormat);
    }

    return settingsDiffer;
  }

  public static void ConformSettings(string srcPath, string dstPath)
  {
    try
    {
      TextureImporter dst = (TextureImporter) TextureImporter.GetAtPath(dstPath);
      TextureImporter src = (TextureImporter) TextureImporter.GetAtPath(srcPath);
      int settingsIndex;
      TextureImporterSettings srcSettings = new TextureImporterSettings(); // Used later to handle texture settings
      TextureImporterSettings dstSettings = new TextureImporterSettings(); // Used later to handle texture settings
      
      int srcTextureSize = 0;
      TextureImporterFormat srcFormat = 0;

      dst.ReadTextureSettings(dstSettings);
      src.ReadTextureSettings(srcSettings);
      
      // Check if settings differ
      if (!CheckChangeSettingsPermission())
      {
          Debug.Log ("[SceneHalfSizer] No permissions to conform settings.\nsrc: "+srcPath+"\ndst: "+dstPath);
          return;
      }
      
      srcSettings.CopyTo(dstSettings);
      dstSettings.spritePixelsToUnits = srcSettings.spritePixelsToUnits/2f; // HALF-SIZE TEXTURE
      dst.SetTextureSettings(dstSettings);
      
      for (settingsIndex=UNITY_PLATFORMS.Length-1; settingsIndex>=0; settingsIndex--)
      {
        src.GetPlatformTextureSettings(UNITY_PLATFORMS[settingsIndex], out srcTextureSize, out srcFormat);
        dst.SetPlatformTextureSettings(UNITY_PLATFORMS[settingsIndex], srcTextureSize, srcFormat);
      }
    
      AssetDatabase.ImportAsset(dstPath);
    }
    catch (Exception e)
    {
      Debug.Log("Error occured while conforming import settings. Src: "+srcPath + ", Dst: " + dstPath);
      Debug.Log(e);
    }
  }

  private static bool CheckChangeSettingsPermission(bool forceRecheck = false)
  {
    if (forceRecheck)
    {
      SCRIPT_MAY_CHANGE_IMPORT_SETTINGS = false;
    }

    if (!SCRIPT_MAY_CHANGE_IMPORT_SETTINGS && EditorUtility.DisplayDialog(
      "Import Settings Incorrect",
      "At least one texture import setting (pivot or pixel to unit size) is incorrect. Allow script to change import settings in the '_halfsize' folder?",
      "Allow",
      "Cancel"))
    {
      SCRIPT_MAY_CHANGE_IMPORT_SETTINGS = true;
    }

    return SCRIPT_MAY_CHANGE_IMPORT_SETTINGS;
    
  }
}
