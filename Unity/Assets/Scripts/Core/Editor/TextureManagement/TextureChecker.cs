using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// TODO: This class only supports one compression format.  Should look into a wider array depending on source material

public class TextureChecker : EditorWindow {
  [UnityEditor.MenuItem("Assets/Check Texture Configs")]
  static void CheckAllTextureConfigs()
  {
    string startingPath = (Selection.activeObject == null ) ? "Assets" : AssetDatabase.GetAssetPath(Selection.activeObject); 
    _AssetChecker assetChecker = new _AssetChecker();
    assetChecker.Analyze(startingPath);
    //TextureChecker window = (TextureChecker)EditorWindow.GetWindow (typeof (TextureChecker));
  }

  private class _AssetChecker {

    public void Analyze(string startingPath) {
      // Analyze to see what we have here.
      List<string> filesToScan = new List<string>();
      if (Directory.Exists(startingPath)) {
        filesToScan = FindFiles(startingPath);
      } else if (File.Exists(startingPath)) {
        // this looks like a file... just add this and continue
        filesToScan.Add(startingPath);
      }

      // Check with the user for large jobs.
      bool runJob = true;
      // If theres more than 1 file selected to scan, prompt to ensure thats what they mean (a single file is easy enough to undo)
      if (filesToScan.Count > 1) {
        if (!EditorUtility.DisplayDialog("Change Texture Settings?", 
                                         string.Format("Process ({1} files) from {0}? ",startingPath, filesToScan.Count),
                                         "Continue",
                                         "Cancel")) {
          Debug.Log("Canceled on startup by the user");
          runJob = false;
        }
      }
       

      if (runJob) {
        float fileCount = 0;
        foreach (string file in filesToScan)
        {
          if (EditorUtility.DisplayCancelableProgressBar("Updating Texture Configurations", file, ++fileCount/filesToScan.Count)) {
            Debug.Log("Canceled by the user");
            return;
          }

          if (AnalyzeFile(file)) {
            AssetDatabase.ImportAsset(file, ImportAssetOptions.ForceUpdate);
          }
        }
      }

      Debug.Log("TextureChecker Done.");
      EditorUtility.ClearProgressBar();
    }

    private static bool IsPowerOfTwo(ulong x) 
    {
      return (x & (x - 1)) == 0;
    }

    private static void ResetFormat(string filePath, TextureImporterFormat targetFormat) {
      TextureImporter textureImporter = AssetImporter.GetAtPath(filePath) as TextureImporter;
      textureImporter.textureFormat = targetFormat;
    }

    private bool AnalyzeFile(string file) 
    {
      Texture2D texture =  AssetDatabase.LoadAssetAtPath(file,typeof(Texture2D)) as Texture2D;

      try {
        if (texture != null) {
          return (SpriteRuleset.CalculateOptimalTextureSettings(file, texture) != null);
        }
        return false;
      } finally {
        // clean up memory.  If we don't do this, it all accumulates in memory until we're done.
        EditorUtility.UnloadUnusedAssetsIgnoreManagedReferences(); 
      }
    }

    /// <summary>
    /// Recursively find all the files in the folder
    /// </summary>
    /// <returns>The folder to scan at this stage</returns>
    /// <param name="relativePath">Relative path.</param>
    private List<string> FindFiles(string relativePath) {
      List<string> files = new List<string>();
      EditorUtility.DisplayProgressBar("Finding Assets", relativePath , 0.5f);

      // Process files, then dive into directories.  The only files we know we don't want are .meta files
      files.AddRange(Directory.GetFiles(relativePath).Where(name => !name.EndsWith(".meta")));

      // Dive into any subdirectories looking for more files.
      foreach (string directory in Directory.GetDirectories(relativePath)) {
        files.AddRange(FindFiles(directory));
      }

      return files;
    }
  }
}
