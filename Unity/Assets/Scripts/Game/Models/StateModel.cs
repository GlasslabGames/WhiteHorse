using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StateModel : Model<StateModel> {
  public string Name;
  public string Abbreviation;
  public int ElectoralCount;
  public float Population;

  private static Dictionary<string, StateModel> ms_statesByAbbreviation;

  public static StateModel GetModelByAbbreviation(string a) {
    if (ms_statesByAbbreviation == null) {
      ms_statesByAbbreviation = new Dictionary<string, StateModel>();
      foreach (StateModel s in ms_models.Values) {
        ms_statesByAbbreviation.Add(s.Abbreviation, s);
      }
    }
    return ms_statesByAbbreviation[a];
  }
}
