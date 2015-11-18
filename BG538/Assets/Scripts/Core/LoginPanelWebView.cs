
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(GLWebView))]
public class LoginPanelWebView : MonoBehaviour {
  
	public static string rUsername;
	public static string rUserhandle; // includes userId
	public static string rSaveData;

  // Web View for displaying front-end modals and pages
  private GLWebView m_webView;

	public event Action onLoginComplete;
	public event BoolEvent onLoginFail;
	  
  void Awake() {
    m_webView = GetComponent<GLWebView>();
  }
  
  void OnEnable() {
	#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
    Debug.Log("*** Loading login webview..", this);
    m_webView.LoadView("/sdk/v2/game/"+SdkManager.SDK_CLIENT_ID+"/login", LoginCallback, NoConnectionCallback);
	#endif
  }

  void OnDisable()
  {
	#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
    Debug.Log("LoginPanelWebview disabled, hiding webview", this);
    if (m_webView != null)
    {
      m_webView.CancelRequest();
    }
	#endif
  }

  
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
  private bool LoginCallback( UniWebView webView, UniWebViewMessage message ) {
    if( message.rawMessage.IndexOf( "/auth/edmodo/login" ) != -1 )
    {
      //webView.Load( message.rawMessage );

      // Close the web view
      webView.Hide();
      gameObject.SetActive(false);

      //MainMenuController.Instance.GoToEdmodoLogin();

      return false;
    }

    if( message.args.ContainsKey( "action" ) ) {
      Debug.Log("Login callback received: " + message.args["action"], this);
      switch( message.args[ "action" ] ) {
        case "CLOSE":
          // Close the web view
          webView.Hide();
          gameObject.SetActive(false);
          break;
          
        case "SUCCESS":

          // Close the web view
          webView.Hide();
          gameObject.SetActive(false);

					// Get the user info
          SdkManager.Instance.GLSDK.GetUserInfo(onGetUserInfoReturn);
          break;
          
        case "FAIL":
          Debug.LogError( "Received FAIL action from /sdk/v2/game/"+SdkManager.SDK_CLIENT_ID+"/login" , this);
          
          // Close the web view
          webView.Hide();
          gameObject.SetActive(false);

					if (onLoginFail != null) onLoginFail( true );
          break;
          
        default:
          break;
      }

      return true;
    }
    else
    {
      return false;
    }
  }
#endif

	private void onGetUserInfoReturn(string data)
	{
		//MainMenuController.Instance.HideLoadingScreen();
		
		Debug.Log("*** Loaded user info: " + data);
		
		Dictionary<string, object> parsedData = MiniJSON.Json.Deserialize(data) as Dictionary<string, object>;
		if (parsedData == null || (parsedData.ContainsKey("status") && (string) parsedData["status"] == "error"))
		{
			Debug.LogError("Unable to retrieve user info");
			SdkManager.Instance.GLSDK.Logout();
			gameObject.SetActive(false);
			return;
		}
		
		// Reset the tutorial user state
		//SdkManager.Instance.GLSDK.SetIsTutorialuser( false );
		
		Debug.Log("Setting AccountManager information...", this);
		rUsername = Convert.ToString (parsedData ["username"]);
		SdkManager.username = rUsername;
		// Get the account information
		
		string userType = "";
		if( parsedData["type"] != null ) {
			userType = Convert.ToString(parsedData["type"]);
			Debug.Log ( "we have a user type: " + userType );
		}
		
		Debug.Log("Setting userID in SdkManager and performing device update...", this);
		// Now we need to pair the session cookie we sent to the server (which is considered valid at this point)
		// with the new device Id we're about to set, which is the user's id + base device Id.
		string myCookie = SdkManager.Instance.GLSDK.GetCookie( true );
		rUserhandle = rUsername + "-" + Convert.ToInt64 (parsedData ["id"]);
		SdkManager.Instance.GLSDK.SetPlayerHandle( rUserhandle );
		SdkManager.Instance.GLSDK.SetCookie( myCookie );
		SdkManager.Instance.GLSDK.DeviceUpdate();
		
		
		// Start the first session and set auto management on sessions
		SdkManager.Instance.GLSDK.SetAutoSessionManagement( true );
		//SdkManager.Instance.StartSession();
		
		Debug.Log("*** Starting game", this);
		if (onLoginComplete != null) onLoginComplete ();
	}

  private void NoConnectionCallback( bool noInternet )
  {
    Debug.Log("*** No connection callback received. noInternet = " + noInternet + " loginFail callback: "+onLoginFail, this);
	#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
    if (m_webView != null)
    {
      m_webView.CancelRequest();
			m_webView.CurrentWebView.Hide();
    }
	#endif

		if (onLoginFail != null) onLoginFail( noInternet );

    gameObject.SetActive( false );
  }
}