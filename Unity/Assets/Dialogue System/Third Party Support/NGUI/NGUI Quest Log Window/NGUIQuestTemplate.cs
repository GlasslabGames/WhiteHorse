using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.NGUI {

	/// <summary>
	/// This component hooks up the elements of an NGUI quest template.
	/// Add it to your quest template and assign the properties.
	/// </summary>
	public class NGUIQuestTemplate : MonoBehaviour	{

		public UILabel heading;

		public UILabel description;

		public UIButton trackButton;

		public UIButton abandonButton;

		public bool ArePropertiesAssigned {
			get {
				return (heading != null) && (description != null) && 
					(trackButton != null) && (abandonButton != null);
			}
		}

	}

}
