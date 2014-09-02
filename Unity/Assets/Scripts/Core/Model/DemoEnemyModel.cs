using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class DemoEnemyModel: Model<DemoEnemyModel>
{
	public string Name;
	public List<float> Position;
	public List<float> Scale;
	public int Bark;
}