////////////////////////////////////////////////////////////////////////////////
//  
// @module Affter Effect Importer
// @author Osipov Stanislav lacost.st@gmail.com
//
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;


[System.Serializable]
public class AEFootage : AESprite {
	
	protected SpriteRenderer m_spriteRenderer;
	protected UITexture m_uiTexture;

	protected float w;
	protected float h;

	public float opacity = 1f;
	
	[SerializeField]
	private Color materialColor = Color.white;

	private bool _isEnabled = true;
	
  private static Vector3 VECTOR = new Vector3();

	//--------------------------------------
	// INITIALIZE
	//--------------------------------------

	
	public override void WakeUp() {

		if((plane.renderer != null && plane.renderer.sharedMaterial == null) &&
		   (m_uiTexture != null && m_uiTexture.material == null) &&
		   _layer != null) {
			SetMaterial ();
		}
	}

  private bool m_initComponentReferences = false;
  protected void InitComponentReferences()
  {
    if (!m_initComponentReferences)
    {
      m_spriteRenderer = plane.GetComponent<SpriteRenderer>();
      m_uiTexture = plane.GetComponent <UITexture>();
      m_initComponentReferences = true;
    }
  }

	public override void init(AELayerTemplate layer, AfterEffectAnimation animation,  AELayerBlendingType forcedBlending) {
		
    InitComponentReferences();

    	materialColor = Color.white; // Fix for blacked items

		base.init (layer, animation, forcedBlending);


		gameObject.name = layer.name + " (Footage)";
		SetMaterial ();
		
		color = _anim.MaterialColor;
		
		GoToFrameForced (0);

		AESpriteRenderer r = plane.gameObject.AddComponent<AESpriteRenderer> ();
		r.anim = _anim;
		r.enabled = false;

	}
	

	//--------------------------------------
	// PUBLIC METHODS
	//--------------------------------------
	

	public override void disableRenderer () {
    if(_isEnabled) {
      InitComponentReferences();
			if (m_spriteRenderer != null)
			{
				m_spriteRenderer.enabled = false;
			}
      else if (m_uiTexture != null)
			{
        m_uiTexture.enabled = false;
			}
			else
			{
				Debug.LogWarning ("[AEFootage("+name+")] No texture or renderer to disable");
      }

			_isEnabled = false;
		}

	}

	public override void enableRenderer () {
    if(!_isEnabled) {
      InitComponentReferences();
			if (m_spriteRenderer != null)
			{
        m_spriteRenderer.enabled = true;
			}
			else if (m_uiTexture != null)
			{
        m_uiTexture.enabled = true;
			}
			else
			{
				Debug.LogWarning ("[AEFootage("+name+")] No texture or renderer to disable");
			}
			_isEnabled = true;
		}

	}

  // TODO STILL BUGGY
  public override void GoToFrameForced (int frameIndex) {
    if(frameIndex < _layer.inFrame || frameIndex > _layer.outFrame) {
			disableRenderer ();
			//return;
		} else {
			enableRenderer ();
		}
    
    AEFrameTemplate frame = _layer.GetFrame (frameIndex);
    if(frame != null)
    {
      /**
       * POSITION
       */
      transform.localPosition = frame.positionUnity;
      
      /**
       * PIVOT
       */
      VECTOR.Set(-frame.pivot.x, frame.pivot.y, 0f);

      childAnchor.transform.localPosition = VECTOR;
      
      //TODO remove z index caclulcation
      VECTOR.z = _anim.GetLayerGlobalZ (zIndex, this);
      plane.localPosition = VECTOR;
      
      /**
       * ROTATION
       */
      VECTOR.Set(0f, 0f, -frame.rotation);
      transform.localRotation = Quaternion.Euler(VECTOR);
      
      /**
       * SCALE
       */
      transform.localScale = frame.scale;
      
      /**
       * OPACITY
       */
      SetOpacity(frame.opacity * 0.01f * parentOpacity);
    }
	}
	
	
	public override void GoToFrame(int index) {
		if(index < _layer.inFrame || index > _layer.outFrame) {
			disableRenderer ();
			//return;
		} else {
			enableRenderer ();
		}
		
    AEFrameTemplate frame = _layer.GetKeyframe (index);
    if(frame == null) {
			return;
		}

		if(frame.IsPositionChanged) {
      transform.localPosition = frame.positionUnity;
		}

		if(frame.IsPivotChanged) {
      VECTOR.Set(-frame.pivot.x, frame.pivot.y, 0f);

      childAnchor.transform.localPosition = VECTOR;

			VECTOR.z = _anim.GetLayerGlobalZ (zIndex, this);
			plane.localPosition = VECTOR;
			if (m_uiTexture != null) {
				//m_uiTexture.depth = (int) -pos.z;
				m_uiTexture.transform.localScale = Vector3.one;
      }
		}
	
		if(frame.IsRotationChanged) {
      VECTOR.Set(0f, 0f, -frame.rotation);
      transform.localRotation = Quaternion.Euler(VECTOR);
		}

		if(frame.IsScaleChanged) {
			transform.localScale = frame.scale;
		}

    if (frame.IsOpacityChanged)
    {
      SetOpacity(parentOpacity * frame.opacity * 0.01f * _anim.opacity);
    }
	}


	public virtual void SetOpacity(float op) {
		if(opacity != op) {
			opacity = op;

			materialColor.a = opacity;
			color = materialColor;
		} 
	}

	public override void SetColor(Color c) {
		materialColor = c;
		float a = color.a;
		c.a = a;
		color = c;
	}
	

	public virtual void SetMaterial() {
		string textureName = _anim.imagesFolder + _layer.sourceNoExt;
		Texture2D tex = Resources.Load (textureName) as Texture2D;

		if(tex != null) {
			plane.renderer.sharedMaterial = new Material (shader);
			plane.renderer.sharedMaterial.SetTexture("_MainTex", tex);
			
			plane.renderer.sharedMaterial.name = tex.name;
	
			w = _layer.width;
			h = _layer.height;
	
      VECTOR.Set(w, h, 1); 
      plane.localScale = VECTOR;
		} else {
			Debug.LogWarning("After Effect: Texture " + textureName + " not found");
		}

	}



	//--------------------------------------
	// GET / SET
	//--------------------------------------



	public virtual Color color {
		get {
      InitComponentReferences();
			Material m = m_spriteRenderer.sharedMaterial;
			if(m.HasProperty("_Color")) {
				return m.color;
			} else {
				if(m.HasProperty("_TintColor")) {
					return m.GetColor ("_TintColor");
				} else {
					return Color.white;
				}
			}
		}
    set {
			if(plane.renderer.sharedMaterial.HasProperty("_Color")) {
				plane.renderer.sharedMaterial.color = value;
			}  else {
				if(plane.renderer.sharedMaterial.HasProperty ("_TintColor")) {
					plane.renderer.sharedMaterial.SetColor ("_TintColor", value);
				}

			}
		}
	}

	public Shader shader {
		get {
			Shader sh;

			switch(blending) {
				case AELayerBlendingType.ADD:
				sh = _anim.GetAddShader ();
				break;
				default:
				sh = _anim.GetNormalShader ();
				break;
			}

			return sh;
		}
	}
}
