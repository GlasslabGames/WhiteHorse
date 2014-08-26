using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class AEAnimationController : EventDispatcher {
	
	[SerializeField]
	public bool PlayOnStart;
	
	[SerializeField]
	public List<AEClipTemplate> _clips;
	
	[SerializeField]
	public float opacity = 1f;
	
	[SerializeField]
	public AEClipTemplate currentClip;
	
	private bool IsFading = false;
	
	
	void Awake() {
		if(PlayOnStart) {
			if(currentClip != null) {
				currentClip.anim.Play();
			}
		}
	}
	
	
	//--------------------------------------
	// PUBLIC METHODS
	//--------------------------------------
	
	
	public bool RegisterClip(AfterEffectAnimation anim) {
		
		bool IsRegisterd = false;

    for(int i=clips.Count-1; i>=0; i--)
    {
      AEClipTemplate clip = clips[i];
      if(clip.anim == anim) {
        IsRegisterd = true;
        break;
      }
    }

		if(!IsRegisterd) {
			AEClipTemplate tpl =  new AEClipTemplate();
			tpl.anim = anim;
			tpl.name = anim.dataFile.name;
			anim.transform.localPosition = Vector3.zero;
			
			if(anim.Loop) {
				tpl.wrapMode = AEWrapMode.Loop;
			} else {
				tpl.wrapMode = AEWrapMode.Once;
			}
			
			anim.PlayOnStart = false;
			clips.Add(tpl);
			
			return true;
		} else {
			return false;
		}
		
	}
	
	public void CrossFade(string clip, float time) {
		
		if(IsFading) {
			return;
		}
		
		
		AEClipTemplate c = GetClipByName(clip);
		if(c != null) {
			
			
			if(!c.name.Equals(currentClip.name)) {
				IsFading = true;
				currentClip.anim.addEventListener(AfterEffectAnimation.FADE_COMPLETE, OnAnimationFadeComplete);
				currentClip.anim.AnimateOpacity(currentClip.anim.opacity, 0f, time);
				
				
				c.anim.gameObject.SetActive(true);
				
				if(c.wrapMode == AEWrapMode.Loop) {
					c.anim.Loop = true;
				} else {
					c.anim.Loop = false;
				}
				
				c.anim.opacity = 0f;
				c.anim.gameObject.SetActive(true);
				c.anim.AnimateOpacity(0f, 1f, time);
				c.anim.GoToAndPlay(0);
				
				currentClip = c;
				
				
			} 
			
			
		}
	}
	
	private void OnAnimationFadeComplete(CEvent e) {
		e.dispatcher.removeEventListener(AfterEffectAnimation.FADE_COMPLETE, OnAnimationFadeComplete);
		(e.dispatcher as AfterEffectAnimation).gameObject.SetActive(false);
		
		IsFading = false;
	}
	
	public void SetClipName(string name, AfterEffectAnimation anim) {
		foreach(AEClipTemplate clip in clips) {
			if(clip.anim == anim) {
				clip.name = name;
			}
		}
	}
	
	public AEClipTemplate GetClipByName(string name) {
		foreach(AEClipTemplate clip in clips) {
			if(clip.name == name) {
				return clip;
			}
		}
		
		return null;
	}
	
	public void SetDefaultClip(AEClipTemplate tpl) {
		foreach(AEClipTemplate clip in clips) {
			clip.defaultClip = false;
		}
		
		tpl.defaultClip = true;
	}
	
	public void ClearClips() {
		clips.Clear();
	}
	
	public void CleanUpLayers() {
		foreach(AEClipTemplate clip in clips.ToArray()) {
			if(clip.anim.transform.parent != transform) {
				clips.Remove(clip);
			}
		}
		
		if(clips.Count == 0) {
			currentClip = null;
			return;
		}
		
		bool hasDefault = false;
		foreach(AEClipTemplate clip in clips) {
			if(clip.defaultClip) {
				hasDefault = true;
			}
		}
		
		if(!hasDefault) {
			clips[0].defaultClip = true;
		}
		
		foreach(AEClipTemplate clip in clips) {
			clip.anim.PlayOnStart = false;
			if(!clip.defaultClip) {
				clip.anim.gameObject.SetActive(false);
			} else {
				currentClip = clip;
				clip.anim.gameObject.SetActive(true);
			}
		}
	}
	
	
	//--------------------------------------
	// GET / SET
	//--------------------------------------
	
	public List<AEClipTemplate> clips {
		get {
			if(_clips == null) {
				_clips =  new List<AEClipTemplate>();
			}
			return _clips;
		}
	}
	
	public AEClipTemplate clip {
		get {
			return currentClip;
		}
	}
	
	
	
}
