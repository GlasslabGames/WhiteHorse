using UnityEngine;
using System.Collections;

public class BroadcastEvent : Interaction {
  public string EventName;

  public override void Do() {
    PlayMakerFSM.BroadcastEvent(EventName);

    base.Do();
  }
}
