using UnityEngine;
using System.Collections;

public class DetailViewButton : MonoBehaviour {
	public UITexture background;
	public UILabel label;
	public UILabel priceLabel;
	private GLButton m_button;
	private Color m_originalColor;

	void Init () {
		m_button = GetComponent<GLButton> ();
	}
	
	public void SetEnabled(bool enable = true, bool noMoney = false) {
		if (m_button) {
			m_button.enabled = enable;
		}
		if (background != null) {
			if (!enable) {
				if (enabled) m_originalColor = background.color; // store the active background color, which was set based on the player's color
				background.color = GameObjectAccessor.Instance.GameColorSettings.disabledButton;
			} else if (m_originalColor != Color.clear) background.color = m_originalColor;
		}

		Color textColor = (enable)? Color.white : GameObjectAccessor.Instance.GameColorSettings.disabledButtonText;

		if (label != null) {
			label.color = textColor;
		}

		if (priceLabel != null) {
			if (noMoney) priceLabel.color = GameObjectAccessor.Instance.GameColorSettings.redStateDark;
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
