
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
/*
[CustomEditor(typeof(TestConditionalHolder))]
public class ConditionalEditor : Editor
{
  private Type[] conditionalTypes;
  private int[] conditionalIds;
  private string[] conditionalNames;
  private int selectedIndex;
  private int numConditionals;
  private TestConditionalHolder test;

  public override void OnInspectorGUI()
  {
    test = target as TestConditionalHolder;

    GUILayout.Space(3f);

    conditionalTypes = typeof(Conditional).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Conditional))).ToArray<Type>();

    numConditionals = conditionalTypes.Length;
    selectedIndex = numConditionals; // Last index is <None>, so default to that
    conditionalNames = new String[numConditionals + 1];
    conditionalIds = new int[numConditionals + 1];
    for (int i = 0; i < numConditionals; i++)
    {
      conditionalNames[i] = conditionalTypes[i].FullName;
      conditionalIds[i] = i;
    }

    conditionalNames[numConditionals] = "<None>";
    conditionalIds[numConditionals] = -1;

    for (int i = 0; i < test.c.Length; i++)
    {
      drawConditionalEditor(test.c[i], i);
    }

    drawConditionalAddButton();
    //NGUIEditorTools.DrawEvents("On Click", button, button.onClick);
  }

  private void drawConditionalEditor(Conditional c, int index)
  {
    SerializedProperty sp = serializedObject.FindProperty("c").GetArrayElementAtIndex(index);
    //EditorGUILayout.LabelField(index.ToString());
    EditorGUILayout.PropertyField(sp, new GUIContent(index.ToString()+"\t\t"+c.GetType().FullName), true);
  }

  private void drawConditionalAddButton()
  {
    int selectedIndex = EditorGUILayout.Popup(numConditionals, conditionalNames);
    if (selectedIndex != numConditionals)
    {
      Conditional[] newConditionalList = new Conditional[test.c.Length + 1];
      for (int i = test.c.Length - 1; i >= 0; i--)
      {
        newConditionalList[i] = test.c[i];
      }

      newConditionalList[test.c.Length] = (Conditional) conditionalTypes[selectedIndex].GetConstructor(Type.EmptyTypes).Invoke(new object[] {});

      test.c = newConditionalList;
    }
  }
}
*/