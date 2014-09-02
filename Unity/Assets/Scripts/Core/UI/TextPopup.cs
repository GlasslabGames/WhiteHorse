using UnityEngine;
using System;
using System.Collections;

public class TextPopup : MonoBehaviour {
  public UITable Table;
  public UIBasicSprite Background; // could be a UISprite or UITexture

  public UILabel NameLabel;
  public UILabel WarningLabel;
  public UILabel DescriptionLabel;

  public bool Resize = true;

  public bool RepositionWhenOffEdge; // horizontally
  private Vector3 m_prevPosition;
  private Vector3 m_defaultSpritePosition;
  private Vector3 m_defaultTablePosition;

  public bool FlipHorizontallyWhenOffEdge;
  private bool onLeft = false;

  public UITweener TweenOnShow;

  private float m_hideAfter = 0;

  public event Action OnHide;
  public event Action OnShow;
  public event Action OnResize;

  void Awake()
  {
    m_defaultSpritePosition = Background.transform.localPosition;
    m_defaultTablePosition = Table.transform.localPosition;
  }

  void Start()
  {
    Refresh ();
  }

  void OnEnable()
  {
    Refresh();
  }

  void Update()
  {
    if (m_hideAfter > 0 && gameObject.activeInHierarchy) {
      m_hideAfter -= Time.deltaTime;
      if (m_hideAfter <= 0) Hide ();
    }
  }

  public void Show(string title = "", string description = "", string warning = "")
  {
    Show (transform.position, title, description, warning);
  }
 

  public virtual void Show(Vector3 pos, string title, string description, string warning = "")
  {
    gameObject.SetActive(true);
    PlaceAt(pos); // null pos means stay in the same place
    SetText(title, description, warning);
    
    if (TweenOnShow != null) {
      TweenOnShow.enabled = true;
      TweenOnShow.ResetToBeginning();
      TweenOnShow.enabled = true;
      TweenOnShow.PlayForward();
    }

    m_hideAfter = 0; // cancel any pending order to hide, since we're showing new content

    if (OnShow != null) OnShow();
  }

  // Does some of the same things as Show (namely showing the popup) but without changing any values
  public void Appear()
  {
    gameObject.SetActive(true);
    
    if (TweenOnShow != null) {
      Debug.Log ("Trying to play tweenOnShow", TweenOnShow);
      TweenOnShow.enabled = true;
      TweenOnShow.ResetToBeginning();
      TweenOnShow.enabled = true;
      TweenOnShow.PlayForward();
    }
  }

  public void HideAfter(float delay) {
    m_hideAfter = delay;
  }

  public void Hide()
  {
    gameObject.SetActive(false);
    if (OnHide != null) OnHide();
  }

  public void SetText(string title, string description, string warning = "")
  {
    if (NameLabel) {
      NameLabel.gameObject.SetActive (title != null && title != "");
      NameLabel.text = title;
    }

    if (WarningLabel) {
      WarningLabel.gameObject.SetActive (warning != null && warning != "");
      WarningLabel.text = warning;
    }

    if (DescriptionLabel) {
      DescriptionLabel.gameObject.SetActive (description != null && description != "");
      DescriptionLabel.text = description;
    }

    Refresh ();
  }

  [ContextMenu("Refresh")]
  public void Refresh()
  {
    if (Resize && Table != null) {
      // Reposition doesn't necessarily take effect immediately, so use a callback to refresh background when it's done
      Table.onReposition += ResizeBackground;
      Table.Reposition ();
    }
  }

  // After repositioning table, resize the background to match
  protected void ResizeBackground() {
    Table.onReposition -= ResizeBackground;

    Bounds tableBounds = NGUIMath.CalculateRelativeWidgetBounds (Table.transform);
	
    Background.height = (int)(
      tableBounds.extents.y * 2
      + Table.padding.y * 2
			+ Background.border.y * 2
    );

    if (OnResize != null) OnResize();
  }

  public void PlaceAt(Vector3 position)
  {
    transform.position = position;
    if (position == m_prevPosition) return;
    m_prevPosition = position;
    
    if (RepositionWhenOffEdge || FlipHorizontallyWhenOffEdge) {
      // move the background and table to their default positions
	  Background.transform.localPosition = m_defaultSpritePosition;
      Table.transform.localPosition = m_defaultTablePosition;
      
      // then check if it's offscreen
	  Vector3[] corners = Background.worldCorners;
      bool flipped = transform.localScale.x > 0;
      // if the whole thing is flipped, the corners we care about are on the other side
      float left = (flipped)? corners[0].x : corners[2].x;
      float right = (flipped)? corners[2].x : corners[0].x;

      Camera camera = Utility.FirstAncestorOfType<UICamera>(transform).camera; // Try to get the UICamera this textPopup is under
      if (camera == null) camera = UICamera.currentCamera; // if that doesn't work for some reason, get the default camera instead
      float screenLeft = camera.ScreenToWorldPoint(new Vector3(0, 0)).x;
      float screenRight = camera.ScreenToWorldPoint(new Vector3(Screen.width, 0)).x;

      if (RepositionWhenOffEdge) {
        if (left < screenLeft || right > screenRight) { // off screen
          // adjust the x pos by the amount we need to move the edge
          float xAdjustment = (left < screenLeft)? (screenLeft - left) : (screenRight - right);

		  Vector3 pos = Background.transform.position;
          pos.x += xAdjustment;
		  Background.transform.position = pos;

          pos = Table.transform.position;
          pos.x += xAdjustment;
          Table.transform.position = pos;
        }
      } else if ((left < screenLeft && onLeft) || (right > screenRight && !onLeft)) {
        // need to flip horizontally
        Vector3 scale;
        MonoBehaviour[] behaviours = new MonoBehaviour[] { this, NameLabel, WarningLabel, DescriptionLabel };
        foreach (MonoBehaviour b in behaviours) {
          if (b != null) {
            scale = b.transform.localScale;
            scale.x *= -1;
            b.transform.localScale = scale;
          }
        }
        onLeft = !onLeft;
        Table.Reposition();
      }

    }
  }
 
}
