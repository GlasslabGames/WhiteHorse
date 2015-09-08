using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class Model<T>
{
	public static SortedDictionary<int, T> ms_models = new SortedDictionary<int, T>(); // filled in by ModelDataStore

	public int Id;

	public static List<T> Models {
		get {
			return ms_models.Values.ToList();
		}
	}

	public static T GetModel(int id)
	{
		if (ms_models.ContainsKey(id)) return ms_models[id];
		else return default(T);
	}

  static Model() {
    ModelDataStore.SetUp();
  }

}