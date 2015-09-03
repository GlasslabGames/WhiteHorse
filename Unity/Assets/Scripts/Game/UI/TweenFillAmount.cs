// Based on TweenFillAmount

using UnityEngine;

/// <summary>
/// Tween a UIBasicSprite (UITexture or UISprite)'s fillAmount.
/// </summary>

public class TweenFillAmount : UITweener
{
	public float from;
	public float to;

	UIBasicSprite mSprite;

	public UIBasicSprite cachedSprite {
		get {
			if (mSprite == null) mSprite = GetComponent<UIBasicSprite>();
			return mSprite;
		}
	}

	/// <summary>
	/// Tween's current value.
	/// </summary>

	public float value
	{
		get
		{
			return cachedSprite.fillAmount;
		}
		set
		{
			cachedSprite.fillAmount = value;
		}
	}

	void Awake () {}

	/// <summary>
	/// Tween the value.
	/// </summary>

	protected override void OnUpdate (float factor, bool isFinished) { value = from * (1f - factor) + to * factor; }

	/// <summary>
	/// Start the tweening operation.
	/// </summary>

	static public TweenFillAmount Begin (GameObject go, float duration, float target)
	{
		TweenFillAmount comp = UITweener.Begin<TweenFillAmount>(go, duration);
		comp.from = comp.value;
		comp.to = target;

		if (duration <= 0f)
		{
			comp.Sample(1f, true);
			comp.enabled = false;
		}
		return comp;
	}

	[ContextMenu("Set 'From' to current value")]
	public override void SetStartToCurrentValue () { from = value; }

	[ContextMenu("Set 'To' to current value")]
	public override void SetEndToCurrentValue () { to = value; }

	[ContextMenu("Assume value of 'From'")]
	void SetCurrentValueToStart () { value = from; }

	[ContextMenu("Assume value of 'To'")]
	void SetCurrentValueToEnd () { value = to; }
}
