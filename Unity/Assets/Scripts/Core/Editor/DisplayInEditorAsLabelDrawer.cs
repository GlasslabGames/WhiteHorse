using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

[CustomPropertyDrawer(typeof(DisplayInEditorAsLabelAttribute))]
public class DisplayInEditorAsLabelDrawer  : PropertyDrawer {
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    label = EditorGUI.BeginProperty(position, label, property);
    position = EditorGUI.PrefixLabel (position, GUIUtility.GetControlID (FocusType.Passive), label);
    string labelValue = "";
    switch (property.propertyType) {
    case SerializedPropertyType.Integer:
      labelValue = property.intValue.ToString();
      break;
    case SerializedPropertyType.Boolean:
      labelValue = property.boolValue.ToString();
      break;
    case SerializedPropertyType.Float:
      labelValue = property.floatValue.ToString();
      break;
    case SerializedPropertyType.String:
      labelValue = property.stringValue;
      break;
    case SerializedPropertyType.Color:
      labelValue = "<To Be Implemented>";
      break;
    case SerializedPropertyType.ObjectReference:
      labelValue = property.objectReferenceValue.ToString();
      break;
    case SerializedPropertyType.LayerMask:
      labelValue = "<To Be Implemented>";
      break;
    case SerializedPropertyType.Enum:
      // Attempts to call Name on the enum to support Description attribute.  Requires EnumExtension.cs.
      labelValue = (property.serializedObject.targetObject.GetType().GetField(property.name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(property.serializedObject.targetObject) as Enum).ToString();
      //labelValue = property.enumNames[property.enumValueIndex];
      break;
    case SerializedPropertyType.Vector2:
      labelValue = "<To Be Implemented>";
      break;
    case SerializedPropertyType.Vector3:
      labelValue = "<To Be Implemented>";
      break;
    case SerializedPropertyType.Rect:
      labelValue = "<To Be Implemented>";
      break;
    case SerializedPropertyType.ArraySize:
      labelValue = "<To Be Implemented>";
      break;
    case SerializedPropertyType.Character:
      labelValue = "<To Be Implemented>";
      break;
    case SerializedPropertyType.AnimationCurve:
      labelValue = "<To Be Implemented>";
      break;
    case SerializedPropertyType.Bounds:
      labelValue = "<To Be Implemented>";
      break;
    case SerializedPropertyType.Gradient:
      labelValue = "<To Be Implemented>";
      break;
    }

    EditorGUI.LabelField (position, labelValue);
    EditorGUI.EndProperty();
  }
}