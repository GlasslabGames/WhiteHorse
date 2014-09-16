using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniJSON;
using System;
using System.Reflection;

public static class ModelDataStore 
{
 
  static ModelDataStore() 
  {
#if UNITY_EDITOR
    if (EditorApplication.isCompiling) return;
#endif
    Load ();
  }

  public static void SetUp() {} // the previous function will automatically be called if this is the first time SetUp was called

  static void Load()
  {
    Debug.Log("Loading ModelDataStore");
    	Dictionary<string, object> data = ReadJsonFile("demoData");
    
    	// Create a dictionary of models for each type
		List<DemoEnemyModel> enemies = ParseCategory<DemoEnemyModel>( data["Enemies"] as Dictionary<string, object> );
    	enemies.TrimExcess();
		DemoEnemyModel.ms_models = new SortedDictionary<int, DemoEnemyModel>(enemies.ToDictionary(x => x.Id, x => x));

		List<DemoBarkModel> barks = ParseCategory<DemoBarkModel>( data["Barks"] as Dictionary<string, object> );
		barks.TrimExcess();
		DemoBarkModel.ms_models = new SortedDictionary<int, DemoBarkModel>(barks.ToDictionary(x => x.Id, x => x));
  }

  static Dictionary<string, object> ReadJsonFile(string fileName) {
    TextAsset textAsset = Resources.Load(fileName) as TextAsset; // load the JSON file
    Dictionary<string, object> dict = (Dictionary<string, object>) Json.Deserialize( textAsset.text );
    return dict;
  }

  static List<T> ParseCategory<T>(Dictionary<string, object> category) where T : new() {
    List<object> nodes = category["nodes"] as List<object>; // "nodes" is the tag with the list of claims/data/etc
    List<T> parsed = new List<T>();
    T obj;

    // Loop through the nodes and create an object of the right type (claim, data, etc) for each
    foreach (Dictionary<string, object> node in nodes) {
      if (node != null) {
        //Debug.Log ("Parsing node with id "+node["Id"]);
        obj = ParseNode<T>(node); // this creates the actual object
        parsed.Add(obj);
      } else {
        Debug.LogError("[ModelDataStore] Found bad/empty node.");
      }
    }

    return parsed;
  }

  static T ParseNode<T>(Dictionary<string, object> node) where T : new() {
    Type type = typeof(T);
    T obj = new T(); // create a new object of the given type

    // Set each field/property of that object to the value based on the dictionary
    foreach (string key in node.Keys) {
      Debug.Log ("[ModelDataStore] "+key + ": " + node[key]);

      // We don't know if it's a field or a property, so try both
      PropertyInfo propertyInfo = type.GetProperty(key);
      FieldInfo fieldInfo = type.GetField(key);
      if (propertyInfo == null && fieldInfo == null) {
        Debug.LogError("[ModelDataStore] Can't access field/property "+key+" in "+type+"!");
        continue;
      }

      Type propType = (propertyInfo != null)? propertyInfo.PropertyType : fieldInfo.FieldType;
      Debug.Log("PropType: "+propType+" IsGeneric?"+propType.IsGenericType);

      // If the property is a list, we need to convert all the entries to the appropriate type
      if (propType.IsGenericType) {
        List<object> list = node[key] as List<object>;
        if (list != null)
        {
          Type listType = propType.GetGenericArguments()[0];

          // based on the list type, we need to convert the entries in the list to the correct type, then save that list as the value
          if (listType == typeof(System.Int64) || listType == typeof(System.Int32)) {
            List<int> intList = new List<int>();

            foreach (object entry in list)
            {
              int i = Convert.ToInt32 (entry);
              intList.Add(i);
            }
            
            if (propertyInfo != null) propertyInfo.SetValue(obj, intList, null);
            else fieldInfo.SetValue(obj, intList);
          }
          else if (listType == typeof(System.Single) || listType == typeof(System.Double)) {
            List<float> floatList = new List<float>();
            
            foreach (object entry in list)
            {
              float f = Convert.ToSingle (entry);
              floatList.Add(f);
            }
            
            if (propertyInfo != null) propertyInfo.SetValue(obj, floatList, null);
            else fieldInfo.SetValue(obj, floatList);
          }
          else if (listType == typeof(System.String)) {
            List<string> stringList = new List<string>();
            
            foreach (object entry in list)
            {
              string s = Convert.ToString (entry);
              stringList.Add(s);
            }
            
            if (propertyInfo != null) propertyInfo.SetValue(obj, stringList, null);
            else fieldInfo.SetValue(obj, stringList);
          }
          else Debug.LogError ("[ModelDataStore] Property "+key+" has list with unusable type "+listType);
        }
        else Debug.LogError ("[ModelDataStore] Property "+key+" has unusable generic type "+node[key].GetType());
      }
      else {
        //Debug.Log("[ModelDataStore] Trying to access field/property "+key+" in "+type+" with value "+node[key]);
        var value = Convert.ChangeType(node[key], propType); // convert the value to the appropriate type

        // Then set that value to the field/property
        if (propertyInfo != null) propertyInfo.SetValue(obj, value, null);
        else fieldInfo.SetValue(obj, value);
      }
    }

    return obj;
  }
 
}