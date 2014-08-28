using UnityEditor;
using UnityEditor.Sprites;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

class PVRTC4CompressionPackerPolicy : IPackerPolicy
{
  private class Entry
  {
    public Sprite         sprite;
    public AtlasSettings  settings;
    public string         tag;
    public SpriteMeshType meshType;
  }
  
  public int GetVersion()
  {
    return 1;
  }

  public void OnGroupAtlases(BuildTarget target, PackerJob job, int[] textureImporterInstanceIDs)
  {
    List<Entry> entries = new List<Entry>();
    for (int i = 0; i < textureImporterInstanceIDs.Length; i++)
    {
      TextureImporter ti = (TextureImporter)EditorUtility.InstanceIDToObject(textureImporterInstanceIDs[i]);
      TextureImportInstructions ins = new TextureImportInstructions();
      ti.ReadTextureImportInstructions(ins, target);
      
      TextureImporterSettings tis = new TextureImporterSettings();
      ti.ReadTextureSettings(tis);
      
      Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(ti.assetPath).Select(x => x as Sprite).Where(x => x != null).ToArray();
      foreach (Sprite sprite in sprites)
      {
        Entry entry = new Entry();
        entry.sprite = sprite;
        entry.settings.format = TextureFormat.PVRTC_RGBA4; //ins.desiredFormat;
        entry.settings.usageMode = ins.usageMode;
        entry.settings.colorSpace = ins.colorSpace;
        entry.settings.compressionQuality = ins.compressionQuality;
        entry.settings.filterMode = ti.filterMode;
        entry.settings.maxWidth = 2048;
        entry.settings.maxHeight = 2048;
        entry.tag = ti.spritePackingTag;
        entry.meshType = tis.spriteMeshType;
        
        entries.Add(entry);
      }
    }
    
    // First split sprites into groups based on packing tag
    var tagGroups =
      from e in entries
        group e by e.tag;
    foreach (var tagGroup in tagGroups)
    {
      int page = 0;
      // Then split those groups into smaller groups based on texture settings
      var settingsGroups =
        from t in tagGroup
          group t by t.settings;
      foreach (var settingsGroup in settingsGroups)
      {
        string atlasName = string.Format("{0}", tagGroup.Key);
        if (settingsGroups.Count() > 1)
          atlasName += string.Format(" (Group {0})", page);
        
        job.AddAtlas(atlasName, settingsGroup.Key);
        foreach (Entry entry in settingsGroup)
        {
          SpritePackingMode packingMode = (entry.meshType == SpriteMeshType.Tight) ? SpritePackingMode.Tight : SpritePackingMode.Rectangle;
          job.AssignToAtlas(atlasName, entry.sprite, packingMode, SpritePackingRotation.None);
        }
        
        ++page;
      }
    }
  }
}
