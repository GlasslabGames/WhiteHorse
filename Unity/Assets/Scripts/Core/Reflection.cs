using System;
using System.Reflection;

// Caching reflection results may be in order if this class get used a lot.
public static class Reflection {
  public static void Set(Object target, string fieldName, object newValue)
  {
    target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, newValue);
  }

  public static object Get(Object target, string fieldName)
  {
    return(target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target));
  }

  public static object Call(Object target, string methodName, params object[] args)
  {
    return target.GetType().GetMethod(methodName).Invoke(target, args);
  }

  /// <summary>
  /// Gets the name.
  /// </summary>
  /// <returns>The string of the variable name passed in via anonymous object.</returns>
  /// <param name="item">Expected ot be passed in via "new { [variable] }" </param>
  /// <typeparam name="T">The 1st type parameter.</typeparam>
  public static string GetName<T>(T item) where T : class
  {
    // Get the name of the original variable by a "feature" of C# and anonymous objects that will store the 
    // value passed in as the name of the argument used to assgn that variable.
    PropertyInfo[] properties = typeof(T).GetProperties();
    if (properties.Length != 1) {
      // TODO: This is ... wrong.  need to figure out what to do here.
    }

    properties [0].GetType ();
    return properties[0].Name;

  }
}
