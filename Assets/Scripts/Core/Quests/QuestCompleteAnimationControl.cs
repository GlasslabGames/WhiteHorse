using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestCompleteAnimationControl : MonoBehaviour {

  public bool IsPendingPlayAnimation = false;
  public GLAfterEffectsAnimationController QuestCompleteAni;
  public UISprite Background;
  public QuestView questView;
  private UIWidget[] m_widgets;
  private bool m_isPlayingAni = false;

  public TextPopup MissionPopup;
  public UITable QuestTable;
  public UITable AniTable;

  void Awake() {
    m_widgets = GetComponentsInChildren<UIWidget>(true);
  }

  void OnEnable() {
    QuestCompleteAni.AnimationFinished += OnAnimationComplete;
    ChangeAlpha(IsPendingPlayAnimation ? 0f : 1f);
    QuestCompleteAni.ResetAnimation();
    MissionPopup.Table = QuestTable;
    MissionPopup.Refresh();
  }

  void Update() {
    if (IsPendingPlayAnimation && !m_isPlayingAni)
    {
      MissionPopup.Table = AniTable;
      MissionPopup.Refresh();
      m_isPlayingAni = true;
      ChangeAlpha(0f);
      StartCoroutine(PlayQuestCompleteAnimation());
    }
  }

  IEnumerator PlayQuestCompleteAnimation() {
    yield return new WaitForSeconds(0.1f);
    Vector3 aniPos = QuestCompleteAni.transform.localPosition;
    aniPos.y = (Background.transform.localPosition.y - Background.height) / 2 - 40f;
    QuestCompleteAni.transform.localPosition = aniPos;
    QuestCompleteAni.PlayAnimation(0);
    IsPendingPlayAnimation = false; // even if we don't finish the animation, we still want to stop it from playing agin
    yield return null;
  }

  void OnAnimationComplete(GLAfterEffectsAnimationController controller) {
    ChangeAlpha(1f);
    MissionPopup.Table = QuestTable;
    MissionPopup.Refresh();
    questView.Open(QuestView.AUTOMATIC_OPEN_SECONDS);
    m_isPlayingAni = false;
  }

  void ChangeAlpha(float alpha) {
    if (m_widgets != null)
    {
      foreach (var wid in m_widgets)
        wid.alpha = alpha;
    }
  }

  void OnDisable () {
    QuestCompleteAni.AnimationFinished -= OnAnimationComplete;
    StopAllCoroutines();
    m_isPlayingAni = false;
    if ((QuestCompleteAni.IsPlaying || QuestCompleteAni.IsInterrupted) && QuestCompleteAni.GetCurrentFrame() > 10)
    {
      IsPendingPlayAnimation = false;
    }
  }
}
