using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Wrapper for GLSprite which can select among multiple atlases as needed.
/// </summary>
[RequireComponent(typeof(UISprite))]
public class GLSprite : MonoBehaviour {
  public UISprite Sprite;

  /**
   * NOTE: This list gets cleared during runtime to remove references to atlases.
   */
  public List<UIAtlas> Atlases;

  // store a dictionary of spriteName -> atlasName for atlas that contains that sprite
  //private Dictionary<string, UIAtlas> m_spriteAtlases;
  private Dictionary<string, string> m_spriteAtlases;

  // lowercase s for consistency with UISprite.spriteName
  public string spriteName {
    get {
      return Sprite.spriteName;
    }
    set {
      // if this sprite is in one of our atlases, switch atlases
      if (HasSprite(value)) {
        string atlasName = m_spriteAtlases[value];
        if (Sprite.atlas == null || Sprite.atlas.name != atlasName)
        {
          UIAtlas atlas = Resources.Load<UIAtlas>(atlasName);
          Sprite.atlas = atlas;
        }
      } else {
        Debug.LogWarning ("No sprite with name "+value+" in attached atlases!", this);
      }

      // set the spriteName anyway
      Sprite.spriteName = value;
    }
  }

  void Awake() {
    if (m_spriteAtlases == null)
    {
      MakeDictionary();
    }

    Atlases = null;
  }

	void Start () {}
	
	public bool HasSprite(string name) {
    if (m_spriteAtlases == null) MakeDictionary();

    return m_spriteAtlases.ContainsKey(name);
  }

  void MakeDictionary() {
    m_spriteAtlases = new Dictionary<string, string>();
    foreach (UIAtlas atlas in Atlases) {
      BetterList<string> list = atlas.GetListOfSprites();
      foreach (string name in list) {
        if (m_spriteAtlases.ContainsKey(name)) {
          Debug.LogWarning("Multiple atlases have sprites named "+name, this);
        } else {
          m_spriteAtlases.Add (name, atlas.name);
        }
      }
    }
  }
}
