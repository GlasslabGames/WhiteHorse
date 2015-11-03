using UnityEngine;
using System.Collections;

public class Walkthrough : MonoBehaviour {
	public Transform guideParent;
	private int index;

	public void Show() {
		gameObject.SetActive(true);
		index = -1;
		Continue();
	}

	public void Hide() {
		gameObject.SetActive(false);
	}

	public void Continue() {
		index ++;
		if (index >= guideParent.childCount) {
			Hide();
		} else {
			for (var i = 0; i < guideParent.childCount; i++) {
				guideParent.GetChild(i).gameObject.SetActive(i == index);
			}
		}
	}
}
