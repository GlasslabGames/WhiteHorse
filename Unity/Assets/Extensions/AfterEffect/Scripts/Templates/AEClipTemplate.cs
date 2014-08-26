using UnityEngine;
using System.Collections;


[System.Serializable]
public class AEClipTemplate  {

	public AfterEffectAnimation anim;
	public string name;
	public AEWrapMode wrapMode;
	public bool defaultClip;
	
	//for editor use only
	public bool IsEditMode = false;
	
}
