using UnityEngine;
using UnityEditor;
using System.Collections;

public static class SpriteRuleset {
  public static string[] ms_platforms = {"Web", "Standalone", "iPhone", "Android", "Flashplayer"};

  private static bool IsPowerOfTwo(ulong x) 
  {
    return (x & (x - 1)) == 0;
  }

  public static bool IsTextureFormatCompressed(TextureFormat format) 
  {
    return true;
  }

  public static TextureImporterFormat SelectTextureFormat(TextureImporter textureImporter, Texture2D texture)
  {
    TextureImporterFormat targetFormat = textureImporter.textureFormat;
    if ((texture.width == texture.height) && IsPowerOfTwo((ulong)texture.width)) {
      // Image is square POT - this image should be compressed (iPhone specific?)
      targetFormat = TextureImporterFormat.AutomaticCompressed;
    } else if (targetFormat != TextureImporterFormat.Automatic16bit && targetFormat != TextureImporterFormat.AutomaticTruecolor) {
      // TODO: figure out if it should be 16 bit.
      targetFormat = TextureImporterFormat.AutomaticTruecolor;
    }
    
    return targetFormat;
  }

  public static int SelectTextureSize(Texture2D texture)
  {
    return 2048;
  }

  public static TextureImporter CalculateOptimalTextureSettings(string file, Texture2D texture) 
  {
    bool changed = false;
    TextureImporter textureImporter = AssetImporter.GetAtPath(file) as TextureImporter;
    if ((textureImporter == null) || (textureImporter.textureType != TextureImporterType.Sprite)) {
      // null == not actually a texture, probably a font...
      // ! a sprite == don't mess with it, this is a sprite-only ruleset.
      return null;
    }
    TextureImporterFormat importFormat = SelectTextureFormat(textureImporter, texture);

    // If we have an importFormat apply it; otherwise accept the system defaults.
    if (textureImporter.textureFormat != importFormat) {
      textureImporter.textureFormat = importFormat;
      changed = true;
    }

    int maxTextureSize = SelectTextureSize(texture);
    if (textureImporter.maxTextureSize != maxTextureSize) {
      textureImporter.maxTextureSize = maxTextureSize;
      changed = true;
    }

    // TODO: allow overrides?  Right now, no
    // There is no good way to simply check if there are custom settings.  So we check the values; if they differ, clear.
    foreach (string platform in ms_platforms) {
      textureImporter.GetPlatformTextureSettings(platform, out maxTextureSize, out importFormat);
      if (textureImporter.maxTextureSize != maxTextureSize || textureImporter.textureFormat != importFormat) {
        textureImporter.ClearPlatformTextureSettings(platform);
        changed = true;
      }
    }

    // Return our suggested import settings
    return (changed) ? textureImporter : null;
  }



}
