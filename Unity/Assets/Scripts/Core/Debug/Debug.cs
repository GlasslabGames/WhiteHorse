#if !UNITY_EDITOR
using UnityEngine;

public static class Debug
{
  public static new void Log(object message)
  {
    Log(message, "No context");
  }
  
  public static new void Log(object message, UnityEngine.Object context)
  {
    #if !GAME_RELEASE
    string output = constructMessage(message,context);
    TestFlightBinding.Log(output);
    UnityEngine.Debug.Log(output, context);
	//if (DebugDataCollector.Instance != null)
		//DebugDataCollector.Instance.AddLog(output);
    #endif
  }
  
  public static new void LogError(object message)
  {
    LogError(message, "No context");
  }
  
  public static new void LogError(object message, UnityEngine.Object context)
  {
    #if !GAME_RELEASE
    string output = constructMessage(message, context);
    TestFlightBinding.Log("ERROR: "+output);
	UnityEngine.Debug.LogError(output, context);
	//if (DebugDataCollector.Instance != null)
		//DebugDataCollector.Instance.AddLog(output);
    #endif
  }
  
  public static new void LogWarning(object message)
  {
    LogWarning(message, "No context");
  }
  
  public static new void LogWarning(object message, UnityEngine.Object context)
  {
    #if !GAME_RELEASE
    string output = constructMessage(message, context);
    TestFlightBinding.Log(output);
    UnityEngine.Debug.LogWarning(output, context);
	//if (DebugDataCollector.Instance != null)
		//DebugDataCollector.Instance.AddLog(output);
    #endif
  }
  
  public static new void DrawLine(Vector3 v, Vector3 v2, Color c)
  {
    UnityEngine.Debug.DrawLine(v, v2, c);
  }
  
  public static new void DrawRay(Vector3 pos, Vector3 direction, Color c)
  {
    UnityEngine.Debug.DrawRay(pos, direction, c);
  }

  public static void Log(object message, object context)
  {
    #if !GAME_RELEASE
    string output = constructMessage(message, context);
    TestFlightBinding.Log(output);
	UnityEngine.Debug.Log(output, context as UnityEngine.Object);
	//if (DebugDataCollector.Instance != null)
		//DebugDataCollector.Instance.AddLog(output);
    #endif
  }

  public static new void LogError(object message, object context)
  {
    #if !GAME_RELEASE
    string output = constructMessage(message, context);
    TestFlightBinding.Log("ERROR: "+output);
	UnityEngine.Debug.LogError(output, context as GameObject);
	//if (DebugDataCollector.Instance != null)
		//DebugDataCollector.Instance.AddLog(output);
    #endif
  }

  private static string constructMessage(object message, object context)
  {
    if (message == null)
    {
      message = "null";
    }

    if (context != null)
    {
      Component contextAsComponent = context as Component;
      if (contextAsComponent != null)
      {
        return "["+contextAsComponent.GetType().FullName+"(GameObject:" +
          Utility.GetHierarchyString(contextAsComponent.gameObject) +
            ")] " + message.ToString();
      }

      GameObject contextAsGameObject = context as GameObject;
      if (contextAsGameObject != null)
      {
        return "[(GameObject:" + Utility.GetHierarchyString(contextAsGameObject) + ")] " + message.ToString();
      }
      
      return "[" + context.ToString() + "] " + message.ToString();
    }
    else
    {
      return message.ToString();
    }
  }

  public static new void LogWarning(object message, object context)
  {
    #if !GAME_RELEASE
    string output = constructMessage(message, context);
    TestFlightBinding.Log("WARNING: "+output);
	UnityEngine.Debug.LogWarning(output, context as GameObject);
	//if (DebugDataCollector.Instance != null)
		//DebugDataCollector.Instance.AddLog(output);
    #endif
  }
}
#endif