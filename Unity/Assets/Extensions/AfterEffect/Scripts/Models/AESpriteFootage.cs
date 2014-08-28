using UnityEngine;
using System.Collections;

public class AESpriteFootage : AEFootage {
  private static Vector3 VECTOR = new Vector3();
	
	public override void SetMaterial() {
		string textureName = _anim.imagesFolder + _layer.sourceNoExt;
		Sprite sprite = Resources.Load<Sprite>(textureName);
		
		if(sprite != null) {
			
			switch(blending) {
			case AELayerBlendingType.ADD:
				if (m_spriteRenderer != null)
				{
					m_spriteRenderer.material = new Material (Shader.Find ("Particles/Additive"));
				}
				else if (m_uiTexture != null)
				{
					m_uiTexture.material = new Material (Shader.Find ("Particles/Additive"));
				}
				
				break;
			}
			
			if (m_spriteRenderer != null)
			{
        m_spriteRenderer.sprite = sprite;
        
        w = _layer.width / sprite.bounds.size.x;
        h = _layer.height / sprite.bounds.size.y;
        
        VECTOR.Set(w, h, 1); 
        plane.localScale = VECTOR;
			}
			else if (m_uiTexture != null)
			{
				m_uiTexture.pivot = UIWidget.Pivot.TopLeft;
				m_uiTexture.mainTexture = sprite.texture;
        m_uiTexture.MakePixelPerfect();
        if (sprite.name.EndsWith("_halfSize"))
        {
          m_uiTexture.width *= 2;
          m_uiTexture.height *= 2;
        }
			}
		} else {
			Debug.LogWarning("After Effect: Texture " + textureName + " not found");
		}
		
	}
	
	
	public override Color color {
		get {
			if (m_spriteRenderer != null)
			{
				return m_spriteRenderer.color;
			}
			else if (m_uiTexture != null)
			{
				return m_uiTexture.color;
			}
			else
			{
				Debug.LogWarning("[AESpriteFootage("+name+")] No sprite or texture");
				return Color.white;
			}
		}
		
		set {

      InitComponentReferences();
      if (m_spriteRenderer != null)
			{
        m_spriteRenderer.color = value;
			}
      else if (m_uiTexture != null)
			{
        m_uiTexture.color = value;
			}
			else
			{
				Debug.LogWarning("[AESpriteFootage("+name+")] No sprite or texture");
			}
		}
	}
	
	
	
}

