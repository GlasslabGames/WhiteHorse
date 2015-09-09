using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StatePopupButton : MonoBehaviour {
	public Image background;
	public Text functionLabel;
	public Text priceLabel;
	private Button button;

	void Init () {
		button = GetComponent<Button> ();
	}
	
	public void SetEnabled(bool enable = true, bool noMoney = false) {
		if (button) button.enabled = enable;

		if (background != null) {
			/* TODO if (!enable) {
				if (enabled) m_originalColor = background.color; // store the active background color, which was set based on the player's color
				background.color = GameObjectAccessor.Instance.GameColorSettings.disabledButton;
			} else if (m_originalColor != Color.clear) background.color = m_originalColor;*/
		}
		
		Color textColor = (enable)? Color.white : GameSettings.Instance.Colors.disabledButtonText;
		
		if (functionLabel) functionLabel.color = textColor;
		
		if (priceLabel) {
			if (noMoney) priceLabel.color = GameSettings.Instance.Colors.darkRed;
			else priceLabel.color = textColor;
		}

		enabled = enable;
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
