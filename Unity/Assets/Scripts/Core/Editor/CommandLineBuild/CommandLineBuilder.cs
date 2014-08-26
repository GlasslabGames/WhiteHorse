using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class CommandLineBuilder : Editor {
  protected static readonly string ms_baseBuildPath = "Builds/";
  [UnityEditor.MenuItem("File/Command Line Build/Build iOS")]
  static void Build ()
  {
    List<string> scenes = new List<string>();
    
    foreach(EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
    {
      if(scene != null && scene.enabled) {
        scenes.Add(scene.path);
      }
    }
    if (scenes.Count == 0) {
      // No scenes to build here...
      return;
    }

    // Get some original values we will be messing with locally, but don't want to permanently change.
    string originalbundleVersion = PlayerSettings.bundleVersion;
    //BuildTarget originalTarget = EditorUserBuildSettings.activeBuildTarget;

    BuildTarget buildTarget = BuildTarget.iPhone;
    BuildTargetGroup buildTargetGroup = BuildTargetGroup.iPhone;
    // Beyond this point, we need to ensure that we reverse the temporary changes to any variables we don't want to change.
    try {
      string buildNumberString = CommandLineReader.GetCustomArgument ("Build");
      string pathString = CommandLineReader.GetCustomArgument ("Path");

      if (string.IsNullOrEmpty(pathString))
      {
        pathString = "";
      }
      else if (pathString.StartsWith("/"))
      {
        pathString = pathString.Substring(1);
      }

      string preprocessorString = CommandLineReader.GetCustomArgument ("Preprocessor");
      bool isDebug = true;
	  bool isClassroom = false;

      if (!string.IsNullOrEmpty(preprocessorString))
      {
        Debug.Log ("Found preprocessor definitions:" + preprocessorString);
        string[] preprocessorDefinitions = preprocessorString.Split(new char[] {';'});
        for (int i=preprocessorDefinitions.Length-1; i >= 0; i--)
        {
			if (preprocessorDefinitions[i] == "GAME_RELEASE")
			{
				Debug.Log ("Preprocessor GAME_RELEASE found");
				isDebug = false;
			}
			else if( preprocessorDefinitions[ i ] == "CLASSROOM" )
			{
				Debug.Log ("Preprocessor CLASSROOM found");
				isClassroom = true;
			}
        }
      }

      PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, preprocessorString);
      PlayerSettings.bundleVersion += ".";

      string buildPath = ms_baseBuildPath + pathString; // by default, the buildpath is the ms_baseBuildpath.
      if (!buildPath.EndsWith("/"))
      {
        buildPath += "/";
      }
      
      // We treat an empty buildstring as the flag that this is being run from the editor, not a build system
      if (buildNumberString == null) {
        PlayerSettings.bundleVersion += "dev";
      } else {
        PlayerSettings.bundleVersion += buildNumberString;
      }

      PlayerSettings.companyName = "GlassLab, Inc";
      if (isDebug)
      {
        Debug.Log ("Setting up for debug build...");
		if( !isClassroom )
		{
			PlayerSettings.productName = "Mars Generation One: Argubot Academy (stage)";
			PlayerSettings.bundleIdentifier = "org.glasslab.marsaastage";
		}
		else
		{
			PlayerSettings.productName = "MGO EDU (stage)";
			PlayerSettings.bundleIdentifier = "org.glasslab.marsaaedustage";
		}

        Texture2D[] icons = PlayerSettings.GetIconsForTargetGroup(buildTargetGroup);
        for (int i=icons.Length-1; i>=0; i--)
        {
          Texture2D icon = icons[i];
          if (icon != null)
          {
            string path = AssetDatabase.GetAssetPath(icon);
			if( isClassroom ) {
            	path = Path.GetDirectoryName(path) + "/edu/" + Path.GetFileName(path);
			}
			else {
				path = Path.GetDirectoryName(path) + "/stage_" + Path.GetFileName(path);
			}
            Debug.Log("Looking for replacement for "+icon.name+" at "+path+".");
            Texture2D replacement = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
            if (replacement != null)
            {
              Debug.Log ("Replacement for "+icon.name+" found, replacing with "+replacement.name+".");
              icons[i] = replacement;
            }
          }
        }
        
        PlayerSettings.SetIconsForTargetGroup(buildTargetGroup, icons);
      }
      else
      {
		if( !isClassroom )
		{
			PlayerSettings.productName = "Mars Generation One: Argubot Academy";
			PlayerSettings.bundleIdentifier = "org.glasslab.marsaa";
		}
		else
		{
			PlayerSettings.productName = "MGO EDU";
			PlayerSettings.bundleIdentifier = "org.glasslab.marsaaedu";

			Texture2D[] icons = PlayerSettings.GetIconsForTargetGroup(buildTargetGroup);
			for (int i=icons.Length-1; i>=0; i--)
			{
				Texture2D icon = icons[i];
				if (icon != null)
				{
					string path = AssetDatabase.GetAssetPath(icon);
					path = Path.GetDirectoryName(path) + "/edu/" + Path.GetFileName(path);
					Debug.Log("Looking for replacement for "+icon.name+" at "+path+".");
					Texture2D replacement = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
					if (replacement != null)
					{
						Debug.Log ("Replacement for "+icon.name+" found, replacing with "+replacement.name+".");
						icons[i] = replacement;
					}
				}
			}
			
			PlayerSettings.SetIconsForTargetGroup(buildTargetGroup, icons);
		}
      }

      // Create base path if it doesn't yet exist.
      if (!Directory.Exists(ms_baseBuildPath)) {
        Directory.CreateDirectory(ms_baseBuildPath);
      }

      Debug.Log (string.Format ("Writing output to [{0} ({1})]",buildPath, Path.GetFullPath(buildPath)));
      if (Directory.Exists(buildPath)) {
		    Debug.Log ("[" + buildPath + "] already exists...");
        if (buildNumberString == null) {
          int decision = EditorUtility.DisplayDialogComplex("Export location [" + buildPath + "] already exists","Continuing will overwrite/append the existing export","Delete and Continue", "Cancel", "Continue");
          switch (decision) {
          case 0:
            Debug.Log ("Deleting [" + buildPath + "]...");
            Directory.Delete(buildPath,true);
            break;
          case 1:
            Debug.Log ("Canceling build.");
            return;
          case 2:
          default:
            Debug.Log ("Attempting to overwrite/append [" + buildPath + "]...");
            // nothing to do
            break;
          }
        } else {
          // build systems will always delete the path if it exists.
          Debug.Log ("Automatically deleting [" + buildPath + "] due to non-interactive session...");
          Directory.Delete(buildPath,true);
        }
      }
      Debug.Log ("Building to [" + buildPath + "]");
      // Remember to switch the editor build target (system gets very unhappy if you try to buildplayer with a different target than the editor's build platform)
      EditorUserBuildSettings.SwitchActiveBuildTarget(buildTarget);
	    CustomBuilder.BuildManifestFile();
      BuildPipeline.BuildPlayer(scenes.ToArray(), buildPath, buildTarget, /* BuildOptions.AcceptExternalModificationsToPlayer |*/ BuildOptions.EnableHeadlessMode);
    } finally {
      // Gurantee the restoration of some configuration parameters.
      //EditorUserBuildSettings.SwitchActiveBuildTarget(originalTarget);
      PlayerSettings.bundleVersion = originalbundleVersion;
    }
  }
}
