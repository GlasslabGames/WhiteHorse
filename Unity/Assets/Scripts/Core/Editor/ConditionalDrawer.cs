using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;

[CustomPropertyDrawer(typeof(Conditional))]
public class ConditionalDrawer : PropertyDrawer
{
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    SerializedProperty tempProp = property.Copy();
    Rect tempRect = new Rect();
    int numConditionals = Conditional.ALL_CONDITIONAL_TYPES.Length;
    int selectedIndex = numConditionals; // Last index is <None>, so default to that
    if (tempProp.objectReferenceValue != null)
    {
      for (int i = numConditionals - 1; i>=0; i--)
      {
        if (tempProp.objectReferenceValue.GetType() == Conditional.ALL_CONDITIONAL_TYPES[i]) selectedIndex = i;
      }
    }

    EditorGUI.BeginProperty(position, label, tempProp);
    float labelWidth = position.width / 4;
    tempRect.Set(position.x, position.y, labelWidth, EditorGUIUtility.singleLineHeight);
    EditorGUI.LabelField(tempRect, label);

    tempRect.Set(position.x + labelWidth, position.y, position.width - labelWidth, EditorGUIUtility.singleLineHeight);
    int newIndex = EditorGUI.Popup(tempRect, selectedIndex, Conditional.ALL_CONDITIONAL_NAMES);

    if (tempProp.objectReferenceValue != null)
    {
      tempRect.Set(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

      EditorGUI.indentLevel += 1;
      tempProp.isExpanded = EditorGUI.Foldout(tempRect, tempProp.isExpanded, "Conditional Properties\t" + tempProp.objectReferenceInstanceIDValue);
      if (tempProp.isExpanded)
      {
        EditorGUI.BeginChangeCheck();
        SerializedObject obj = new SerializedObject(tempProp.objectReferenceValue);
        SerializedProperty prop = obj.GetIterator();
        EditorGUI.indentLevel += 1;

        prop.Next(true);
        while (prop.NextVisible(prop.isExpanded) && !SerializedProperty.EqualContents(prop, prop.GetEndProperty()))
        {
          tempRect.y += tempRect.height;
          tempRect.height = EditorGUI.GetPropertyHeight(prop, null, false);
          EditorGUI.PropertyField(tempRect, prop, prop.propertyType == SerializedPropertyType.ObjectReference ? new GUIContent(prop.objectReferenceInstanceIDValue.ToString()) : null);
          //tempRect.y += EditorGUI.GetPropertyHeight(prop) - EditorGUIUtility.singleLineHeight;
				}
        EditorGUI.indentLevel -= 1;

        if (EditorGUI.EndChangeCheck())
        {
          obj.ApplyModifiedProperties();
        }
      }
      EditorGUI.indentLevel -= 1;
    }

    EditorGUI.EndProperty();

    if (newIndex != selectedIndex)
    {
      if (newIndex == numConditionals)
      {
        tempProp.objectReferenceValue = null;
      }
      else
      {
        ScriptableObject newConditional = ScriptableObject.CreateInstance(Conditional.ALL_CONDITIONAL_TYPES[newIndex]);//.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
        tempProp.objectReferenceValue = newConditional;
        //Debug.Log(newConditional.GetInstanceID());
        tempProp.serializedObject.ApplyModifiedProperties();
      }
    }
  }

  public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
  {
    SerializedProperty tempProp = property.Copy();
    //string debugText = EditorGUI.indentLevel + " - " +label.text;
    float height = base.GetPropertyHeight(tempProp, label); // Always at least 2 lines

    if (tempProp.objectReferenceValue != null)
    {
      //debugText += "\t.";
      height += EditorGUIUtility.singleLineHeight;
      SerializedObject obj = new SerializedObject(tempProp.objectReferenceValue);
      SerializedProperty prop = obj.GetIterator();
      prop.Next(true);
      while (prop.NextVisible(prop.isExpanded) && !SerializedProperty.EqualContents(prop, prop.GetEndProperty()))
      {
        //debugText += "\t.";
				//if (prop.isExpanded || prop.depth == 0)
        if (prop.isExpanded)
        {
          //height += EditorGUIUtility.singleLineHeight;
          height += EditorGUI.GetPropertyHeight(prop, null, false);
          //debugText += "\t..";
        }
        else
        {
          height += EditorGUIUtility.singleLineHeight;
          //debugText += "\t...";
        }
      }
    }

    //Debug.Log(debugText);
    return height;
  }
}