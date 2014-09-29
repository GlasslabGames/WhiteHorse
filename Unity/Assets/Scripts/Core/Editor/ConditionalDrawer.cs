using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using GlassLab.Core.Conditional;

[CustomPropertyDrawer(typeof(Conditional))]
public class ConditionalDrawer : PropertyDrawer
{
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    Rect tempRect = new Rect();
    int numConditionals = Conditional.ALL_CONDITIONAL_TYPES.Length;
    int selectedIndex = numConditionals; // Last index is <None>, so default to that
    if (property.objectReferenceValue != null)
    {
      for (int i = numConditionals - 1; i>=0; i--)
      {
        if (property.objectReferenceValue.GetType() == Conditional.ALL_CONDITIONAL_TYPES[i]) selectedIndex = i;
      }
    }

    EditorGUI.BeginProperty(position, label, property);
    float labelWidth = EditorGUIUtility.labelWidth - (EditorGUI.indentLevel*15);
    tempRect.Set(position.x, position.y, labelWidth, EditorGUIUtility.singleLineHeight);
    EditorGUI.LabelField(tempRect, label);

    tempRect.Set(position.x + labelWidth, position.y, position.width - labelWidth, EditorGUIUtility.singleLineHeight);
    int newIndex = EditorGUI.Popup(tempRect, selectedIndex, Conditional.ALL_CONDITIONAL_NAMES);

    if (property.objectReferenceValue != null)
    {
      tempRect.Set(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

      EditorGUI.indentLevel += 1;
      bool expand = property.isExpanded;
      expand = EditorGUI.Foldout(tempRect, property.isExpanded, "Conditional Properties");

      if (property.isExpanded)
      {
        EditorGUI.BeginChangeCheck();
        SerializedObject obj = new SerializedObject(property.objectReferenceValue);
        SerializedProperty prop = obj.GetIterator();
        EditorGUI.indentLevel += 1;

        prop.NextVisible(true); // step into first visible child
        while (prop.NextVisible(false) && !SerializedProperty.EqualContents(prop, prop.GetEndProperty()))
        {
          tempRect.y += tempRect.height;
          tempRect.height = EditorGUI.GetPropertyHeight(prop, null, true);
          //Debug.Log(property.propertyPath + "." + prop.propertyPath + ", " + prop.isExpanded + " => " + expand);
          EditorGUI.PropertyField(tempRect, prop, null, true);
				}
        EditorGUI.indentLevel -= 1;

        if (EditorGUI.EndChangeCheck()) obj.ApplyModifiedProperties();
      }
      property.isExpanded = expand;
      EditorGUI.indentLevel -= 1;
    }

    EditorGUI.EndProperty();

    if (newIndex != selectedIndex)
    {
      if (newIndex == numConditionals)
      {
        property.objectReferenceValue = null;
      }
      else
      {
        ScriptableObject newConditional = ScriptableObject.CreateInstance(Conditional.ALL_CONDITIONAL_TYPES[newIndex]);
        property.objectReferenceValue = newConditional;
        property.serializedObject.ApplyModifiedProperties();
      }
    }
  }

  public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
  {
    float height = base.GetPropertyHeight(property, label); // Always at least 2 lines

    if (property.objectReferenceValue != null)
    {
      height += EditorGUIUtility.singleLineHeight;
      if (property.isExpanded)
      {
        SerializedObject obj = new SerializedObject(property.objectReferenceValue);
        SerializedProperty prop = obj.GetIterator();
        prop.NextVisible(true); // step into first visible child
        while (prop.NextVisible(false) && !SerializedProperty.EqualContents(prop, prop.GetEndProperty()))
        {
          height += EditorGUI.GetPropertyHeight(prop, null, true);
        }
      }
    }
    return height;
  }
}