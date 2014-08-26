
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class GLAssetImportListener : AssetPostprocessor
{
  private static HashSet<string> IGNORED_EXTENSIONS = new HashSet<string>(){
    ".cs"
  };

  private static HashSet<string> IMAGE_EXTENSIONS = new HashSet<string>(){
    ".png", ".jpg", ".psd"
  };
  private const double MANIFEST_REGEN_WAIT_TIME = .25; // wait time in seconds between asset changes to update.

  private static HashSet<string> modifyingAssets = new HashSet<string>();

  private static double lastUpdateTime = double.NaN;

  // TODO: Unity sometimes spams these when saving scenes/prefabs, queue them up!
  private static void OnPostprocessAllAssets (
    string[] importedAssets,
    string[] deletedAssets,
    string[] movedAssets,
    string[] movedFromAssetPaths)
  {
    string debugOutput = "";
    int i;
    string asset;

    bool manifestDirty = false;

    for (i=importedAssets.Length-1; i>=0; i--)
    {
      asset = importedAssets[i];
      if (modifyingAssets.Contains(asset)) continue;
      modifyingAssets.Add(asset);
      manifestDirty = manifestDirty || (
         Path.GetFileName(asset) != CustomBuilder.MANIFEST_FILE_NAME &&
         !IGNORED_EXTENSIONS.Contains(Path.GetExtension(asset))
         );
      debugOutput += "\nIMPORT: " + asset;

      if (IMAGE_EXTENSIONS.Contains(Path.GetExtension(asset)))
      {
        if (Path.GetFileNameWithoutExtension(asset).EndsWith("_halfSize"))
        {
          // TODO Fix, too annoying
          string originalFileName = Path.GetFileNameWithoutExtension(asset);
          if (!originalFileName.EndsWith("_halfSize")) continue;
          originalFileName = originalFileName.Substring(0, originalFileName.LastIndexOf("_halfSize")); // remove suffix
          if (!asset.Contains("/_halfSize/")) continue;
          string originalAssetPath = Path.GetDirectoryName(asset).Substring(0,asset.LastIndexOf("/_halfSize/"))+"/"+ originalFileName + Path.GetExtension(asset);
          if (File.Exists(originalAssetPath) && SceneHalfSizer.CheckSettingsDiffer(originalAssetPath, asset))
          {
            debugOutput += "\n[GLAssetImporter] Conforming asset to parent: "+originalAssetPath;
            
            SceneHalfSizer.ConformSettings(originalAssetPath, asset);
          }
        }
        else if(File.Exists(SceneHalfSizer.GetHalfsizeAssetPath(asset)))
        {
          string halfSizeAssetPath = SceneHalfSizer.GetHalfsizeAssetPath(asset);
          if (SceneHalfSizer.CheckSettingsDiffer(asset, halfSizeAssetPath))
          {
            debugOutput += "\n[GLAssetImporter] Parent changed, conforming asset to parent: "+halfSizeAssetPath;
            
            SceneHalfSizer.ConformSettings(asset, halfSizeAssetPath);
          }
        }
      }
      else if (Path.GetExtension(asset) == ".unity" && File.Exists(SceneHalfSizer.GetHalfsizeAssetPath(asset)))
      {
        debugOutput += "\n[GLAssetImporter] HalfSize scene found for "+asset+", recreating halfsize scene.";
        queueHalfsizeSceneGeneration(asset);
      }
      else if (Directory.Exists(asset))
      {
        checkEmptyAndRemoveDirectory(asset);
      }
    }
    
    for (i=deletedAssets.Length-1; i>=0; i--)
    {
      asset = deletedAssets[i];
      if (modifyingAssets.Contains(asset)) continue;
      modifyingAssets.Add(asset);
      manifestDirty = manifestDirty || (
        Path.GetFileName(asset) != CustomBuilder.MANIFEST_FILE_NAME &&
        !IGNORED_EXTENSIONS.Contains(Path.GetExtension(asset))
        );
      debugOutput += "\nDELETE: " + asset;

      if (Path.GetExtension(asset) == ".unity")
      {
        string halfSizeScenePath = SceneHalfSizer.GetHalfsizeAssetPath(asset);
        if (File.Exists(halfSizeScenePath))
        {
          debugOutput += "\n[GLAssetImporter] Parent scene for halfsize scene was deleted: " + asset;
        
          if (EditorUtility.DisplayDialog(
            "Parent scene deleted",
            "Parent scene for halfsize scene was deleted. Delete halfsize scene?\nParent: "+asset+"\nHalfsize Scene: "+halfSizeScenePath,
            "Delete",
            "Keep"))
          {
            File.Delete(halfSizeScenePath);
          }
        }
      }

      if (Directory.Exists(Path.GetDirectoryName(asset)))
      {
        checkEmptyAndRemoveDirectory(Path.GetDirectoryName(asset));
      }
    }

    for (i=movedAssets.Length-1; i>=0; i--)
    {
      asset = movedAssets[i];
      if (modifyingAssets.Contains(asset)) continue;
      modifyingAssets.Add(asset);
      string originalPath = movedFromAssetPaths[i];
      manifestDirty = manifestDirty || (
        Path.GetFileName(asset) != CustomBuilder.MANIFEST_FILE_NAME &&
        !IGNORED_EXTENSIONS.Contains(Path.GetExtension(asset))
        );
      debugOutput += "\nMOVE: " + asset + "\n\t\tFROM: " + originalPath;

      if (Path.GetExtension(asset) == ".unity")
      {
        string halfSizeScenePath = SceneHalfSizer.GetHalfsizeAssetPath(originalPath);
        if (File.Exists(halfSizeScenePath))
        {
          debugOutput += "\n[GLAssetImporter] Parent scene for halfsize scene was deleted: " + asset;
          
          if (EditorUtility.DisplayDialog(
            "Parent scene deleted",
            "Parent scene for halfsize scene was moved. The halfsize scene will be regenerated, but do you want to keep the unmoved original halfsize scene?\nParent: "+asset+"\nHalfsize Scene: "+halfSizeScenePath,
            "Delete",
            "Keep"))
          {
            File.Delete(halfSizeScenePath);
          }
          
          queueHalfsizeSceneGeneration(asset);
        }
      }

      if (Directory.Exists(Path.GetDirectoryName(originalPath)))
      {
        checkEmptyAndRemoveDirectory(Path.GetDirectoryName(originalPath));
      }
    }

    if (manifestDirty || modifyingAssets.Count != 0)
    {
      if (!string.IsNullOrEmpty(debugOutput))
        Debug.Log("[GLAssetImportListener] Assets changed: " + debugOutput + "\n-----------------");

      if (double.IsNaN(lastUpdateTime))
      {
        EditorApplication.update += buildManifestAfterDelay;
        EditorApplication.playmodeStateChanged += buildManifestNow;
      }
      lastUpdateTime = EditorApplication.timeSinceStartup;
    }
  }

  private static bool shouldIgnoreAsset(string path)
  {
    return
      Path.GetFileName(path) != CustomBuilder.MANIFEST_FILE_NAME &&
      !IGNORED_EXTENSIONS.Contains(Path.GetExtension(path));
  }

  private static void checkEmptyAndRemoveDirectory(string path)
  {
    string[] filePathsInDirectory = Directory.GetFiles(path);
    if (filePathsInDirectory.Length == 0)
    {
      if (EditorUtility.DisplayDialog(
        "Empty directory detected",
        "An empty directory was detected. If it remains empty it will cause .meta file spam in git. Delete?\n"+path,
        "Delete",
        "Keep"))
      {
        Directory.Delete(path);
      }
    }
  }
  
  private static void createHalfsizeScenesNow()
  {
    for (int i=dirtyHalfsizeScenes.Count-1; i>=0; i--)
    {
      SceneHalfSizer.CreateHalfsizeScene(dirtyHalfsizeScenes[i]);
    }

    dirtyHalfsizeScenes.Clear();
  }

  private static List<string> dirtyHalfsizeScenes = new List<string>();
  private static void queueHalfsizeSceneGeneration(string scene)
  {
    if (!dirtyHalfsizeScenes.Contains(scene))
    {
      dirtyHalfsizeScenes.Add(scene);
    }
  }

  private static void buildManifestNow()
  {
    EditorApplication.playmodeStateChanged -= buildManifestNow;
    EditorApplication.update -= buildManifestAfterDelay;

    lastUpdateTime = double.NaN;

    CustomBuilder.BuildManifestFile();

    modifyingAssets.Clear();
  }

  private static void buildManifestAfterDelay()
  {
    if (EditorApplication.timeSinceStartup - lastUpdateTime >= MANIFEST_REGEN_WAIT_TIME)
    {
      createHalfsizeScenesNow();

      buildManifestNow();
    }
  }
}