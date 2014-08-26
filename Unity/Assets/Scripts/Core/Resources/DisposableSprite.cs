using UnityEngine;

public class DisposableSprite : MonoBehaviour
{
  private SpriteRenderer m_spriteRenderer;
  private UITexture m_uiTexture;
  private GLTexture m_glTexture;

  [SerializeField]
  private string m_resourceName;

  private bool m_initialized = false;

  public bool DisposeOnDisable = true;

  void Awake()
  {
    init ();
  }

  private void init()
  {
    if (!m_initialized)
    {
      m_initialized = true;
      m_glTexture = GetComponent<GLTexture>();
      m_spriteRenderer = GetComponent<SpriteRenderer> ();
      if (m_spriteRenderer == null)
      {
        m_uiTexture = GetComponent<UITexture> ();
        if (m_uiTexture == null)
        {
          Debug.LogWarning ("[DisposableSprite("+gameObject.name+")] Could not find sprite renderer or UITexture");
        }
      }
    }
  }

  void OnDisable()
  {
    RemoveAndSaveSprite ();
  }

  [ContextMenu("Remove and save sprite")]
  public void RemoveAndSaveSprite()
  {
    if (!m_initialized) init ();

    if (DisposeOnDisable && GLResourceManager.Instance != null)
    {
      if (m_spriteRenderer != null && m_spriteRenderer.sprite != null)
      {
        string assetName = m_spriteRenderer.sprite.name;
        if (!GLResourceManager.Instance.AssetExists(assetName))
        {
          Debug.LogWarning("[DisposableSprite] Asset '"+assetName+"' does not exist in resources, canceling dispose.", this);
          return;
        }

        m_resourceName = GLResourceManager.Instance.GetResourceLocation(assetName);
        m_spriteRenderer.sprite = null;
      }
      else if (m_uiTexture != null && m_uiTexture.mainTexture != null)
      {
        string assetName = m_uiTexture.mainTexture.name;
        if (!GLResourceManager.Instance.AssetExists(assetName))
        {
          Debug.LogWarning("[DisposableSprite] Asset '"+assetName+"' does not exist in resources, canceling dispose.", this);
          return;
        }

        m_resourceName = GLResourceManager.Instance.GetResourceLocation(assetName);
        m_uiTexture.mainTexture = null;
      }
    }
  }

  [ContextMenu("Restore sprite")]
  void OnEnable()
  {
    if (!m_initialized) init ();

    if (DisposeOnDisable && !string.IsNullOrEmpty(m_resourceName))
    {
      if (m_glTexture != null)
      {
        m_glTexture.spriteName = m_resourceName;
      }
      else
      {
        if (m_spriteRenderer != null && m_spriteRenderer.sprite == null)
        {
          m_spriteRenderer.sprite = Resources.Load<Sprite>(m_resourceName);
          
          if (m_spriteRenderer.sprite == null)
          {
            Debug.LogError ("[DisposableSprite("+gameObject.name+")] Could not find sprite "+m_resourceName+
                            ". It must be placed in a resources folder.");
          }
        } else if (m_uiTexture != null && m_uiTexture.mainTexture == null)
        {
          m_uiTexture.mainTexture = Resources.Load<Texture>(m_resourceName);
          
          if (m_uiTexture.mainTexture == null)
          {
            Debug.LogError ("[DisposableSprite("+gameObject.name+"] Could not find texture "+m_resourceName+
                            ". It must be placed in a resources folder.");
          }
        }
      }
    }
  }
}