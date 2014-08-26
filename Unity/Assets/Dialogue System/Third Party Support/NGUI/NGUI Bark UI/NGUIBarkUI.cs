using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.NGUI {
	
	/// <summary>
	/// Implements IBarkUI using NGUI to show bark text above a character's head. To use this
	/// component, add it to an NPC. You must also set up a single UIBarkRoot in your NGUI UI.
	/// All NGUIBarkUI instances will add their labels to this UIBarkRoot. Instead of using 
	/// the appearance properties below (font, color, etc.), you can create a template label
	/// in the bark root and assign the template property. The bark UI system will use this
	/// template instead.
	/// </summary>
	[AddComponentMenu("Dialogue System/Third Party/NGUI/Bark UI")]
	public class NGUIBarkUI : MonoBehaviour, IBarkUI {
		
		/// <summary>
		/// If assigned, the bark uses this template when creating a UILabel.
		/// </summary>
		public UILabel template;
		
		/// <summary>
		/// Set <c>true</c> to include the barker's name in the text.
		/// </summary>
		public bool includeName = false;
		
		/// <summary>
		/// The font to use for the bark text.
		/// </summary>
		public UIFont font;
		
		/// <summary>
		/// The color to use for the bark text.
		/// </summary>
		public Color color = Color.white;
		
		/// <summary>
		/// The text effect to apply to the bark text.
		/// </summary>
		public UILabel.Effect effect = UILabel.Effect.None;
		
		/// <summary>
		/// Set <c>true</c> to bounce the size of the text in.
		/// </summary>
		public bool tweenScale = true;
		
		/// <summary>
		/// The duration in seconds to show the bark text before fading it out.
		/// </summary>
		public float duration = 5f;
		
		/// <summary>
		/// Set <c>true</c> to make the bark text follow the barker.
		/// </summary>
		public bool followBarker = true;

		/// <summary>
		/// If set, follows this transform instead of the barker's.
		/// </summary>
		public Transform overrideBarkerTransform;

		/// <summary>
		/// Set <c>true</c> to keep the bark text onscreen until the sequence ends.
		/// </summary>
		public bool waitUntilSequenceEnds;
		
		/// <summary>
		/// Set <c>true</c> to run a raycast to the player. If the ray is blocked (e.g., a wall
		/// blocks visibility to the player), don't show the bark.
		/// </summary>
		public bool checkIfPlayerVisible = true;
		
		/// <summary>
		/// The layer mask to use when checking for player visibility.
		/// </summary>
		public LayerMask visibilityLayerMask = 1;
		
		/// <summary>
		/// The seconds left to display the current bark.
		/// </summary>
		private float secondsLeft = 0f;
		
		/// <summary>
		/// The label that this component will use to display bark text.
		/// </summary>
		private UILabel label = null;

		private Transform playerCameraTransform = null;
		
		private Collider playerCameraCollider = null;
		
		private NGUIFollowTarget followTarget = null;
		
		/// <summary>
		/// Starts the component by adding a bark label to the UIBarkRoot object, which should
		/// be found in an NGUI UI.
		/// </summary>
		public void Start() {
			if (UIBarkRoot.rootObject != null) {
				if ((template != null) && (template.GetComponent<UILabel>() != null)) {
					
					// Create a label using the template:
					GameObject labelObject = Instantiate(template.gameObject) as GameObject;
					labelObject.transform.parent = UIBarkRoot.rootObject.transform;
					label = labelObject.GetComponent<UILabel>();
				} else {
					
					// Create a label using the specified properties:
					label = NGUITools.AddWidget<UILabel>(UIBarkRoot.rootObject);
					if ((font == null) && DialogueDebug.LogWarnings) Debug.LogError(string.Format("{0}: No font assigned to NGUIBarkUI on {1}.", DialogueDebug.Prefix, name));
					label.bitmapFont = font;
					label.color = color;
					label.effectStyle = effect;
					label.overflowMethod = UILabel.Overflow.ResizeFreely;
				}
				
				// Set up the label but leave it invisible for now:
				label.gameObject.name = string.Format("Bark ({0})", name);
				if (followBarker) {
					followTarget = label.gameObject.GetComponent<NGUIFollowTarget>() ?? label.gameObject.AddComponent<NGUIFollowTarget>();
					followTarget.target = (overrideBarkerTransform != null) ? overrideBarkerTransform : this.transform;
				} else {
					Destroy(label.gameObject.GetComponent<NGUIFollowTarget>());
				}
				label.gameObject.SetActive(false);
			} else {
				if (DialogueDebug.LogErrors) Debug.LogWarning(string.Format("{0}: No UIBarkRoot found in scene. Add one to your NGUI UI.", DialogueDebug.Prefix));
			}
		}
		
		/// <summary>
		/// Updates the seconds left and hides the label if time is up.
		/// </summary>
		public void Update() {
			if (secondsLeft > 0) {
				secondsLeft -= Time.deltaTime;
				if (checkIfPlayerVisible && (followTarget != null)) CheckPlayerVisibility();
				if ((secondsLeft <= 0) && (label != null) && !waitUntilSequenceEnds) Hide();
			}
		}
		
		/// <summary>
		/// If the barker is destroyed, also destroy its bark label.
		/// </summary>
		public void OnDestroy() {
			if (label != null) Destroy(label.gameObject);
		}
		
		/// <summary>
		/// Barks the specified subtitle.
		/// </summary>
		/// <param name='subtitle'>
		/// Subtitle to bark.
		/// </param>
		public void Bark(Subtitle subtitle) {
			if (label != null) {
				if (includeName) {
					label.text = string.Format("{0}: {1}", subtitle.speakerInfo.Name, subtitle.formattedText.text);
				} else {
					label.text = subtitle.formattedText.text;
				}
				label.gameObject.SetActive(true);
				if (tweenScale) {
					TweenScale tween = TweenScale.Begin(label.gameObject, 1f, Vector3.one);
					tween.from = 0.5f * Vector3.one;
					tween.method = UITweener.Method.BounceIn;
				} else {
					label.transform.localScale = Vector3.one;
				}
				secondsLeft = duration;
				playerCameraTransform = Camera.main.transform;
				playerCameraCollider = (playerCameraTransform != null) ? playerCameraTransform.collider : null;
			}
		}
		
		/// <summary>
		/// Indicates whether a bark is playing or not.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is playing; otherwise, <c>false</c>.
		/// </value>
		public bool IsPlaying { 
			get { return secondsLeft > 0; }
		}
		
		void OnBarkEnd(Transform actor) {
			Hide();
		}
		
		private void Hide() {
			label.gameObject.SetActive(false);
		}
		
		private void CheckPlayerVisibility() {
			bool canSeePlayer = true;
			if ((playerCameraTransform != null) && followTarget.IsVisible) {
				RaycastHit hit;
				if (Physics.Linecast(followTarget.BarkPosition, playerCameraTransform.position, out hit, visibilityLayerMask)) {
					canSeePlayer = (hit.collider == playerCameraCollider);
				}
			}
			if (!canSeePlayer && label.gameObject.activeInHierarchy) {
				label.gameObject.SetActive(false);
			} else if (canSeePlayer && !label.gameObject.activeInHierarchy) {
				label.gameObject.SetActive(true);
			}
		}
		
	}
	
}