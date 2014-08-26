using UnityEngine;
public delegate void OnDragEventHandler(GLDragEventArgs args);

public class GLDragEventArgs : GLConsumableEventArgs
{
  public GLDragEventArgs(GLDragDropItem obj, GLDragDropContainer container)
  {
    DragObject = obj;
    DragContainer = container;
  }
  public readonly GLDragDropItem DragObject;
  public readonly GLDragDropContainer DragContainer;
}