using UnityEngine;
using System.Collections;

/// <summary>
/// Supply some helper utility method for UniWebView
/// </summary>
public class UniWebViewHelper{
	/// <summary>
	/// Is we are running the on retina iOS device.
	/// </summary>
	/// <returns><c>true</c>, if running on retina retina iOS device <c>false</c> otherwise.</returns>
	/// <description>
	/// This method is used to calculate the point insets of webview. Converting the Unity's pixel to iOS's point
	/// </description>
	public static bool RunningOnRetinaIOS()
	{
		#if UNITY_IPHONE
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			return (iPhone.generation != iPhoneGeneration.iPad1Gen &&
			        iPhone.generation != iPhoneGeneration.iPad2Gen &&
			        iPhone.generation != iPhoneGeneration.iPadMini1Gen &&
			        iPhone.generation != iPhoneGeneration.iPhone &&
			        iPhone.generation != iPhoneGeneration.iPhone3G &&
			        iPhone.generation != iPhoneGeneration.iPhone3GS &&
			        iPhone.generation != iPhoneGeneration.iPodTouch1Gen &&
			        iPhone.generation != iPhoneGeneration.iPodTouch2Gen &&
			        iPhone.generation != iPhoneGeneration.iPodTouch3Gen);
		}
		#endif
		return false;
	}
}
