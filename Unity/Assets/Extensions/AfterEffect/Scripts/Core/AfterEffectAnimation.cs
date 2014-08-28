////////////////////////////////////////////////////////////////////////////////
//  
// @module Affter Effect Importer
// @author Osipov Stanislav lacost.st@gmail.com
// Modified by Jerry Fu @ GlassLabGames
//
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[ExecuteInEditMode]
[System.Serializable]
public class AfterEffectAnimation : EventDispatcher {
	public const string ANIMATION_COMPLETE = "animation_complete";
	public const string FADE_COMPLETE = "fade_complete";
	public const string ENTER_FRAME = "enter_frame";

	public TextAsset dataFile;
  public string dataFileName; // Purely used for debug right now to release asset
  public bool IsNGUIAnimation = false;
	public string imagesFolder = "";
	public Color GizmosColor = Color.green;
	public Color MaterialColor = Color.white;

	public int currentFrame = 0;
  public int previousFrame = 0;
  private float currentFrameAsFloat = 0f;
	public bool PlayOnStart = true;
	public bool Loop = true;
	public bool PlayBack = true;
	public bool PlayBackwards = false;

  public bool IsForceSelected = false;
	public bool CPUOptimization = true;

	public float opacity = 1f;


	public float pivotCenterX = 0f;
	public float pivotCenterY = 0f;

	[SerializeField]
	public Transform lastParent;
	
	[SerializeField]
	public AESettingsMode mode = AESettingsMode.Simple;

	public int normal_mode_shader = 1;
	public int add_mode_shader = 4;

	
	[SerializeField]
	private AEAnimationData _animationData = null;
	
	[SerializeField]
	private GameObject _spritesHolder = null;

	[SerializeField]
	private float _timeScale = 25;

	[SerializeField]
	private float _frameDuration = 0.04f;
  
  private bool m_initOnUpdate = false;
  private bool m_playOnUpdate = false;


	private bool _isPlaying = false;

	[SerializeField]
	private List<AESprite> _sprites = new List<AESprite>();

  private Coroutine _dataLoadingCoroutine;

  public event Action onFrame;
  public event Action onComplete;

  // Event for data loading complete to handle attempts to play frames while loading.
  private event Action gotoFrameOnDataLoadComplete; // WARNING: ONLY ONE ACTION SHOULD EVER BE ASSIGNED to avoid confusion on what will play when load is complete
  public event Action onDataLoadCompleteOnceOnly;

  public bool IsLoadingData { get { return _dataLoadingCoroutine != null; } }
  private static Vector3 VECTOR = new Vector3();

	//--------------------------------------
	// INITIALIZE
	//--------------------------------------
	
	
  void Awake() {
    for (int i=_sprites.Count-1; i>=0; i--){
			_sprites[i].WakeUp();
		}
	}

	void Start() {
		if(Application.isPlaying) {
			if(PlayOnStart) {
				Play ();
			}
		}
	}


	//--------------------------------------
	// PUBLIC METHODS
	//--------------------------------------


	public void Play() {
    if (_animationData == null) {
      if (_dataLoadingCoroutine != null)
      {
        m_playOnUpdate = true;
      }
			return;
		}

		if(!_isPlaying) {
			//PlayQueue ();
			_isPlaying = true;
		}
    
    m_playOnUpdate = false;

	}

	public void Stop() {
		if(_isPlaying) {
			//CancelInvoke (PLAY_QUEUE_STRING);
			_isPlaying = false;
		}
	}

  public void GoToAndStop(int index) {
    if (_animationData == null && _dataLoadingCoroutine != null)
    {
      // Replace goto delegate
      Debug.LogWarning("No data loaded when told to GoToAndStop("+index+"), doing that when data finishes loading...", this);
      gotoFrameOnDataLoadComplete = delegate() {
        GoToAndStop(index);
      };
      return;
    }
		Stop ();
		currentFrame = index;
    currentFrameAsFloat = currentFrame;
    previousFrame = index;
		GoToCurrentFrameForced ();
	}

	public void GoToAndPlay(int index) {
    if (_animationData == null && _dataLoadingCoroutine != null)
    {
      // Replace goto delegate
      Debug.LogWarning("No data loaded when told to GoToAndPlay("+index+"), doing that when data finishes loading...", this);
      gotoFrameOnDataLoadComplete = delegate() {
        GoToAndPlay(index);
      };
      return;
    }
    currentFrame = index;
    currentFrameAsFloat = currentFrame;
    previousFrame = index;
		GoToCurrentFrameForced ();
		Play ();
	}

  void OnDisable()
  {
    // Coroutines are stopped, so clear the events
    gotoFrameOnDataLoadComplete = null;
    onDataLoadCompleteOnceOnly = null;
  }
  /*
  private const string PLAY_QUEUE_STRING = "PlayQueue";
	private void PlayQueue() {
    return;
		if (_dataLoadingCoroutine == null) GoToCurrentFrame ();
		//dispatch(ENTER_FRAME, currentFrame); // Using delegates, less memory allocation and trash
    if (onFrame != null) onFrame();

		if(PlayBackwards) {
			currentFrame--;
		} else {
			currentFrame++;
		}

		if(PlayBackwards) {
			if(currentFrame >= 0) {
				Invoke(PLAY_QUEUE_STRING, _frameDuration);
			} else {
				if(Loop) {
					currentFrame = lastFrame;
					Invoke(PLAY_QUEUE_STRING, _frameDuration);
				} else {
					currentFrame = 0;
					_isPlaying = false;
          if (onComplete != null) onComplete();
				}
			}
		} else {
			if(currentFrame < totalFrames) {
				Invoke(PLAY_QUEUE_STRING, _frameDuration);
			} else {
				if(Loop) {
					currentFrame = 0;
					Invoke(PLAY_QUEUE_STRING, _frameDuration);
				} else {
					currentFrame = lastFrame;
          _isPlaying = false;
          if (onComplete != null) onComplete();
				}
			}
		}


	}
  */ 

  public void GoToCurrentFrameForced() {
    for (int i=_sprites.Count-1; i>=0; i--){
			_sprites[i].GoToFrameForced (currentFrame);
		} 
	}
	
	public void GoToCurrentFrame() {
		if(!CPUOptimization) {
			GoToCurrentFrameForced();
			return;
		}
		
    //foreach(AESprite sprite in _sprites) {
    for (int i=_sprites.Count-1; i>=0; i--){
			_sprites[i].GoToFrame (currentFrame);
		}
	}

  public void GoToFrame(int frameNum, int fromFrame)
  {
    frameNum = Mathf.Clamp(frameNum, 0, lastFrame);

    // Backwards
    if (frameNum < fromFrame)
    {
      for (int i=_sprites.Count-1; i>=0; i--){
        _sprites[i].GoToFrameForced (frameNum);
      }
    }
    else
    {
      // Forwards
      for (int frameIndex = fromFrame+1; frameIndex <= frameNum; frameIndex++)
      {
        for (int i=_sprites.Count-1; i>=0; i--){
          _sprites[i].GoToFrame (frameIndex);
        }
      }
    }
  }


  public AESprite GetSpriteByLayerId(int layerId) {
    for (int i=_sprites.Count-1; i>=0; i--){
      AESprite sprite = _sprites[i];
			if(sprite.layerId == layerId) {
				return sprite;
			}
		}

		Debug.LogWarning ("GetSpriteByLayerId  -> sprite not found, layer: " + layerId);
		return null;
	}

	public Shader GetNormalShader() {
		return AEShaders.shaders [normal_mode_shader];
	}

	public Shader GetAddShader() {
		return AEShaders.shaders [add_mode_shader];
	}


	
	public void AnimateOpacity(float valueFrom, float valueTo, float time) {
		AETween tw = AETween.Create(transform);
		tw.MoveTo(valueFrom, valueTo, time, OnOpacityAnimationEvent);
		tw.OnComplete = OnFadeComplete;
	}
	
	private void OnFadeComplete() {
		dispatch(FADE_COMPLETE);
	}


	//--------------------------------------
	// EVENTS
	//--------------------------------------

	void  OnDrawGizmosSelected () {
		if(_animationData != null) {
			Gizmos.color = GizmosColor;

			Vector3 pos = Vector3.zero;

			pos.x += width / 2f;
			pos.y -= height / 2f;


			pos.x -= width * pivotCenterX;
			pos.y += height * pivotCenterY;

			Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

			Gizmos.matrix = rotationMatrix; 

      VECTOR.Set(width, height, 0.01f);
      Gizmos.DrawWireCube (pos, VECTOR);



		} else {
			if(dataFile != null) {
				OnAnimationDataChange ();
			}
		}
	}

	public virtual void UpdateColor() {
		SetColor(MaterialColor);
	}
	
	public virtual void SetColor(Color c) {
    for (int i=_sprites.Count-1; i>=0; i--){
			_sprites[i].SetColor(c);
		}
	}

  public virtual void AsyncOnAnimationDataChange() {
    if (dataFile == null && !string.IsNullOrEmpty(dataFileName))
    {
      dataFile = Resources.Load<TextAsset>(dataFileName);
    }

    if(dataFile != null && _dataLoadingCoroutine == null)
    {
      _dataLoadingCoroutine = GLResourceManager.InstanceOrCreate.StartCoroutine(AEDataParcer.asyncParceAnimationData (dataFile.text, delegate(AEAnimationData data) {
        if (this == null) // This can happen since the controller can be destroyed before the coroutine completes.
        {
          return;
        }

        if (dataFile == null)
        {
          Debug.LogError("Data file was null upon returning from parcing animation data", this);
          return;
        }

        _animationData = data;

        if (gameObject.activeInHierarchy)
        {
          initOnUpdate();
        }
        else
        {
          InitSprites();
        }
        dataFileName = dataFile.name;
        dataFile = null;
        _dataLoadingCoroutine = null;

        if (gotoFrameOnDataLoadComplete != null) gotoFrameOnDataLoadComplete();
        if (onDataLoadCompleteOnceOnly != null)
        {
          Action temp = onDataLoadCompleteOnceOnly;
          onDataLoadCompleteOnceOnly = null; // Null it first in case someone wants to do anything in the callback
          temp();
        }
     }));
    }
    
    timeScale = 1f;
    OnPivotPositionChnage ();
    SetColor(MaterialColor);
    
    int f = currentFrame;
    currentFrame = 0;
    OnEditorFrameChange();
    currentFrame = f;
    OnEditorFrameChange();
  }

  public virtual void OnAnimationDataChange() {
    if(dataFile != null)
    {
      if (_dataLoadingCoroutine != null) Debug.LogError("A loading coroutine was running when OnAnimationDataChange was called.", this);
      _animationData = AEDataParcer.ParceAnimationData (dataFile.text);
      dataFileName = dataFile.name;
      dataFile = null;
      initOnUpdate();
		}

		timeScale = 1f;
		OnPivotPositionChnage ();
		SetColor(MaterialColor);
		
		int f = currentFrame;
		currentFrame = 0;
		OnEditorFrameChange();
		currentFrame = f;
		OnEditorFrameChange();
	}

	public void OnEditorFrameChange() {
		GoToCurrentFrameForced();

		OnPivotPositionChnage ();
	}

	public void OnPivotPositionChnage() {
		if(_animationData != null) {
			Vector3 pos = Vector3.zero;
			pos.x = -width * pivotCenterX;
			pos.y = height * pivotCenterY;
			spritesHolder.transform.localPosition = pos;
		}

	}


	private void OnOpacityAnimationEvent(float val) {
		opacity = val;
		GoToCurrentFrameForced();
	}




	//--------------------------------------
	// GET / SET
	//--------------------------------------

	public bool isPlaying {
		get {
			return _isPlaying;
		}
	}

	public float width {
		get {
			return _animationData.composition.width;
		}
	}

	public float height {
		get {
			return _animationData.composition.heigh;
		}
	}

	public AECompositionTemplate composition {
		get {
			return _animationData.composition;
		}
	}

	public int totalFrames {
		get {
			if(_animationData != null) {
				return _animationData.totalFrames;
			} else {
				return 0;
			}
		}
	}

	public int lastFrame {
		get {
			return Math.Max(totalFrames - 1, 0);
		}
	}

	public AEAnimationData  animationData {
		get {
			return _animationData;
		}
	}

	public GameObject spritesHolder {
		get {
			if(_spritesHolder == null) {
				_spritesHolder = new GameObject ("SpritesHolder");
				_spritesHolder.transform.parent = transform;
				_spritesHolder.transform.localScale = Vector3.one;
				_spritesHolder.transform.localPosition = Vector3.zero;
				_spritesHolder.transform.localRotation = Quaternion.identity;
        _spritesHolder.gameObject.layer = gameObject.layer;
			}
			return _spritesHolder;
		}
	}
	

	public float GetLayerGlobalZ(float index, AESprite sp) {
    return index * AEConf.ZPosPadding + (transform.position.z - sp.transform.parent.position.z);
	}

	
	public float timeScale  {
		get {
			return _timeScale;
		}

		set {
			_timeScale = value;
			if(_animationData != null) {
				_frameDuration = _animationData.frameDuration / _timeScale;
			}

		}
	}

	public float frameDuration {
		get {
			return _frameDuration;
		}
	}
	
	public float duration {
		get {
			return animationData.duration / timeScale;
		}
	}
	
  private void initOnUpdate()
  {
    m_initOnUpdate = true;
  }

  void Update()
  {
    if (m_initOnUpdate)
    {
      InitSprites();
      m_initOnUpdate = false;
    }

    if (m_playOnUpdate)
    {
      Play();
    }

    // Update frame
    if (!_isPlaying) // This is set in Play() which has checks
    {
      return;
    }
    
    float deltaFrame = Time.deltaTime/_frameDuration;
    if(PlayBackwards) {
      currentFrameAsFloat -= deltaFrame;
    } else {
      currentFrameAsFloat += deltaFrame;
    }


    // TODO: Frame counter doesn't keep track of frames skipped when looping
    previousFrame = currentFrame;
    currentFrame = (int) currentFrameAsFloat;
    // Skip update if currentFrame is the same as previous frame
    if (currentFrame == previousFrame)
    {
      return;
    }
    
    if (_dataLoadingCoroutine == null) GoToFrame(currentFrame, previousFrame);
    //dispatch(ENTER_FRAME, currentFrame); // Using delegates, less memory allocation and trash
    if (onFrame != null) onFrame();
    
    if(PlayBackwards)
    {
      if(currentFrame < 0)
      {
        if(Loop) {
          currentFrame = lastFrame;
          previousFrame = currentFrame;
          currentFrameAsFloat += lastFrame;
        }
        else
        {
          currentFrame = 0;
          previousFrame = currentFrame;
          currentFrameAsFloat -= lastFrame;
          _isPlaying = false;
          if (onComplete != null) onComplete();
        }
      }
    }
    else if(currentFrame >= totalFrames)
    {
      if(Loop) {
        currentFrame = 0;
        previousFrame = currentFrame;
        currentFrameAsFloat -= totalFrames;
      } else {
        currentFrame = lastFrame;
        previousFrame = currentFrame;
        currentFrameAsFloat += totalFrames;
        _isPlaying = false;
        if (onComplete != null) onComplete();
      }
    }
  }


	//--------------------------------------
	// PRIVATE METHODS
	//--------------------------------------
  
  private void InitSprites() {
    
    _sprites.Clear ();
    List<Transform> _childs = new List<Transform> ();
    foreach(Transform child in transform) {
      _childs.Add (child);
    }
    
    foreach(Transform c in _childs) {
      DestroyImmediate (c.gameObject);
    }
    
    foreach(AELayerTemplate layer in _animationData.composition.layers) {
      AESprite sprite = null;
      
      
      switch(layer.type) {
        case AELayerType.FOOTAGE:
          sprite = CreateFootage ();
          break;
        case AELayerType.COMPOSITION:
          sprite = CreateComposition ();
          break;
        default:
          Debug.LogError ("Unsupported layer type: " + layer.type.ToString());
          break;
          
      }
      
      _sprites.Add(sprite);
      
      sprite.transform.parent = spritesHolder.transform;
      sprite.transform.localScale = Vector3.one;
      sprite.gameObject.layer = spritesHolder.layer;
      
      if(layer.parent != 0) {
        sprite.layerId = layer.index;
      } else {
        sprite.init (layer, this);
      }
    }

    _sprites.TrimExcess();

    foreach(AELayerTemplate layer in _animationData.composition.layers) {
      if(layer.parent != 0) {
        AESprite p = GetSpriteByLayerId(layer.parent);
        AESprite c = GetSpriteByLayerId (layer.index);
        p.AddChild (c);
        c.init (layer, this);
      }
    } 
    
    OnEditorFrameChange ();
    
  }

  private IEnumerator doInitSprites() {
    float startTime = Time.realtimeSinceStartup;
    _sprites.Clear ();
    List<Transform> _childs = new List<Transform> ();
    foreach(Transform child in transform) {
      _childs.Add (child);
    } 

    if (Time.realtimeSinceStartup - startTime >= .01f) {
      yield return null;
      startTime = Time.realtimeSinceStartup;
    }

    foreach(Transform c in _childs) {
      DestroyImmediate (c.gameObject);
    }
    _childs = null;
    
    if (Time.realtimeSinceStartup - startTime >= .01f) {
      yield return null;
      startTime = Time.realtimeSinceStartup;
    }
    
    foreach(AELayerTemplate layer in _animationData.composition.layers) {
      AESprite sprite = null;
      
      
      switch(layer.type) {
        case AELayerType.FOOTAGE:
          sprite = CreateFootage ();
          break;
        case AELayerType.COMPOSITION:
          sprite = CreateComposition ();
          break;
        default:
          Debug.LogError ("Unsupported layer type: " + layer.type.ToString());
          break;
          
      }
      
      _sprites.Add(sprite);
      
      sprite.transform.parent = spritesHolder.transform;
      sprite.transform.localScale = Vector3.one;
      sprite.gameObject.layer = spritesHolder.layer;
      
      if(layer.parent != 0) {
        sprite.layerId = layer.index;
      } else {
        sprite.init (layer, this);
      }
      
      if (Time.realtimeSinceStartup - startTime >= .01f) {
        yield return null;
        startTime = Time.realtimeSinceStartup;
      }
    } 
    
    _sprites.TrimExcess();
    
    foreach(AELayerTemplate layer in _animationData.composition.layers) {
      if(layer.parent != 0) {
        AESprite p = GetSpriteByLayerId(layer.parent);
        AESprite c = GetSpriteByLayerId (layer.index);
        p.AddChild (c);
        c.init (layer, this);
      }
      
      if (Time.realtimeSinceStartup - startTime >= .01f) {
        yield return null;
        startTime = Time.realtimeSinceStartup;
      }
    }

    OnEditorFrameChange ();
  }

	protected virtual AEFootage CreateFootage() {
    return AEResourceManager.CreateSpriteFootage (IsNGUIAnimation);
	}

	protected virtual AEComposition CreateComposition() {
		return AEResourceManager.CreateComposition ();
	}



	protected Vector3 GetWorldScale() {

		Vector3 worldScale = transform.localScale;
		Transform parent = transform.parent;


		while (parent != null) {
			worldScale = Vector3.Scale(worldScale,parent.localScale);
			parent = parent.parent;
		}
		return worldScale;
	}

	/*
	private void CheckInit() {
		if(!isInited) {
			foreach(AESprite sprite in _sprites) {
				sprite.initInPlayMode (composition.GetLayer (sprite.layerId), this);
			} 

			isInited = true;
		}
	}
	*/
}
