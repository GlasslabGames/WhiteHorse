////////////////////////////////////////////////////////////////////////////////
//  
// @module Affter Effect Importer
// @author Osipov Stanislav lacost.st@gmail.com
//
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AEAnimationData  {

	public AECompositionTemplate composition;

	public List<AECompositionTemplate> usedComposition =  new List<AECompositionTemplate>();

	public float duration;
	public int totalFrames;
	public float frameDuration;




	public void addComposition(AECompositionTemplate c) {
		usedComposition.Add (c);
	}

	public AECompositionTemplate getCompositionById(int id) {
		foreach(AECompositionTemplate tpl in usedComposition) {
			if(tpl.id == id) {
				return tpl;
			}
		}

		return null;
	}

}
