using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AECompositionTemplate  {

	public int id;

	public float width;
	public float heigh;


	public float duration;
	public int totalFrames;
	public float frameDuration;

	public List<AELayerTemplate> layers =  new List<AELayerTemplate>();

	public void addLayer(AELayerTemplate layer) {
		layers.Add (layer);
	}

	public AELayerTemplate GetLayer(int index) {
		index--;
		return layers [index];
	}
	
}
