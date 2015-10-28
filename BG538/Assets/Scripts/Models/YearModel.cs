using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class YearModel : Model<YearModel> {
	public string Year;
	public List<float> Populations;
	public List<int> ElectoralCounts;
}