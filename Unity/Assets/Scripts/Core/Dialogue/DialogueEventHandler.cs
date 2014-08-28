using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;
using HutongGames.PlayMaker;

public class DialogueEventHandler : MonoBehaviour {
  public const string DialogueEndFSMEvent = "DIALOGUE_END";

  public void OnConversationStart() {
    if (SignalManager.ConversationStarted != null) { SignalManager.ConversationStarted(0); }
  }
  public void OnConversationStart(Transform actor) {
    //Debug.Log ("OnConversationStart in DialogueEventHandler");
    if (SignalManager.ConversationStarted != null) { SignalManager.ConversationStarted(0); }
	}
  
  public void OnConversationLine(Subtitle subtitle) {
    //Debug.Log ("OnConversationLine in DialogueEventHandler");
    if (SignalManager.ConversationLine != null) { SignalManager.ConversationLine(subtitle); }
  }
  
  public void OnConversationEnd(Transform actor) {
    //Debug.Log ("OnConversationNed in DialogueEventHandler");
    PlayMakerFSM.BroadcastEvent (DialogueEndFSMEvent);
    if (SignalManager.ConversationEnded != null) { SignalManager.ConversationEnded(0); }
  }
  public void OnConversationTimeout() {}
  public void OnBarkStart(Transform actor) {}
  public void OnBarkEnd(Transform actor) {}
  public void OnSequenceStart(Transform actor) {}
  public void OnSequenceEnd(Transform actor) {}
  public void OnConversationLineCancelled(Subtitle subtitle) {}
  public void OnConversationCancelled(Transform actor) {}

  public void SendFSMEvent(string fsmEvent)
  {
    PlayMakerFSM.BroadcastEvent (fsmEvent);
  }
}
