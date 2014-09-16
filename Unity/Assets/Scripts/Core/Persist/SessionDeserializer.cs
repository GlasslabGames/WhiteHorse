using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace GlassLab.Core.Serialization
{
	class SessionDeserializer
  {
    /**
     * DESERIALIZATION
     */

    private static void DeserializeGameObject(GameObject target, Dictionary<string, object> objectData)
    {
      // Set active from save data
      target.SetActive((bool)objectData["__activeSelf__"]);


      MonoBehaviour[] components = target.GetComponents<MonoBehaviour>();
      // We assume the components already exist
      for (int i = components.Length - 1; i >= 0; i--)
      {
        MonoBehaviour component = components[i];

        string componentName = component.GetType().FullName;
        if (!objectData.ContainsKey(componentName))
        {
          continue;
        }
        Dictionary<string, object> componentData = (Dictionary<string, object>)objectData[componentName];
        Deserialize(component, componentData);
      }

    }

    public static void Deserialize(object target, Dictionary<string, object> data)
    {
      if (target is GameObject)
      {
        DeserializeGameObject((GameObject)target, data);
      }
      else // Custom data class
      {
        DeserializeObject(target, data);
      }
    }

    public static object DeserializeNew(object data, Type targetType)
    {
      if (data == null)
      {
        return null;
      }

      if (targetType.IsArray)
      {
        return deserializeArray(data, targetType);
      }
      else if (targetType.IsGenericType)
      {
        Type genericType = targetType.GetGenericTypeDefinition();
        if (genericType == typeof(List<>))
        {
          return deserializeList(data, targetType);
        }
        else if (genericType == typeof(Dictionary<,>))
        {
          return DeserializeDictionary(data, targetType);
        }
      }
      else if (targetType.IsEnum)
      {
        return Enum.Parse(targetType, (string)data);
      }
      else
      {
        if (targetType.IsValueType || targetType == typeof(string))
        {
          if (data.GetType() != targetType)
          {
            return Convert.ChangeType(data, targetType);
          }
          else
          {
            return data;
          }
        }
        else
        {
          // Data classes
          Dictionary<string, object> objectData = (Dictionary<string, object>)data;
          Type objectType = Type.GetType((string)objectData["__type__"]);
          ConstructorInfo valueConstructor = objectType.GetConstructor(Type.EmptyTypes);
          if (valueConstructor == null)
          {
            throw new Exception("Deserialized type " + objectType.FullName + " needs a constructor with no arguments!");
          }

          object deserializedValue = valueConstructor.Invoke(new object[] { });
          //DeserializeJSON(deserializedValue, (string) data);
          Deserialize(deserializedValue, objectData);

          return deserializedValue;
        }
      }

      // Unreachable, but compiler can't tell for some reason
      return null;
    }

    public static void DeserializeObject(object target, Dictionary<string, object> componentData)
    {
      Type targetType = target.GetType();

      // basic declared variables
      foreach (string fieldName in componentData.Keys)
      {
        // Skip typeinfo data
        if (fieldName == "__type__")
        {
          continue;
        }

        object data = componentData[fieldName];

        // Try putting in a field
        FieldInfo field = targetType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null)
        {
          field.SetValue(target, DeserializeNew(data, field.FieldType));
        }
        else
        {
          // If no field, put in a property
          PropertyInfo prop = targetType.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
          if (prop != null)
          {
            prop.SetValue(target, DeserializeNew(data, prop.PropertyType), null);
          }
          else
          {
            Debug.LogError("[SessionManager] Could not find field or property '" + fieldName + "' in " + targetType.FullName);
            continue;
          }
        }
      }
    }

    private static IList deserializeList(object data, Type fieldType)
    {
      Type valueType = fieldType.GetGenericArguments()[0];
      List<object> list = (List<object>)data;

      Type genericList = typeof(List<>); // Generic list type
      Type specificList = genericList.MakeGenericType(valueType); // Specific list type
      ConstructorInfo ci = specificList.GetConstructor(Type.EmptyTypes); // Reflect constructor for specific list type (List <t>)
      IList newList = (IList)ci.Invoke(new object[] { }); // Instantiate list

      // Re-add elements to list
      int listCount = list.Count;

      for (int j = 0; j < listCount; j++)
      {
        newList.Add(DeserializeNew(list[j], valueType));
      }
      /*
      if (valueType.IsValueType || valueType == typeof(string))
      {
        for (int j = 0; j < listCount; j++)
        {
          newList.Add(Convert.ChangeType(list[j], valueType));
        }
      }
      else
      {
        for (int j = 0; j < listCount; j++)
        {
          Dictionary<string, object> objectData = (Dictionary<string, object>) list[j];
          string typeString = (string) objectData["__type__"];
          Type objectType = Type.GetType(typeString);
          ConstructorInfo valueConstructor = objectType.GetConstructor(Type.EmptyTypes);
          object deserializedValue = valueConstructor.Invoke(new object[] { });
          //DeserializeJSON(deserializedValue, (string) list[j]);
          Deserialize(deserializedValue, objectData);
          newList.Add(deserializedValue);
        }
      }
      */

      return newList;
    }

    private static Array deserializeArray(object data, Type fieldType)
    {
      List<object> array = (List<object>)data;
      Type t = fieldType.GetElementType();
      int numElements = array.Count;
      var convertedArray = Array.CreateInstance(t, numElements);
      for (int j = 0; j < numElements; j++)
      {
        convertedArray.SetValue(DeserializeNew(array[j], t), j);
      }

      return convertedArray;
    }

    // Refactor to take (object target, Dictionary<string, object> data)
    public static IDictionary DeserializeDictionary(object data, Type fieldType)
    {
      Type keyType = fieldType.GetGenericArguments()[0];
      Type valueType = fieldType.GetGenericArguments()[1];
      Dictionary<string, object> dict = (Dictionary<string, object>)data;

      Type genericDictionary = typeof(Dictionary<,>); // Generic type
      Type specificDictionary = genericDictionary.MakeGenericType(keyType, valueType); // Specific type
      ConstructorInfo ci = specificDictionary.GetConstructor(Type.EmptyTypes); // Reflect constructor for specific type
      IDictionary newDict = (IDictionary)ci.Invoke(new object[] { }); // Instantiate

      // Re-add elements to list
      List<string> keys = new List<string>(dict.Keys);
      int keyCount = keys.Count;
      string key;

      for (int j = 0; j < keyCount; j++)
      {
        key = keys[j];
        newDict[Convert.ChangeType(key, keyType)] = DeserializeNew(dict[key], valueType);
      }
      /*
      // Could be optimized by running through the types below, but using DeserializeNew for code cleanliness.
      // TODO: Write an optimized function for this case
      if (valueType.IsArray)
      {
        for (j = 0; j < keyCount; j++)
        {
          key = keys[j];
          newDict[Convert.ChangeType(key, keyType)] = deserializeArray(dict[key], valueType);
        }
      }
      else if (valueType.IsGenericType)
      {
        Type genericType = valueType.GetGenericTypeDefinition();
        if (genericType == typeof(List<>))
        {
          for (j = 0; j < keyCount; j++)
          {
            key = keys[j];
            newDict[Convert.ChangeType(key, keyType)] = deserializeList(dict[key], valueType);
          }
        }
        else if (genericType == typeof(Dictionary<,>))
        {
          for (j = 0; j < keyCount; j++)
          {
            key = keys[j];
            newDict[Convert.ChangeType(key, keyType)] = DeserializeDictionary(dict[key], valueType);
          }
        }
        else
        {
          // Dunno? (Unhandled generics case)
        }
      
      }
      else if (valueType.IsValueType || valueType == typeof(string))
      {
        for (j = 0; j < keyCount; j++)
        {
          key = keys[j];
          newDict[Convert.ChangeType(key, keyType)] = Convert.ChangeType(dict[key], valueType);
        }
      }
      else
      {
        for (j = 0; j < keyCount; j++)
        {
          key = keys[j];
          Dictionary<string, object> objectData = (Dictionary<string, object>) dict[key];
          string typeString = (string) objectData["__type__"];
          Type objectType = Type.GetType(typeString);
          ConstructorInfo valueConstructor = objectType.GetConstructor(Type.EmptyTypes);
          object deserializedValue = valueConstructor.Invoke(new object[] { });
          //DeserializeJSON(deserializedValue, (string) dict[key]);
          Deserialize(deserializedValue, objectData);
          newDict[Convert.ChangeType(key, keyType)] = deserializedValue;
        }
      }
      */
      return newDict;
    }
	}
}
