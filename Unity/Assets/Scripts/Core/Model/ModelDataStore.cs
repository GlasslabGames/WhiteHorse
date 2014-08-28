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
  /*
  public static SortedDictionary<int, DataModel> ms_datas = new SortedDictionary<int, DataModel>();
  public static SortedDictionary<int, ClaimModel> ms_claims = new SortedDictionary<int, ClaimModel>();
  public static SortedDictionary<int, TopicModel> ms_topics = new SortedDictionary<int, TopicModel>();
  */

  static ModelDataStore() 
  {
#if UNITY_EDITOR
    if (EditorApplication.isCompiling) return;
#endif
    Load ();
  }

  static void Load()
  {
    Dictionary<string, object> data = ReadJsonFile("JsonData");
    /*
    // Create a dictionary of models for each type
    List<TopicModel> topics = ParseCategory<TopicModel>( data["Topics"] as Dictionary<string, object> );
    topics.TrimExcess();
    ms_topics = new SortedDictionary<int, TopicModel>(topics.ToDictionary(x => x.Id, x => x));

    List<DataModel> datas = ParseCategory<DataModel>( data["Evidence"] as Dictionary<string, object> );
    datas.TrimExcess();
    ms_datas = new SortedDictionary<int, DataModel>(datas.ToDictionary(x => x.Id, x => x));
    
    List<ClaimModel> claims = ParseCategory<ClaimModel>( data["Claims"] as Dictionary<string, object> );
    claims.TrimExcess();
    ms_claims = new SortedDictionary<int, ClaimModel>(claims.ToDictionary(x => x.Id, x => x));
    */
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
    bool isEquipment = category.ContainsKey("equipment") && (bool) category["equipment"]; // this tag indicates which ones can be collected

    // Loop through the nodes and create an object of the right type (claim, data, etc) for each
    foreach (Dictionary<string, object> node in nodes) {
      if (node != null) {
        //Debug.Log ("Parsing node with id "+node["Id"]);
        obj = ParseNode<T>(node); // this creates the actual object
        parsed.Add(obj);
        if (isEquipment) EquipableModel.AddModel(obj as EquipableModel);
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
      //Debug.Log ("[ModelDataStore] "+key + ": " + node[key]);

      // We don't know if it's a field or a property, so try both
      PropertyInfo propertyInfo = type.GetProperty(key);
      FieldInfo fieldInfo = type.GetField(key);
      if (propertyInfo == null && fieldInfo == null) {
        Debug.LogError("[ModelDataStore] Can't access field/property "+key+" in "+type+"!");
        continue;
      }

      Type propType = (propertyInfo != null)? propertyInfo.PropertyType : fieldInfo.FieldType;

      // If the property is a list of ints, we need to convert all the entries
      if (propType.IsGenericType) {
        List<object> list = node[key] as List<object>;
        if (list != null)
        {
          List<int> intList = new List<int>(); // for now, assume that we only care about lists of ints

          foreach (object entry in list)
          {
            int i = Convert.ToInt32 (entry);
            intList.Add(i);
          }

          // Now that we have a list of ints, we can set that value for the field/property
          if (propertyInfo != null) propertyInfo.SetValue(obj, intList, null);
          else fieldInfo.SetValue(obj, intList);
        }
        //else Debug.LogWarning ("[ModelDataStore] Found generic type "+node[key].GetType());
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