using UnityEngine;
using PixelCrushers.DialogueSystem;

/// <summary>
/// "Use" the item, which can be an event in a quest.
/// </summary>
public class Talk : Interaction {
  public string LuaCode;
  public string ConversationName;

  override protected void Reset() { // sets the default in the inspector
    base.Reset ();
    //Properties.OnceOnly = true;
  }

  public override void Do() {
    if (LuaCode != null && LuaCode != "")
      Lua.Run(LuaCode);
    GLDialogueManager.Instance.StartConversation (ConversationName, null);
    
    base.Do ();
  }
}
