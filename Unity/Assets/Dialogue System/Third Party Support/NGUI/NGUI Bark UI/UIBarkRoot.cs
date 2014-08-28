using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.NGUI {
	
	/// <summary>
	/// To use NGUI barks, you must add one copy of this component to a game object in an NGUI UI.
	/// All barking NPCs will add their NGUI labels as children of this object.
	/// </summary>
	[AddComponentMenu("Dialogue System/Third Party/NGUI/Bark Root")]
	public class UIBarkRoot : MonoBehaviour {
		
		public static GameObject rootObject = null;
		
		public bool dontDestroyUIRootOnLoad = false;
		
		public void Awake() {
			rootObject = gameObject;
			if (dontDestroyUIRootOnLoad) {
				Transform t = transform;
				UIRoot uiRoot = null;
				while ((uiRoot == null) && (t != null)) {
					uiRoot = t.GetComponent<UIRoot>();
					t = t.parent;
				}
				if (uiRoot != null) DontDestroyOnLoad(uiRoot.gameObject);
			}
		}
		
	}

}
