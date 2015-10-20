
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
  
  void Awake() {
    m_webView = GetComponent<GLWebView>();
  }
  
  void OnEnable() {
    // Set the player handle to default
    //SdkManager.Instance.GLSDK.SetPlayerHandle( "" );
    Debug.Log("*** Loading login webview..", this);
    m_webView.LoadView("/sdk/v2/game/"+SdkManager.SDK_CLIENT_ID+"/login", LoginCallback, NoConnectionCallback);
  }

  void OnDisable()
  {
    Debug.Log("LoginPanelWebview disabled, hiding webview", this);
    if (m_webView != null)
    {
      m_webView.CancelRequest();
    }

    //MainMenuController.Instance.ConnectToServer();
  }

  private void onGetUserInfoReturn(string data)
  {
    //MainMenuController.Instance.HideLoadingScreen();
    
    Debug.Log("Loaded user info: " + data);

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
    // Get the account information
		/*
    AccountManager.CURRENT_USER_NAME = Convert.ToString(parsedData["username"]);
    AccountManager.CURRENT_USER_FIRST_NAME = Convert.ToString(parsedData["firstName"]);
    AccountManager.CURRENT_USER_LAST_NAME = Convert.ToString(parsedData["lastName"]);
    AccountManager.CURRENT_USER_ROLE = Convert.ToString(parsedData["role"]);
		AccountManager.CURRENT_USER_ID = Convert.ToInt64(parsedData["id"]);
		*/
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
	
		Debug.Log ("Username: "+rUsername + "PlayerHandle: "+rUserhandle);

		/*
    Debug.Log("Current account role: " + AccountManager.CURRENT_USER_ROLE, this);
    // If we're an instructor, disable telemetry
    if( AccountManager.CURRENT_USER_ROLE == "instructor" ) {
      SdkManager.Instance.GLSDK.SetDataCaptureEnabled( false );
    }
    else 
    */
    {
      //SdkManager.Instance.GLSDK.SetDataCaptureEnabled( true );
    }

	/*
    // If we're a tutorial user, set that
    if( userType == "Demo" ) {
      Debug.Log ( "found a DEMO type" );
      SdkManager.Instance.GLSDK.SetDataCaptureEnabled( true );
      SdkManager.Instance.GLSDK.SetIsTutorialuser( true );
    }
    */

    // Start the first session and set auto management on sessions
    SdkManager.Instance.GLSDK.SetAutoSessionManagement( true );
    SdkManager.Instance.StartSession();

    // Set the connected state to online
    //MainMenuController.Instance.SetConnectedState( ConnectedState.LOGGED_IN );
    
    //AccountManager.Instance.SetCurrentAccount(AccountManager.CURRENT_USER_NAME);

    //Debug.Log("Requesting user save data...", this);
    //MainMenuController.Instance.ShowLoadingScreen();
    //SdkManager.Instance.GLSDK.GetSaveGame(onGetUserSaveReturn); // no save data in this game
  }

  private void onGetUserSaveReturn(string data)
  {
    //MainMenuController.Instance.HideLoadingScreen();

    Debug.Log("Loaded user save: " + data, this);

    Dictionary<string, object> parsedData = MiniJSON.Json.Deserialize(data) as Dictionary<string, object>;
    if (parsedData == null)
    {
      Debug.Log("No save data received.", this);
      // new game, ignore
      //SessionManager.InstanceOrCreate.SetSaveJSON("");
			rSaveData = "";
			//ProfileManager.Instance.ClearCurrentProfileData();
    }
    else if (parsedData.ContainsKey("error"))
    {
      if ((string) parsedData["error"] != "no game data")
      {
        // Handle unknown error
        Debug.LogError("Unable to retrieve user save", this);
        SdkManager.Instance.GLSDK.Logout();
        gameObject.SetActive(false);
        return;
      }

      Debug.Log("User save get returned unknown error, starting game with empty save.", this);

      // Else start new game
			//SessionManager.InstanceOrCreate.SetSaveJSON("");
			//ProfileManager.Instance.ClearCurrentProfileData();
    }
    else
    {
			/*
      Debug.Log("Save data loaded.", this);
      // If we have data that has no error, assume it's a save blob and set info
      if (parsedData.ContainsKey(SessionManager.AVATAR_KEY))
      {
        AvatarType avatar = (AvatarType) Enum.Parse(typeof(AvatarType), Convert.ToString(parsedData[SessionManager.AVATAR_KEY]));
        AccountManager.Instance.SelectAvatar(avatar);
      }

      SessionManager.InstanceOrCreate.SetSaveJSON(data);
      */ 
			rSaveData = data;
    }

    Debug.Log("Starting game", this);
    // Start game
    //MainMenuController.Instance.PlayGame();
		if (onLoginComplete != null)
			onLoginComplete ();
		Application.LoadLevel("lobby"); // proceed to the lobby
  }
  
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

          //MainMenuController.Instance.ShowLoadingScreen();

          SdkManager.Instance.GLSDK.GetUserInfo(onGetUserInfoReturn);
          break;
          
        case "FAIL":
          Debug.LogError( "Received FAIL action from /sdk/v2/game/"+SdkManager.SDK_CLIENT_ID+"/login" , this);
          
          // Close the web view
          //webView.Hide();
          //gameObject.SetActive(false);
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

  private void NoConnectionCallback( bool noInternet )
  {
    Debug.Log("No connection callback received. noInternet = " + noInternet, this);
    if (m_webView != null)
    {
      m_webView.CancelRequest();
    }

    //MainMenuController.Instance.DisplayNoInternetModal( noInternet );

    gameObject.SetActive( false );
  }
}