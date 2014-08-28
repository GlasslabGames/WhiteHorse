////////////////////////////////////////////////////////////////////////////////
//  
// @module <module_name>
// @author Osipov Stanislav lacost.st@gmail.com
//
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;


[System.Serializable]
public abstract class AESprite : MonoBehaviour {

  public int layerId {
    get {
      return m_layerID;
    }
    set {
      m_layerID = value;

      UITexture texture = plane.GetComponent<UITexture>();
      if (texture != null) {
        texture.depth = (int) -m_layerID;
        texture.transform.localScale = Vector3.one;
      }
    }
  }
	private int m_layerID;
	public float zIndex;
	public float parentIndex = 0;
	public float indexModifayer = 1f;


	[SerializeField]
	public Transform plane;


	[SerializeField]
	protected AfterEffectAnimation _anim;

	[SerializeField]
	protected GameObject _childAnchor = null;

	[SerializeField]
	public AEComposition parentComposition = null;


	[SerializeField]
	public AELayerBlendingType blending = AELayerBlendingType.NORMAL;
	

	[SerializeField]
	protected AELayerTemplate _layer;


	//--------------------------------------
	// INITIALIZE
	//--------------------------------------
	
	public abstract void WakeUp();
	


	public virtual void init(AELayerTemplate layer, AfterEffectAnimation animation) {
		init (layer, animation, AELayerBlendingType.NORMAL);
	}

	public virtual void init(AELayerTemplate layer, AfterEffectAnimation animation, AELayerBlendingType forcedBlending) {
		_layer = layer;
		_anim = animation;
    layerId = layer.index;

		zIndex = parentIndex + (layer.index) * indexModifayer;

		if(forcedBlending == AELayerBlendingType.NORMAL) {
			blending = _layer.blending;
		} else {
			blending = forcedBlending;
		}

	}

	public abstract void GoToFrame (int index);
	public abstract void GoToFrameForced (int index);
	public abstract void disableRenderer ();
	public abstract void enableRenderer ();
	public abstract void SetColor(Color c);
	
	//--------------------------------------
	//  PUBLIC METHODS
	//--------------------------------------

	public void AddChild(AESprite sprite) {
    sprite.transform.parent = childAnchor.transform;
    sprite.transform.localScale = Vector3.one;
    sprite.gameObject.layer = childAnchor.layer;
	}
	

	//--------------------------------------
	//  GET/SET
	//--------------------------------------


	public float parentOpacity {
		get {
			if(parentComposition != null) {
				return parentComposition.opacity;
			} else {
				return 1f;
			}
		}
	}


	public GameObject childAnchor {
		get {
			if(_childAnchor == null) {
				_childAnchor = new GameObject ("ChildAnchor");
				_childAnchor.transform.parent = gameObject.transform;
				_childAnchor.transform.localPosition = plane.localPosition;
        _childAnchor.transform.localScale = Vector3.one;
			}

			return _childAnchor;

		}
	}

	
	//--------------------------------------
	//  EVENTS
	//--------------------------------------
	
	//--------------------------------------
	//  PRIVATE METHODS
	//--------------------------------------
	
	//--------------------------------------
	//  DESTROY
	//--------------------------------------

}
