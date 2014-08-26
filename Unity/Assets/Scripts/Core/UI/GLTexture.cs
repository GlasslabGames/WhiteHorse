using UnityEngine;

/// <summary>
/// Sets asset to UITexture or SpriteRenderer using an AssetBundle
/// </summary>
using System.Collections;


public class GLTexture : MonoBehaviour {
  public string bundleName;
  public string initialSpriteName;
  private UITexture m_texture;
  private SpriteRenderer m_spriteRenderer;
  private string m_currentSpriteName; // Name without any half-sizing implications

  public bool HasSprite(string name)
  {
    string resolutionSpecificName = Utility.GetResolutionSpecificName(name);
    return GLResourceManager.Instance.AssetExists(resolutionSpecificName);
  }

  public float alpha {
    get
    {
      if (m_texture != null)
      {
        return m_texture.alpha;
      }
      else if (m_spriteRenderer != null)
      {
        return m_spriteRenderer.color.a;
      }
      else
      {
        Debug.LogError("Could not find alpha");
        return 0f;
      }
    }
    set
    {
      if (m_texture != null)
      {
        m_texture.alpha = value;
      }
      else if (m_spriteRenderer != null)
      {
        m_spriteRenderer.color = new Color(m_spriteRenderer.color.r, m_spriteRenderer.color.g, m_spriteRenderer.color.b, value);
      }
      else
      {
        Debug.LogError("Could not set alpha");
      }
    }
  }

  public Texture mainTexture {
    get {
      if (m_texture != null)
      {
        return m_texture.mainTexture;
      }
      else if (m_spriteRenderer != null && m_spriteRenderer.sprite != null)
      {
        return m_spriteRenderer.sprite.texture;
      }
      else
      {
        return null;
      }
    }
    set {
      if (!value.name.Contains("_halfSize") &&
          GLResourceManager.ScreenHalfSize &&
          GLResourceManager.Instance.AssetExists(Utility.GetResolutionSpecificName(value.name)))
      {
        Debug.LogWarning("[GLTexture] Passed in texture '"+value.name+"' could be half-sized.", this);
        value = Resources.Load<Sprite>(GLResourceManager.Instance.GetResourceLocation(Utility.GetResolutionSpecificName(value.name))).texture;
      }

      if (m_texture != null)
      {
        m_texture.mainTexture = value;
      }
      else if (m_spriteRenderer != null)
      {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.AddTexture("_MainTex", value);
        m_spriteRenderer.SetPropertyBlock(block);
      }
      else
      {
        Debug.LogError("[GLTexture] Could not set texture since no UITexture or SpriteRenderer could be found!", this);
      }
    }
  }

  public string spriteName {
    get {
      return m_currentSpriteName;
    }
    set {
      setTexture(value);
    }
  }

  private void setTexture(string spriteName)
  {
    if (spriteName.Contains("_halfSize"))
    {
      spriteName = spriteName.Replace("_halfSize/", "");
      spriteName = spriteName.Replace("_halfSize", "");
    }
    if (enabled && gameObject.activeInHierarchy)
    {
      StartCoroutine(setTextureAtFrameEnd(spriteName));
    }
    else
    {
      doSetFrame(spriteName);
    }
  }

  // Sets texture at end of frame, avoids flicker
  private IEnumerator setTextureAtFrameEnd(string spriteName)
  {
    yield return new WaitForEndOfFrame();

    doSetFrame(spriteName);
  }

  private void doSetFrame(string spriteName)
  {
    Init();

    m_currentSpriteName = spriteName;
    // if this sprite is in one of our atlases, switch atlases
    string resolutionSpecificName = Utility.GetResolutionSpecificName(spriteName);
    if (GLResourceManager.Instance.AssetExists(resolutionSpecificName)) {
      resolutionSpecificName = GLResourceManager.Instance.GetResourceLocation(resolutionSpecificName);
    }
    else
    {
      Debug.LogError("[GLTexture] Asset '"+resolutionSpecificName+"' does not exist!", this);
    }
    
    // Find asset
    Sprite asset = Resources.Load<Sprite>(resolutionSpecificName);
    Texture tex = null;
    if (asset == null)
    {
      tex = Resources.Load<Texture>(resolutionSpecificName);
    }
    else
    {
      tex = asset.texture;
    }

    if (asset == null && tex == null)
    {
      Debug.LogError("[GLTexture] Asset is null: "+resolutionSpecificName, this);
      return;
    }
    
    // Set asset
    if (m_texture != null)
    {
      m_texture.mainTexture = tex;
    }
    else if (m_spriteRenderer != null)
    {
      if (asset != null)
      {
        m_spriteRenderer.sprite = asset;
      }
      else
      {
        // Sometimes this doesn't work?
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.AddTexture("_MainTex", tex);
        m_spriteRenderer.SetPropertyBlock(block);
      }
    }
    else
    {
      Debug.LogError("[GLTexture] No UITexture or SpriteRenderer. Sprite name '"+resolutionSpecificName+"' not set.", this);
      m_currentSpriteName = null;
    }
  }

  private void Init()
  {
    if (m_texture == null)
      m_texture = GetComponent<UITexture>();
    if (m_spriteRenderer == null)
      m_spriteRenderer = GetComponent<SpriteRenderer>();
  }

  void Awake() {
    Init();
    if (m_texture == null && m_spriteRenderer == null)
    {
      Debug.LogError("[GLTexture] Could not find UITexture or SpriteRenderer!", this);
    }

    if (m_currentSpriteName != null)
    {
      setTexture(m_currentSpriteName);
    }
    else
    {
      if (m_texture != null)
      {
        m_currentSpriteName = m_texture.mainTexture != null ? m_texture.mainTexture.name : null;
      }
      else if (m_spriteRenderer != null)
      {
        m_currentSpriteName = m_spriteRenderer.sprite != null ? m_spriteRenderer.sprite.texture.name : null;
      }
    }

    if (!string.IsNullOrEmpty(m_currentSpriteName))
    {
      int length = m_currentSpriteName.Length;
      if (m_currentSpriteName.Substring(length-9) == "_halfSize")
      {
        m_currentSpriteName = m_currentSpriteName.Substring(0,length-9);
      }
    }

    if (m_texture == null && m_spriteRenderer == null)
    {
      Debug.LogError("[GLTexture] Could not find UITexture or SpriteRenderer component on "+name, this);
      enabled = false;
    }

    if (!string.IsNullOrEmpty(initialSpriteName))
    {
      spriteName = initialSpriteName;
    }
  }
}
