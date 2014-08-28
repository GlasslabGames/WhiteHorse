#if UNITY_IOS || UNITY_ANDROID
//
//	UniWebView.cs
//  Created by Wang Wei(@onevcat) on 2013-10-20.
//
using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// The main class of UniWebView. 
/// </summary>
/// <description>
/// Each gameObject with this script represent a webview object 
/// in system. Be careful: when this script's Awake() get called, it will change the name of 
/// the gameObject to make it unique in the game. So make sure this script is appeneded to a 
/// gameObject that you don't care its name.
/// </description>
public class UniWebView : MonoBehaviour {
	#region Events and Delegate
	//Delegate and event
	public delegate void LoadCompleteDelegate(UniWebView webView, bool success, string errorMessage);
	/// <summary>
	/// Occurs when a UniWebView finished loading a webpage. 
	/// </summary>
	/// <description>
	/// If loading finished successfully, success will be true, otherwise false and with an errorMessage.
	/// </description>
	public event LoadCompleteDelegate OnLoadComplete;

	public delegate void LoadBeginDelegate(UniWebView webView, string loadingUrl);
	/// <summary>
	/// Occurs when a UniWebView began to load a webpage.
	/// </summary>
	/// <description>
	/// You can do something with the UniWebView passed by parameter to get some info 
	/// or do your things when a url begins to load
	/// Sometimes, the webpage contains some other parts besides of the main frame. Whenever these parts
	/// begin to load, this event will be fired. You can get the url from parameter to know what url is
	/// about to be loaded.
	/// It is useful when you want to get some parameters from url when user clicked a link.
	/// </description>
	public event LoadBeginDelegate OnLoadBegin;

	public delegate void ReceivedMessageDelegate(UniWebView webView, UniWebViewMessage message);
	/// <summary>
	/// Occurs when a UniWebView received message.
	/// </summary>
	/// <description>
	/// If a url with format of "uniwebview://yourPath?param1=value1&param2=value2" clicked,
	/// this event will get raised with a <see cref="UniWebViewMessage"/> object.
	/// </description>
	public event ReceivedMessageDelegate OnReceivedMessage;

	public delegate void EvalJavaScriptFinishedDelegate(UniWebView webView, string result);
	/// <summary>
	/// Occurs when a UniWebView finishes eval a javascript and returned something.
	/// </summary>
	/// <description>
	/// You can use EvaluatingJavaScript method to make the webview to eval a js.
	/// The string-returned version of EvaluatingJavaScript is removed. You should
	/// always listen this event to get the result of js.
	/// </description>
	public event EvalJavaScriptFinishedDelegate OnEvalJavaScriptFinished;

	public delegate bool WebViewShouldCloseDelegate(UniWebView webView);
	/// <summary>
	/// Occurs when on web view will be closed by native. Ask if this webview should be closed or not.
	/// </summary>
	/// The users can close the webView by tapping back button (Android) or done button (iOS).
	/// When the webview will be closed, this event will be raised. 
	/// If you return false, the webview will not be closed. If you did not implement it, webview will be closed.
	/// </description>
	public event WebViewShouldCloseDelegate OnWebViewShouldClose;
	
	public delegate void ReceivedKeyCodeDelegate(UniWebView webView, int keyCode);
	/// <summary>
	/// Occurs when users clicks or taps any key while webview is actived. This event only fired on Android. 
	/// </summary>
	/// <description>
	/// On Android, the key down event can not be passed back to Unity due to some issue in Unity 4.3's Android Player.
	/// As result, you are not able to get the key input on Android by just using Unity's Input.GetKeyDown or GetKey method.
	/// If you want to know the user input while the webview on, you can subscribe this event.
	/// This event will be fired with a int number indicating the key code tapped as parameter.
	/// You can refer to Android's documentation to find out which key is tapped (http://developer.android.com/reference/android/view/KeyEvent.html)
	/// This event will be never raised on iOS, because iOS forbids the tracking of user's keyboard or device button events.
	/// </description>
	public event ReceivedKeyCodeDelegate OnReceivedKeyCode;

	#endregion

	[SerializeField]
	private UniWebViewEdgeInsets _insets = new UniWebViewEdgeInsets(0,0,0,0);

	/// <summary>
	/// Gets or sets the insets of a UniWebView object.
	/// </summary>
	/// <value>The insets in point from top, left, bottom and right edge from the screen.</value>
	public UniWebViewEdgeInsets insets {
		get {
			return _insets;
		}
		set {
			if (_insets != value) {
				_insets = value;
				UniWebViewPlugin.ChangeSize(gameObject.name,
				                            this.insets.top,
				                            this.insets.left,
				                            this.insets.bottom,
				                            this.insets.right);
				#if UNITY_EDITOR
				CreateTexture(this.insets.left,
				              this.insets.bottom,
				              Screen.width - this.insets.left - this.insets.right,
				              Screen.height - this.insets.top - this.insets.bottom
				              );
				#endif
			}
		}
	}

	/// <summary>
	/// The url this UniWebView should load. You should set it before loading webpage.
	/// </summary>
	public string url;

	/// <summary>
	/// If true, load the set url when in script's Start() method. 
	/// Otherwise, you should call Load() method yourself.
	/// </summary>
	public bool loadOnStart;

	/// <summary>
	/// If true, show the webview automatically when it finished loading. 
	/// Otherwise, you should listen the OnLoadComplete event and call Show() method your self.
	/// </summary>
	public bool autoShowWhenLoadComplete;

	/// <summary>
	/// Gets the current URL of the web page.
	/// </summary>
	/// <value>The current URL of this webview.</value>
	/// <description>
	/// This value indicates the main frame url of the webpage.
	/// It will be updated only when the webpage finishs or fails loading.
	/// </description>
	public string currentUrl {
		get {
			return UniWebViewPlugin.GetCurrentUrl(gameObject.name);
		}
	}

	private bool _backButtonEnable = true;
	private bool _bouncesEnable;
	private bool _zoomEnable;
	private string _currentGUID;

	/// <summary>
	/// Gets or sets a value indicating whether the back button of this <see cref="UniWebView"/> is enabled.
	/// It is only for Android. If true, users can use the back button of andoird device to goBack or close the web view 
	/// if there is nothing to goBack. Otherwise, the back button will do nothing when the webview is shown.
	/// This value means nothing for iOS. There is no back button for iOS devices.
	/// </summary>
	/// <value><c>true</c> if back button enabled; otherwise, <c>false</c>. Default is true</value>.
	public bool backButtonEnable {
		get {
			return _backButtonEnable;
		}
		set {
			if (_backButtonEnable != value) {
				_backButtonEnable = value;
				#if UNITY_ANDROID && !UNITY_EDITOR
				UniWebViewPlugin.SetBackButtonEnable(gameObject.name, _backButtonEnable);
				#endif
			}
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="UniWebView"/> can bounces or not.
	/// The default iOS webview has a bounces effect when drag out of edge. 
	/// The default Android webview has a color indicator when drag beyond the edge.
	/// UniWebView disabled these bounces effect by default. If you want the bounces, set this property to true.
	/// This property does noting in editor.
	/// </summary>
	/// <value><c>true</c> if bounces enable; otherwise, <c>false</c>.</value>
	public bool bouncesEnable {
		get {
			return _bouncesEnable;
		}
		set {
			if (_bouncesEnable != value) {
				_bouncesEnable = value;
				#if !UNITY_EDITOR
				UniWebViewPlugin.SetBounces(gameObject.name, _bouncesEnable);
				#endif
			}
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="UniWebView"/> can be zoomed or not.
	/// If true, users can zoom in or zoom out the webpage by a pinch gesture.
	/// This propery will valid immediately on Android. But on iOS, it will not valid until the next page loaded.
	/// You can set this property before page loading, or use Reload() to refresh current page to make it valid.
	/// </summary>
	/// <value><c>true</c> if zoom enabled; otherwise, <c>false</c>.</value> Default is false.
	public bool zoomEnable {
		get {
			return _zoomEnable;
		}
		set {
			if (_zoomEnable != value) {
				_zoomEnable = value;
				#if !UNITY_EDITOR
				UniWebViewPlugin.SetZoomEnable(gameObject.name, _zoomEnable);
				#endif
			}
		}
	}

	/// <summary>
	/// Load current url of this UniWebView.
	/// </summary>
	public void Load() {
		string loadUrl = String.IsNullOrEmpty(url) ? "about:blank" : url;
		UniWebViewPlugin.Load(gameObject.name, loadUrl);
	}

	/// <summary>
	/// A alias method to load a specified url. 
	/// </summary>
	/// <param name="aUrl">A url to set and load</param>
	/// <description>
	/// It will set the url of this UniWebView and then load it.
	/// </description>
	public void Load(string aUrl) {
		url = aUrl;
		Load();
	}

	/// <summary>
	/// Load a HTML string.
	/// </summary>
	/// <param name="htmlString">The content HTML string for the web page.</param>
	/// <param name="baseUrl">The base URL in which the webview should to refer other resources</param>
	public void LoadHTMLString(string htmlString, string baseUrl) {
		UniWebViewPlugin.LoadHTMLString(gameObject.name, htmlString, baseUrl);
	}

	/// <summary>
	/// Reload current page.
	/// </summary>
	public void Reload() {
		UniWebViewPlugin.Reload(gameObject.name);
	}

	/// <summary>
	/// Stop loading the current request. It will do nothing if the webpage is not loading.
	/// </summary>
	public void Stop() {
		UniWebViewPlugin.Stop(gameObject.name);
	}

	/// <summary>
	/// Show this UniWebView on screen. 
	/// </summary>
	/// <description>
	/// Usually, it should be called when you get the LoadCompleteDelegate raised with a success flag true.
	/// The webview will not be presented until you call this method on it.
	/// </description>
	public void Show() {
		UniWebViewPlugin.Show(gameObject.name);
		#if UNITY_EDITOR
		_webViewIntPtr = UniWebViewPlugin.GetIntPtr(gameObject.name);
		_hidden = false;
		#endif
	}

	/// <summary>
	/// Send a piece of javascript to the web page and evaluate (execute) it.
	/// </summary>
	/// <param name="javaScript">A single javascript method call to be sent to and executed in web page</param>
	/// <description>
	/// Call this method with a single js method caliing. The webview will evaluate (execute) the javascript.
	/// OnEvalJavaScriptFinished will be raised with the result when the js eval finished.
	/// This method can only accept a single function call. You can add your js function define
	/// in the html page directly, or add it by using AddJavaScript(string javaScript) method
	/// </description>
	public void EvaluatingJavaScript(string javaScript) {
		UniWebViewPlugin.EvaluatingJavaScript(gameObject.name, javaScript);
	}

	/// <summary>
	/// Add some javascript to the web page.
	/// </summary>
	/// <param name="javaScript">Some javascript code you want to add to the page.</param>
	/// <description>
	/// This method will execute the input javascript code without raising an 
	/// OnEvalJavaScriptFinished event. You can use this method to add some customized js
	/// function to the web page, then use EvaluatingJavaScript(string javaScript) to execute it.
	/// This method will add js in a async way in Android, so you should call it earlier than EvaluatingJavaScript
	/// </description>
	public void AddJavaScript(string javaScript) {
		UniWebViewPlugin.AddJavaScript(gameObject.name, javaScript);
	}

	/// <summary>
	/// Dismiss this UniWebView.
	/// </summary>
	/// <description>
	/// Calling this method on a UniWebView will hide it. Deprecated in ver1.2.4. Use Hide() instead!
	/// </description>
	[Obsolete("Dismiss() is deprecated in ver1.2.4. Use Hide() instead.")]
	public void Dismiss() {
		Hide();
	}

	/// <summary>
	/// Hide this UniWebView.
	/// </summary>
	/// <description>
	/// Calling this method on a UniWebView will hide it.
	/// </description>
	public void Hide() {
		#if UNITY_EDITOR
		_hidden = true;
		#endif
		UniWebViewPlugin.Dismiss(gameObject.name);
	}

	/// <summary>
	/// Clean the cache of this UniWebView.
	/// </summary>
	public void CleanCache() {
		UniWebViewPlugin.CleanCache(gameObject.name);
	}

	/// <summary>
	/// Clean the cookie using in the app.
	/// </summary>
	/// <param name="key">The key under which you want to clean the cache.</param>
	/// <description>
	/// Try to clean cookies under the specified key using in the app. 
	/// If you leave the key as null or send an empty string as key, all cache will be cleared.
	/// This method will clear the cookies in memory and try to
	/// sync the change to disk. The memory opreation will return
	/// right away, but the disk operation is async and could take some time.
	/// Caution, in Android, there is no way to remove a specified cookie.
	/// So this method will call setCookie method with the key to set
	/// it to an empty value instead. Please refer to Android 
	/// documentation on CookieManager for more information.
	/// </description>
	public void CleanCookie(string key = null) {
		UniWebViewPlugin.CleanCookie(gameObject.name, key);
	}

	/// <summary>
	/// Set the background of webview to transparent.
	/// </summary>
	/// <description>
	/// In iOS, there is a grey background in webview. If you don't want it, just call this method to set it transparent.
	/// </description>
	public void SetTransparentBackground(bool transparent = true) {
		UniWebViewPlugin.TransparentBackground(gameObject.name, transparent);
	}

	/// <summary>
	/// If the tool bar is showing or not.
	/// </summary>
	/// <description>
	/// This parameter is only available in iOS. In other platform, it will be always false.
	/// </description>
	public bool toolBarShow = false;

	/// <summary>
	/// Show the tool bar. The tool bar contains three buttons: go back, go forward and close webview.
	/// The tool bar is only available in iOS. In Android, you can use the back button of device to go back.
	/// </summary>
	/// <param name="animate">If set to <c>true</c>, show it with an animation.</param>
	public void ShowToolBar(bool animate) {
		#if UNITY_IOS && !UNITY_EDITOR
		toolBarShow = true;
		UniWebViewPlugin.ShowToolBar(gameObject.name,animate);
		#endif
	}

	/// <summary>
	/// Hide the tool bar. The tool bar contains three buttons: go back, go forward and close webview.
	/// The tool bar is only available in iOS. In Android, you can use the back button of device to go back.
	/// </summary>
	/// <param name="animate">If set to <c>true</c>, show it with an animation.</param>
	public void HideToolBar(bool animate) {
		#if UNITY_IOS && !UNITY_EDITOR
		toolBarShow = false;
		UniWebViewPlugin.HideToolBar(gameObject.name,animate);
		#endif
	}

	/// <summary>
	/// Set if a default spinner should show when loading the webpage.
	/// </summary>
	/// <description>
	/// The default value is true, which means a spinner will show when the webview is on, and it is loading some thing.
	/// The spinner contains a label and you can set a message to it. <see cref=""/>
	/// You can set it false if you do not want a spinner show when loading.
	/// </description>
	/// <param name="show">If set to <c>true</c> show.</param>
	public void SetShowSpinnerWhenLoading(bool show) {
		UniWebViewPlugin.SetSpinnerShowWhenLoading(gameObject.name, show);
	}

	/// <summary>
	/// Set the label text for the spinner showing when webview loading.
	/// The default value is "Loading..."
	/// </summary>
	/// <param name="text">Text.</param>
	public void SetSpinnerLabelText(string text) {
		UniWebViewPlugin.SetSpinnerText(gameObject.name, text);
	}


	/// <summary>
	/// Go to the previous page if there is any one.
	/// </summary>
	public void GoBack() {
		UniWebViewPlugin.GoBack(gameObject.name);
	}

	/// <summary>
	/// Go to the next page if there is any one.
	/// </summary>
	public void GoForward() {
		UniWebViewPlugin.GoForward(gameObject.name);
	}

	/// <summary>
	/// Adds the URL scheme. After be added, all link of this scheme will send a message when clicked.
	/// </summary>
	/// <param name="scheme">Scheme.</param>
	public void AddUrlScheme(string scheme) {
		UniWebViewPlugin.AddUrlScheme(gameObject.name, scheme);
	}

	/// <summary>
	/// Removes the URL scheme. After be removed, this kind of url will be handled by the webview.
	/// </summary>
	/// <param name="scheme">Scheme.</param>
	public void RemoveUrlScheme(string scheme) {
		UniWebViewPlugin.RemoveUrlScheme(gameObject.name, scheme);
	}

	#region Messages from native
	private void LoadComplete(string message) {
		bool loadSuc = string.Equals(message, "");
		bool hasCompleteListener = (OnLoadComplete != null);

		if (loadSuc) {
			if (hasCompleteListener) {
				OnLoadComplete(this, true, null);
			}
			if (autoShowWhenLoadComplete) {
				Show();
			}
		} else {
			Debug.LogWarning("Web page load failed: " + gameObject.name + "; url: " + url + "; error:" + message);
			if (hasCompleteListener) {
				OnLoadComplete(this, false, message);
			}
		}
	}

	private void LoadBegin(string url) {
		Debug.Log("Begin to load: " + url);
		if (OnLoadBegin != null) {
			OnLoadBegin(this, url);
		}
	}

	private void ReceivedMessage(string rawMessage) {
		UniWebViewMessage message = new UniWebViewMessage(rawMessage);
		if (OnReceivedMessage != null) {
			OnReceivedMessage(this,message);
		}
	}

	private void WebViewDone(string message) {
		bool destroy = true;
		if (OnWebViewShouldClose != null) {
			destroy = OnWebViewShouldClose(this);
		}
		if (destroy) {
			Hide();
			Destroy(this);
		}
	}

	private void WebViewKeyDown(string message) {
		int keyCode = Convert.ToInt32(message);
		if (OnReceivedKeyCode != null) {
			OnReceivedKeyCode(this, keyCode);
		}
	}

	private void EvalJavaScriptFinished(string result) {
		if (OnEvalJavaScriptFinished != null) {
			OnEvalJavaScriptFinished(this, result);
		}
	}

	private IEnumerator LoadFromJarPackage(string jarFilePath) {
		WWW stream = new WWW(jarFilePath);
		yield return stream;
		if (stream.error != null) {
			if (OnLoadComplete != null) {
				OnLoadComplete(this,false,stream.error);
			}
			yield break;
		} else {
			LoadHTMLString(stream.text, "");
		}
	}
	
	#endregion

	#region Life Cycle
	void Awake() {
		_currentGUID = System.Guid.NewGuid().ToString();
		gameObject.name = gameObject.name + _currentGUID;
		UniWebViewPlugin.Init(gameObject.name,
		                      this.insets.top,
		                      this.insets.left,
		                      this.insets.bottom,
		                      this.insets.right);
		#if UNITY_EDITOR
		CreateTexture(this.insets.left,
	    	          this.insets.bottom,
	        	      Screen.width - this.insets.left - this.insets.right,
	            	  Screen.height - this.insets.top - this.insets.bottom
	              	);
		#endif
	}

	void Start() {
		if (loadOnStart) {
			Load();
		}
	}

	private void OnDestroy() {
		#if UNITY_EDITOR
		Clean();
		#endif
		UniWebViewPlugin.Destroy(gameObject.name);
		gameObject.name = gameObject.name.Replace(_currentGUID, "");
	}

	#endregion

	#region UnityEditor Debug
	#if UNITY_EDITOR
	private Rect _webViewRect;
	private Texture2D _texture;
	private string _inputString;
	private IntPtr _webViewIntPtr;
	private bool _hidden;

	private void CreateTexture(int x, int y, int width, int height) {
		if (Application.platform == RuntimePlatform.OSXEditor) {
			int w = 1;
			int h = 1;
			while (w < width) { w <<= 1; }
			while (h < height) { h <<= 1; }
			_webViewRect = new Rect(x, y, width, height);
			_texture = new Texture2D(w, h, TextureFormat.ARGB32, false);
		}
	}

	private void Clean() {
		if (Application.platform == RuntimePlatform.OSXEditor) {
			_webViewIntPtr = IntPtr.Zero;
			Destroy(_texture);
			_texture = null;
		}
	}

	private void Update() {
		if (Application.platform == RuntimePlatform.OSXEditor) {
			_inputString += Input.inputString;
		}
	}

	private void OnGUI()
	{
		if (Application.platform == RuntimePlatform.OSXEditor) {
			if (_webViewIntPtr != IntPtr.Zero && !_hidden) {
				Vector3 pos = Input.mousePosition;
				bool down = Input.GetMouseButton(0);
				bool press = Input.GetMouseButtonDown(0);
				bool release = Input.GetMouseButtonUp(0);
				float deltaY = Input.GetAxis("Mouse ScrollWheel");
				bool keyPress = false;
				string keyChars = "";
				short keyCode = 0;
				if (_inputString.Length > 0) {
					keyPress = true;
					keyChars = _inputString.Substring(0, 1);
					keyCode = (short)_inputString[0];
					_inputString = _inputString.Substring(1);
				}

				UniWebViewPlugin.InputEvent(gameObject.name, 
				                            (int)(pos.x - _webViewRect.x), (int)(pos.y - _webViewRect.y), deltaY,
				                            down, press, release, keyPress, keyCode, keyChars,
				                            _texture.GetNativeTextureID());
				GL.IssuePluginEvent((int)_webViewIntPtr);
				Matrix4x4 m = GUI.matrix;
				GUI.matrix = Matrix4x4.TRS(new Vector3(0, Screen.height, 0),
				                           Quaternion.identity, new Vector3(1, -1, 1));
				GUI.DrawTexture(_webViewRect, _texture);
				GUI.matrix = m;
			}
		}
	}
	#endif
	#endregion
}
#endif