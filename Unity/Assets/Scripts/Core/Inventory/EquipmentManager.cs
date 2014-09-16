using UnityEngine;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using System.Linq;
using System;
using GlassLab.Core.Serialization;

public sealed class EquipmentManager : SingletonBehavior<EquipmentManager> {
  [PersistAttribute]
  public List<int> m_inventoryItemIds = new List<int>(); // current available item ids

  [PersistAttribute]
  private List<int> m_trashedIDList = new List<int>();
  
  [PersistAttribute]
  public List<int> m_collectedItemIds = new List<int>(); // all items ever found in this game (by id)
  // so if we once picked up an item, it never reappears in the world even when it's not longer in our inventory
  // Not sure if this is still being used now that we have ways to discard items and stuff :?

  [PersistAttribute]
  private List<int> m_discoveredIDList = new List<int>(); // List of items discovered


  public void UntrashEquipment(EquipableModel em)
  {
    if (IsEquipmentTrashed(em)) {
      m_trashedIDList.Remove(em.Id);

      SessionManager.InstanceOrCreate.Save();
      
      PegasusManager.Instance.GLSDK.AddTelemEventValue( "dataId", em.Id );
      PegasusManager.Instance.AppendDefaultTelemetryInfo();
      PegasusManager.Instance.GLSDK.SaveTelemEvent( "Untrash_data" );
    }
  }

  public void TrashEquipment(EquipableModel em)
  {
    Debug.Log ("[EquipmentManager] TrashEquipment "+em.Description);
    if (!IsEquipmentTrashed(em)) {
      m_trashedIDList.Add(em.Id);
      
      SessionManager.InstanceOrCreate.Save();
      
      PegasusManager.Instance.GLSDK.AddTelemEventValue( "dataId", em.Id );
      PegasusManager.Instance.AppendDefaultTelemetryInfo();
      PegasusManager.Instance.GLSDK.SaveTelemEvent( "Trash_data" );
    }
  }

  public bool IsEquipmentTrashed(EquipableModel em)
  {
    return m_trashedIDList.Contains(em.Id);
  }

  public void UntrashAllEquipment()
  {
    m_trashedIDList.Clear();
  }

  public bool IsEquipmentDiscovered(EquipableModel em)
  {
    return m_discoveredIDList.Contains(em.Id);
  }

  public void LogEquipmentDiscovered(EquipableModel em)
  {
    if (!IsEquipmentDiscovered(em))
    {
      m_discoveredIDList.Add(em.Id);
    }
  }

  private EquipmentManager() {}

  public void ResetInventory()
  {
    m_discoveredIDList.Clear();

    NotifyEquipmentChanged ();

    SessionManager.InstanceOrCreate.Save();
  }

  public bool HasEquipment(int equipmentID)
  {
    return m_inventoryItemIds.Contains(equipmentID);
  }

  public bool HasEquipment(EquipableModel equipment)
  {
    return HasEquipment(equipment.Id);
  }

	public void Add(int id) {
		EquipableModel e = EquipableModel.GetModel (id);
		if (e != null)
    {
      Add (e);
    } else {
      Debug.LogWarning ("[EquipmentManager] Trying to add equipment "+id+" but there's no equipment with that ID!");
    }
	}

  public void Remove(int id)
  {
    EquipableModel e = EquipableModel.GetModel(id);
    if (e != null)
    {
      Remove(e);
    }
    else
    {
      Debug.LogWarning("[EquipmentManager] Trying to remove equipment " + id + " but there's no equipment with that ID!");
    }
  }

  public void Remove(EquipableModel equipment)
  {
    if (!HasEquipment(equipment))
    {
      Debug.LogWarning("[EquipmentManager] Player doesn't have equipment id "+equipment.Id+". Aborting remove.", this);
      return;
    }

    m_inventoryItemIds.Remove(equipment.Id);

    NotifyEquipmentChanged ();
    
    SessionManager.InstanceOrCreate.Save();
  }

  public void Add(EquipableModel equipment)
  {
    if (HasEquipment(equipment))
    {
      Debug.LogWarning("[EquipmentManager] Player already has equipment id "+equipment.Id+". Aborting add.", this);
      return;
    }

    m_inventoryItemIds.Add(equipment.Id);
    
    NotifyEquipmentChanged ();

    SessionManager.InstanceOrCreate.Save();
  }

  public void NotifyEquipmentChanged()
  {
    if (SignalManager.EquipmentChanged != null ) SignalManager.EquipmentChanged();
  }
}
