using UnityEngine;

public class SpriteNameAttribute : PropertyAttribute {
  public readonly string atlasName;

  // Define some constants here so that it's easy to change them later
  public const string POPUP_ATLAS = "Exploration - Objects";
  public const string INVENTORY_ATLAS = "Exploration - Inventory Objects";
  public const string ROBOT_SELECT_ATLAS = "Robot Select";
  public const string ROBOT_SELECT_NEW_ATLAS = "NewBotSelect";
  public const string ROBOTS_ATLAS = "Bots";

  public SpriteNameAttribute (string atlasName) {
    this.atlasName = atlasName;
  }
}
