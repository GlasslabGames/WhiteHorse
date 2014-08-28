using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.NGUI {
	
	/// <summary>
	/// Keeps a game object positioned over the "head" of another game object. NGUIBarkUI uses it
	/// to keep bark text above the NPC's head.
	/// </summary>
	[AddComponentMenu("Dialogue System/Third Party/NGUI/Follow Target")]
	public class NGUIFollowTarget : MonoBehaviour {

		/// <summary>
		/// The target to follow.
		/// </summary>
		public Transform target;
		
		/// <summary>
		/// If the NPC doesn't have a CharacterController or CapsuleCollider, or if you want to
		/// override its height, set this above zero.
		/// </summary>
		public float overrideHeight = 0;
	
		/// <summary>
		/// Gets the world space position of the bark.
		/// </summary>
		/// <value>The bark position.</value>
		public Vector3 BarkPosition { get; private set; }
		
		/// <summary>
		/// Indicates whether the bark is visible based on the camera's heading.
		/// </summary>
		/// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
		public bool IsVisible { get; private set; }
		
		/// <summary>
		/// The offset to the character's head from the origin of the target.
		/// </summary>
		private Vector3 offsetToHead = Vector3.zero;
		
		private Camera uiCamera;
		private Camera gameplayCamera;
		private Transform myTransform;
	
		public void Awake() {
			myTransform = transform; 
		}
	
		/// <summary>
		/// Start by computing the offset to the head and finding the cameras to use when 
		/// positioning the game object.
		/// </summary>
		void Start() {
			if (target != null) {
				FindCameras();
				ComputeOffsetToHead();
			} else {
				if (DialogueDebug.LogErrors) Debug.LogError(string.Format("{0}: {1} doesn't have a valid target to follow.", DialogueDebug.Prefix, gameObject.name));
				enabled = false;
			}
		}
		
		/// <summary>
		/// Finds the UI and gameplay cameras.
		/// </summary>
		private void FindCameras() {
			uiCamera = NGUITools.FindCameraForLayer(gameObject.layer);
			gameplayCamera = NGUITools.FindCameraForLayer(target.gameObject.layer);
		}
		
		/// <summary>
		/// Computes the offset to the character's head, based on the top of the 
		/// CharacterController or CapsuleCollider. Use overrideHeight if greater than 0.
		/// </summary>
		private void ComputeOffsetToHead() {
			if (overrideHeight > 0) {
				offsetToHead = new Vector3(0, overrideHeight, 0);
			} else {
				CharacterController controller = target.GetComponent<CharacterController>();
				if (controller != null) {
					offsetToHead = new Vector3(0, controller.height, 0);
				} else {
					CapsuleCollider collider = target.GetComponent<CapsuleCollider>();
					if (collider != null) offsetToHead = new Vector3(0, collider.height, 0);
				}
			}
		}
		
		/// <summary>
		/// Updates the position of the game object over the character's head.
		/// </summary>
		void Update() {
			BarkPosition = target.position + offsetToHead;
			Vector3 viewportPos = gameplayCamera.WorldToViewportPoint(BarkPosition);
			IsVisible = (viewportPos.z >= 0);
			if (!IsVisible) return;
			myTransform.position = uiCamera.ViewportToWorldPoint(viewportPos);
			myTransform.localPosition = new Vector3(myTransform.localPosition.x, myTransform.localPosition.y, 0);
		}
	
	}

}
	