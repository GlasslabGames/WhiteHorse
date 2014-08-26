using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UILabel))]
public class VersionLabel : MonoBehaviour {

	// Use this for initialization
	[ContextMenu("Execute")]
	void Start () {
    UILabel label = GetComponent<UILabel>();
    label.text = "Version " + GLResourceManager.InstanceOrCreate.GetVersionString();
	}
}
