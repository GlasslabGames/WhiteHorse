using UnityEngine;
using System.Collections;


public class OpenURL : MonoBehaviour {

  // The URL to open
  public string Domain;
  public string Path;
  public bool UseSDKDomain;


  /**
   * Function opens the URL at the application level.
   */
  public void Open()
  {
    string domainToUse = Domain;

    if( UseSDKDomain ) {
      string sdkDomain = PegasusManager.Instance.GLSDK.GetConnectUri();
      if( sdkDomain != "" ) {
        domainToUse = sdkDomain;
      }
    }
    Application.OpenURL( domainToUse + Path );
  }
}