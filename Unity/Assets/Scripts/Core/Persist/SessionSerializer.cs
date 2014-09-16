using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Collections;

namespace GlassLab.Core.Serialization
{
  class SessionSerializer
  {
    /**
     * SERIALIZATION
     */

    public static Dictionary<string, object> Serialize(object target)
    {
      if (target is GameObject)
      {
        return SerializeGameObject((GameObject)target);
      }
      else
      {
        return SerializeObject(target); // We don't have deserialization for data classes yet
      }
    }

    /*
    // TODO: Overloads (Currently not using because dynamic typing may choose the wrong overload, ie. "object o = (GameObject) go" will choose the object instead of GameObject overload due to compile-time selection of functions)
    public static Dictionary<string, object> Serialize(GameObject target)
    {

    }
    */

    public static bool HasPersistAttributes(object target)
    {
      Type targetType = target.GetType();
      IEnumerable<PropertyInfo> props = targetType.GetProperties().Where(x => x.GetCustomAttributes(typeof(PersistAttribute), false).Length > 0);
      IEnumerable<FieldInfo> fields = targetType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetCustomAttributes(typeof(PersistAttribute), false).Length > 0);

      return fields.Count() > 0 || props.Count() > 0;
    }

    public static Dictionary<string, object> SerializeObject(object target)
    {
      Dictionary<string, object> data = new Dictionary<string, object>();

      Type targetType = target.GetType();
      data["__type__"] = targetType.FullName;

      IEnumerable<PropertyInfo> props = targetType.GetProperties().Where(
        x => x.GetCustomAttributes(typeof(PersistAttribute), false).Length > 0 // TODO: 2nd parameter of false should probably be true, check
        );
      foreach (PropertyInfo prop in props)
      {
        if (prop.PropertyType.IsEnum)
        {
          data[prop.Name] = prop.GetValue(target, null).ToString();
        }
        // MiniJSON already knows how to serialize these types - ICollections [Arrays, lists, dictionaries], value types, strings
        else if (prop.PropertyType == typeof(ICollection) || prop.PropertyType.IsSubclassOf(typeof(ICollection)) ||
                 prop.PropertyType.GetInterfaces().Contains(typeof(ICollection)) ||
                 prop.PropertyType.IsValueType || prop.PropertyType == typeof(string))
        {
          data[prop.Name] = prop.GetValue(target, null);
        }
        else // Recurse through non-primitive data classes
        {
          data[prop.Name] = Serialize(prop.GetValue(target, null));
        }
      }

      IEnumerable<FieldInfo> fields = targetType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(
        x => x.GetCustomAttributes(typeof(PersistAttribute), false).Length > 0
        );
      foreach (FieldInfo field in fields)
      {
        if (field.FieldType.IsEnum)
        {
          data[field.Name] = field.GetValue(target).ToString();
        }
        // MiniJSON already knows how to serialize these types - ICollections [Arrays, lists, dictionaries], value types, strings
        else if (field.FieldType.IsValueType || field.FieldType == typeof(string))
        {
          data[field.Name] = field.GetValue(target);
        }
        else if (field.FieldType == typeof(ICollection) || field.FieldType.IsSubclassOf(typeof(ICollection)) ||
                 field.FieldType.GetInterfaces().Contains(typeof(ICollection)))
        {
          if (field.FieldType.IsGenericType)
          {
            Type genericType = field.FieldType.GetGenericTypeDefinition();
            if (genericType == typeof(List<>))
            {
              // TODO deserializeList
              Type valueType = field.FieldType.GetGenericArguments()[0];
              if (valueType.IsEnum)
              {
                data[field.Name] = field.GetValue(target);
              }
              else
              {
                data[field.Name] = field.GetValue(target);
              }
            }
            else if (genericType == typeof(Dictionary<,>))
            {
              // TODO deserializeDictionary
              data[field.Name] = field.GetValue(target);
            }
          }
          else
          {
            data[field.Name] = field.GetValue(target);
          }
        }
        else // Recurse through non-primitive data classes
        {
          data[field.Name] = Serialize(field.GetValue(target));
        }
      }

      return data;
    }

    private static Dictionary<string, object> SerializeGameObject(GameObject target)
    {
      Dictionary<string, object> data = new Dictionary<string, object>();

      data["__type__"] = typeof(GameObject).FullName;

      data["__activeSelf__"] = target.activeSelf;

      // Reflect Attributes
      MonoBehaviour[] components = target.GetComponents<MonoBehaviour>();

      for (int i = components.Length - 1; i >= 0; i--)
      {
        MonoBehaviour component = components[i];
        data[component.GetType().FullName] = Serialize(component);
      }

      // Get children, store them

      return data;
    }
  }
}