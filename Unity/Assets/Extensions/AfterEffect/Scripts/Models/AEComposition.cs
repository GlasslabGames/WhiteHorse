////////////////////////////////////////////////////////////////////////////////
//  
// @module <module_name>
// @author Osipov Stanislav lacost.st@gmail.com
//
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AEComposition : AESprite {
	private bool _isEnabled = true;

	[SerializeField]
	public float opacity;


	[SerializeField]
	private AECompositionTemplate composition;

	[SerializeField]
	private List<AESprite> _sprites = new List<AESprite>();
	
  private static Vector3 VECTOR = new Vector3();

	//--------------------------------------
	// INITIALIZE
	//--------------------------------------

	//--------------------------------------
	//  PUBLIC METHODS
	//--------------------------------------
	 
	public override void WakeUp() {
    for (int i=_sprites.Count-1; i>=0; i--)
    {
			_sprites[i].WakeUp ();
		} 
	}
	

	public override void init(AELayerTemplate layer, AfterEffectAnimation animation,  AELayerBlendingType forcedBlending) {

		base.init (layer, animation, forcedBlending);

		gameObject.name = layer.name + " (Composition)";
	
		composition = animation.animationData.getCompositionById (layer.id);


		InitSprites (animation.IsNGUIAnimation);
		ApplyCompositionFrameForced (0);
	}




	public override void disableRenderer ()
  {
    if(_isEnabled) {
      for (int i=_sprites.Count-1; i>=0;i--)
      {
        _sprites[i].disableRenderer ();
      }

			_isEnabled = false;
		}
	}

	public override void enableRenderer ()
  {
		if(!_isEnabled) {
      for (int i=_sprites.Count-1; i>=0;i--)
      {
        _sprites[i].enableRenderer ();
			}

			_isEnabled = true;
		}
	}

	
  public override void GoToFrameForced (int index) {
		int frameIndex = 0;

    if(index >= _layer.inFrame && index <= _layer.outFrame) {
			frameIndex = index - _layer.inFrame;
      enableRenderer();
		} else {
			disableRenderer ();
			return;
		}
		
		ApplyCompositionFrameForced(frameIndex);
	}
	
	
  public void ApplyCompositionFrameForced(int frameIndex) {
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
      opacity = frame.opacity * 0.01f * parentOpacity;
    }

    // Regardless of whether this composition has a frame, we still want to update the children
    for (int i=_sprites.Count-1; i>=0; i--)
    {
      _sprites[i].GoToFrameForced (frameIndex);
    }
	}
	
	public override void GoToFrame(int index) {

		int frameIndex = 0;

		if(index >= _layer.inFrame && index <= _layer.outFrame) {
      frameIndex = index - _layer.inFrame;
      enableRenderer();
    } else {
      disableRenderer ();
			return;
		}
		
		ApplyCompositionFrame(frameIndex);
	}
	
	public void ApplyCompositionFrame(int frameIndex) {
		AEFrameTemplate frame = _layer.GetKeyframe (frameIndex);
		if(frame != null) {
  		if(frame.IsPositionChanged) {
        transform.localPosition = frame.positionUnity;
  		}
  	
  		if(frame.IsPivotChanged)
      {
        VECTOR.Set(-frame.pivot.x, frame.pivot.y, 0f);

        childAnchor.transform.localPosition = VECTOR;

  			//TODO remove z index calculation
        VECTOR.z = _anim.GetLayerGlobalZ (zIndex, this);
        plane.localPosition = VECTOR;
  		}

  		if(frame.IsRotationChanged)
      {
        transform.localRotation = Quaternion.Euler(0f, 0f, -frame.rotation);
  		}

  		if(frame.IsScaleChanged)
      {
  			transform.localScale = frame.scale;
  		}

      if (frame.IsOpacityChanged)
      {
        opacity = frame.opacity * 0.01f * parentOpacity;
      }
    }

    for (int i=_sprites.Count-1; i>=0; i--)
    {
      _sprites[i].GoToFrame (frameIndex);
    }
	}
	
	public override void SetColor(Color c) {
    for (int i=_sprites.Count; i>=0; i--)
    {
      _sprites[i].SetColor(c);
		} 
	}
	
	//--------------------------------------
	//  GET/SET
	//--------------------------------------


	public List<AESprite> sprites {
		get {
			return _sprites;
		}
	}
	

	//--------------------------------------
	//  EVENTS
	//--------------------------------------
	
	//--------------------------------------
	//  PRIVATE METHODS
	//--------------------------------------

	private void InitSprites(bool isNGUI) {

		_sprites.Clear ();

    List<AELayerTemplate> layers = composition.layers;
    for (int i=layers.Count-1; i>=0; i--)
    {
      AELayerTemplate layer = layers[i];
			AESprite sprite = null;

			layer.forcedBlending = _layer.blending;

			switch(layer.type) {
				case AELayerType.FOOTAGE:
  				sprite = CreateFootage (isNGUI);
  				break;
				case AELayerType.COMPOSITION:
  				sprite = CreateComposition ();
  				break;
				default:
  				Debug.LogError ("Unsupported layer type: " + layer.type.ToString());
  				break;
			}

			sprite.transform.parent = plane;
			sprite.parentIndex = zIndex;
			sprite.indexModifayer = indexModifayer * 0.01f;
      sprite.parentComposition = this;

			if(layer.parent != 0) {
				sprite.layerId = layer.index;
			} else {
				sprite.init (layer, _anim, blending); 
      }
      
      _sprites.Add(sprite);
		}

    _sprites.TrimExcess();

    // TODO does this need to happen outside the loop above? Double loop
    for (int i=layers.Count-1; i>=0; i--)
    {
      AELayerTemplate layer = layers[i];
			if(layer.parent != 0) {
				AESprite p = GetSpriteByLayerId(layer.parent);
				AESprite c = GetSpriteByLayerId (layer.index);
				p.AddChild (c);
				c.init (layer, _anim, blending); 
			}
		}
	}


  public AESprite GetSpriteByLayerId(int layerId) {
    for (int i=_sprites.Count-1; i>=0; i--)
    {
      AESprite sprite = _sprites[i];
			if(sprite.layerId == layerId) {
				return sprite;
			}
		} 

		Debug.LogWarning ("GetSpriteByLayerId  -> sprite not found, layer: " + layerId);
		return null;

	}
	


	protected virtual AEFootage CreateFootage(bool isNGUI) {
		return AEResourceManager.CreateSpriteFootage (isNGUI);
	}

	protected virtual AEComposition CreateComposition() {
		return AEResourceManager.CreateComposition ();
	}

	
	//--------------------------------------
	//  DESTROY
	//--------------------------------------

}
