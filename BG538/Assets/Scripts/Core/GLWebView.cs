using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;


public class GLWebView : MonoBehaviour {
	#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8

	public delegate void WebViewReceivedMessageCallback( UniWebView webView, UniWebViewMessage message );

	/**
	 * Internal class defines the URLs to load with keys and callbacks.
	 */
	public class WebView {
		public string Path;
		public Func<UniWebView, UniWebViewMessage, bool> ResponseCallback;
    public bool UseCookie;

    public WebView( string path, Func<UniWebView, UniWebViewMessage, bool> callback, bool useCookie ) {
			Path = path;
			ResponseCallback = callback;
      UseCookie = useCookie;
		}
	}

	// Keep a reference to the web view
  public UniWebView CurrentWebView { get; private set; }
	private WebView m_currentView;
  private Action<bool> m_failureCallback;

	// to avoid multithread issues, try tracking this values here (for use in doRequest)
	private static bool tryingToConnect;
	private static bool connected;
	private static bool noInternet;

  public UniWebView LoadView( string addr, Func<UniWebView, UniWebViewMessage, bool> callback, Action<bool> failureCallback = null, bool useCookie = true )
  {
		Debug.Log ("*** LoadView " + addr);
    CancelRequest(); // Cancel request if there already was one

    // Get the web view object
    CurrentWebView = gameObject.AddComponent<UniWebView>();

    // Set the failure callback
    m_failureCallback = failureCallback;
    
    // Set the web view insets
    #if !UNITY_EDITOR
    int sides = 137;
    int top = 35;
	int bottom = 417;

    CurrentWebView.insets = new UniWebViewEdgeInsets( top, sides, bottom, sides );
    #else
    CurrentWebView.insets = new UniWebViewEdgeInsets( 0, 0, 0, 0 );
    #endif

    // Register for web view events. The webview is destroyed later, so no need to unregister
    CurrentWebView.OnLoadBegin += OnLoadBegin;
    CurrentWebView.OnLoadComplete += OnLoadComplete;
    CurrentWebView.OnReceivedMessage += OnReceivedMessage;
    CurrentWebView.OnEvalJavaScriptFinished += OnEvalJavaScriptFinished;
    CurrentWebView.OnWebViewShouldClose += OnWebViewShouldClose;
    
    // Set the current web view
    m_currentView = new WebView(addr, callback, useCookie);
    
    // Show the web view and load it
    CurrentWebView.autoShowWhenLoadComplete = true;

    StartCoroutine(doRequest());
    return CurrentWebView;
  }

  private IEnumerator doRequest()
  {
		tryingToConnect = true;

		SdkManager.Instance.GLSDK.Connect(Application.persistentDataPath, SdkManager.SDK_CLIENT_ID, SdkManager.SDK_SERVER_URI, 
      delegate( string response ) {
        Debug.Log("SERVER: in ConnectCallback(): " + response);// , this);

        // Likely server is down
        if( response == "" ) {
          connected = false;
          noInternet = false;
        }
        else {
          // Deserialize the response and get the status field
          Dictionary<string, object> responseAsJSON = Json.Deserialize(response) as Dictionary<string, object>;
          if (!responseAsJSON.ContainsKey("error"))
          {
            // Set the connected state
            connected = true;
            noInternet = true;
						Debug.Log("connected!");
          } else {
					  connected = false;
					  noInternet = true;
						Debug.Log("not connected...");
					}
        }
        tryingToConnect = false;
    });

    while (tryingToConnect)
    {
      yield return null;
    }

    if (connected)
    {
			Debug.Log("*** is connected!");
      if( m_currentView.UseCookie )
      {
        CurrentWebView.Load(SdkManager.Instance.GLSDK.GetConnectUri() + "/sdk?cookie=" + SdkManager.Instance.GLSDK.GetCookie() + "&redirect=" + m_currentView.Path);
      }
      else
      {
        CurrentWebView.Load(SdkManager.Instance.GLSDK.GetConnectUri() + m_currentView.Path);
      }
    }
    else
    {
      Debug.LogError("No internet connection!");
      if( m_failureCallback != null )
      {
        m_failureCallback( noInternet );
      }
    }

    yield break;
  }

  public void CancelRequest()
  {
    Debug.Log("Request canceled.", this);
		tryingToConnect = false;
    if (CurrentWebView != null)
    {
      CurrentWebView.Stop();
      cleanWebView();
    }
  }

	/**
	 * Web view default callbacks.
	 */
	private void OnLoadBegin( UniWebView webView, string loadingUrl ) {
    if (CurrentWebView == webView)
    {
      Debug.Log( "WebView OnLoadBegin: " + loadingUrl , this);
    }
    else
    {
      Debug.LogError("Received message 'OnLoadBegin' from WebView that is not owned by this GLWebView.", this);
    }
	}

  private void OnLoadComplete( UniWebView webView, bool success, string errorMessage ) {
    if (CurrentWebView == webView)
    {
      Debug.Log("CurrentURL: " + webView.currentUrl, this);
      Debug.Log("ErrorMessage: " + errorMessage, this);
      if( success ) {
        Debug.Log( "WebView OnLoadComplete succeeded!" , this);
        CurrentWebView.AddUrlScheme( "http" );
      }
      else {
        Debug.Log( "WebView OnLoadComplete failed: " + errorMessage , this);
        cleanWebView();
      }
    }
    else
    {
      Debug.LogError("Received message 'OnLoadComplete' from WebView that is not owned by this GLWebView.", this);
    }
	}

  private void OnReceivedMessage( UniWebView webView, UniWebViewMessage message ) {
    if (webView == CurrentWebView)
    {
      Debug.Log( "WebView OnReceiveMessage: " + message.rawMessage , this);

      foreach (KeyValuePair<string, string> entry in message.args)
      {
        Debug.Log("Key: "+entry.Key+", Value: "+entry.Value);
      }
      //Key: openURL, Value: www.playfully.org
      if (message.args.ContainsKey("openURL"))
      {
        string url = WWW.UnEscapeURL(message.args["openURL"]);
        if (!url.StartsWith("http://"))
        {
          url = "http://" + url;
        }
        Debug.Log("Opening web page: "+url);
        Application.OpenURL(url);
      }

      // Forward the messages to the appropriate callback
      bool shouldClean = m_currentView.ResponseCallback( CurrentWebView, message );
      if( shouldClean ) {
        cleanWebView();
      }
    }
    else
    {
      Debug.LogError("Received message 'OnReceivedMessage' from WebView that is not owned by this GLWebView.", this);
    }
	}

  private void cleanWebView()
  {
    Debug.Log("Cleaning...", this);
    CurrentWebView.Hide();
    Destroy( CurrentWebView );

    // CurrentWebView is set null when webview closes and sends back an event below
    //CurrentWebView = null;
  }

  private void OnEvalJavaScriptFinished( UniWebView webView, string result ) {
		Debug.Log( "WebView OnEvalJavaScriptFinished: " + result , this);
	}

  private bool OnWebViewShouldClose( UniWebView webView ) {
    Debug.Log( "WebView OnWebViewShouldClose" , this);
    if( webView == CurrentWebView ) {
      CurrentWebView = null;
      return true;
    }
    return false;
  }
#endif
}