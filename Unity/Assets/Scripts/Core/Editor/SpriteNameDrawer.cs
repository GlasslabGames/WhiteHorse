using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

[CustomPropertyDrawer (typeof (SpriteNameAttribute))]
public class SpriteNameDrawer : PropertyDrawer {

  UIAtlas atlas;
  SpriteNameAttribute spriteNameAttribute { get { return ((SpriteNameAttribute)attribute); } }

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    string atlasName = spriteNameAttribute.atlasName;
    string currentValue = property.stringValue;

    UIAtlas[] atlases = Resources.FindObjectsOfTypeAll<UIAtlas>();
    atlas = atlases.FirstOrDefault(x => x.name == atlasName);

    if (atlas == null) {
      EditorGUI.LabelField (position, label.text, "Can't find atlas with name <"+atlasName+">!");
    } else {
      BetterList<string> spriteNames = atlas.GetListOfSprites();
      spriteNames.Insert(0, "(None)");
      int index = Array.FindIndex(spriteNames.ToArray(), x => x == currentValue);
			if (index < 0) index = 0;
      index = EditorGUI.Popup(position, label.text, index, spriteNames.ToArray());
      property.stringValue = spriteNames[index];
    }

  }
}
