using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StatePopupButton : MonoBehaviour {
	public Image background;
	public Text functionLabel;
	public Text priceLabel;
	private Button button;

	void Start () {
		button = GetComponent<Button>();
	}
	
	public void SetEnabled(bool enable = true, bool noMoney = false) {
		if (button) button.interactable = enable;

		background.color = (enable) ? AutoSetColor.GetColorForPlayer (true, AutoSetColor.ColorChoice.darker) : GameSettings.Instance.Colors.disabledButton;
		
		Color textColor = (enable)? Color.white : GameSettings.Instance.Colors.disabledButtonText;
		
		if (functionLabel) functionLabel.color = textColor;
		
		if (priceLabel) {
			if (noMoney) priceLabel.color = GameSettings.Instance.Colors.darkRed;
			else priceLabel.color = textColor;
		}
	}
	
	public void SetPrice(float price) {
		if (priceLabel != null) {
			string s = "";
			if (price > 0) s += "-"; // positive price -> minus money
			else s += "+";
			s += "$" + Mathf.Abs(price);
			priceLabel.text = s;
		}
	}
}
