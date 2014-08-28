using UnityEngine;
using UnityEditor;
using System.Reflection;

public static class DebugMenu
{
  [MenuItem("Debug/Print Global Position")]
  public static void PrintGlobalPosition()
  {
    if (Selection.activeGameObject != null)
    {
      Debug.Log(Selection.activeGameObject.name + " is at " + Selection.activeGameObject.transform.position);
	  Debug.Log(Selection.activeGameObject.name + "'s lossy scale: " + Selection.activeGameObject.transform.lossyScale.x);
    }
  }

	[MenuItem("Debug/Print NGUI Bounds")]
	public static void PrintNGUIBounds()
	{
		if (Selection.activeGameObject != null)
		{
      Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds (Selection.activeGameObject.transform);
			Debug.Log(Selection.activeGameObject.name + " bounds: " + bounds);
		}
	}

	[MenuItem("Debug/Print NGUI WorldCorners")]
	public static void PrintNGUIWorldCorners()
	{
		if (Selection.activeGameObject != null)
		{
			UIWidget widget = Selection.activeGameObject.GetComponent<UIWidget>();
			if (widget == null) Debug.Log ("No widget on this object!");
			else {
				Vector3[] corners = widget.worldCorners;
				foreach (Vector3 v in corners) {
					Debug.Log(Selection.activeGameObject.name + " corner: " + v);
				}
			}
		}
	}

  [MenuItem("Debug/Refresh Component")]
  public static void RefreshComponent()
  {
    if (Selection.activeGameObject != null)
    {
      Selection.activeGameObject.SendMessage("Refresh", SendMessageOptions.DontRequireReceiver);
    }
  }
}
