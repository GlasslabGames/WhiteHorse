using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class EquipableModel
{
  
  [PersistAttribute]
	public int Id {
		get; protected set;
  }
	public string Description {
		get; protected set;
	}
	public string Name {
		get; protected set;
	}
  public string Label {
    get {
      if (Name != null && Name.Length > 0) { return Name; }
      else if (Description != null && Description.Length > 0) {
        if (Description.Length < 50) return Description;
        else return Description.Substring(0, 50) + "...";
      } else { return Type.ToString() + " " + Id.ToString(); }
    }
  }

  public EquipmentTypes Type {
    get; protected set;
  }

  public static void AddModel(EquipableModel model) {
    if (ms_equipables.ContainsKey(model.Id)) {
      Debug.LogError("[EquipableModel] Couldn't add model "+model.Type+" "+model.Description+" because Id "+model.Id+" already exists!");
    } else {
      ms_equipables.Add(model.Id, model);
    }
  }

  public static EquipableModel GetModel(int id)
  {
    EquipableModel model;
    ms_equipables.TryGetValue(id, out model);
    return model;
  }

  static EquipableModel() 
  {
    AddModel(new EquipableModel(0, "None", ""));
  }

  public EquipableModel() {} // Required for serialization
  
  public EquipableModel (int id, string name, string description)
  {
    Id = id;
    Name = name;
    Description = description;
  }

  public override string ToString ()
  {
    return Type.ToString() + " " + Id;
  }
}