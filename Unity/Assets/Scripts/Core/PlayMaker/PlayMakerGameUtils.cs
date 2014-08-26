using System;
using System.Reflection;

public static class PlayMakerGameUtils {
/*  static void Test()
  {
    int args = 0;
    Console.WriteLine("Name is '{0}'", Reflection.GetName(new {args}));
    Console.ReadLine();
  }
*/
  /// <summary>
  /// Gets the FSM variable.
  /// </summary>
  /// <param name="fsm">The PlayMakerFSM to operate on</param>
  /// <param name="variable">Argument to synchronize.  Expected ot be passed in via "new { [variable] }" </param>
  /// <typeparam name="T">The 1st type parameter.</typeparam>
  public static void GetFSMVariable<T>(PlayMakerFSM fsm, T variable) where T : class
  {
    // value passed in as the name of the argument used to assgn that variable.
    PropertyInfo[] properties = typeof(T).GetProperties();
    if (properties.Length != 1) {
      // TODO: This is ... wrong.  need to figure out what to do here.
    }

    string variableName = properties[0].Name;
    Type variableType = properties[0].GetType();

    // Create the function we need to call
    string fsmTypeName = "Fsm" + variableType.ToString();
    string fsmMethodName = "Get" + fsmTypeName;

    // Get the variable from the FSM
    var newValueType = Reflection.Call(fsm.FsmVariables, fsmMethodName, variableName);// m_gameState.FsmVariables.GetFsmBool("m_playerTurn").Value

    // We trust this newValueType has a "Value" function, but we don't know the exact type :(
    T newValue = (T)Reflection.Call(newValueType, "Value");

    // Use reflection to set the supplied variable
    Reflection.Set(variable, variableName, newValue);
  }


}
