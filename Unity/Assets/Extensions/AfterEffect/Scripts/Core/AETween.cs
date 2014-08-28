////////////////////////////////////////////////////////////////////////////////
//  
// @module Affter Effect Importer
// @author Osipov Stanislav lacost.st@gmail.com
//
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public class AETween : MonoBehaviour {

	public delegate void TweenFunction(float val);
	public delegate void TweenCompleteFunction();

	private TweenFunction eventFunction; 
	private TweenCompleteFunction completeFunction = null;
	
	private float _toValue;

	//--------------------------------------
	// INITIALIZE
	//--------------------------------------

	public static AETween  Create(Transform parent) {
		AETween tw =  new GameObject("AETween").AddComponent<AETween>();
		tw.transform.parent = parent;
		return tw;
	}


	//--------------------------------------
	// PUBLIC METHODS
	//--------------------------------------

	void Update() {
		eventFunction(transform.position.x);
	}


	public void MoveTo(float fromValue, float toValue, float time, TweenFunction func) {
		MoveTo(fromValue, toValue, time, func, iTween.EaseType.linear);
	}



	public void MoveTo(float fromValue, float toValue, float time, TweenFunction func, iTween.EaseType ease) {
		Vector3 pos = transform.position;
		pos.x = fromValue;
		transform.position = pos;
		
		_toValue = toValue;

		iTween.MoveTo(gameObject, iTween.Hash("x", toValue,  "time", time, "easeType", ease, "oncomplete", "onTweenComplete", "oncompletetarget", gameObject));

		eventFunction = func;
	}

	//--------------------------------------
	// GET / SET
	//--------------------------------------

	public TweenCompleteFunction OnComplete {
		set {
			completeFunction = value;
		}
	}


	//--------------------------------------
	// PRIVATE METHODS
	//--------------------------------------

	private void onTweenComplete() {
		if(completeFunction != null) {
			completeFunction();
		}
		
		eventFunction(_toValue);

		Destroy(gameObject);
	}


}
