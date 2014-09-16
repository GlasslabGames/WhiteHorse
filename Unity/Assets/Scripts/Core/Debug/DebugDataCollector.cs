using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using GlassLab.Core.Serialization;

public class DebugDataCollector : SingletonBehavior<DebugDataCollector> {

	public UILabel Label;
	public int MaxLines = 200;
	public int MaxCharacters = 8000;
	public int MaxCharactersPerLine = 200;
	public bool ShowLineNumber = true;
  public float SaveToFileEverySeconds = 5f;

  public const string DEBUG_LOG_PREFIX = "mgo_debug_log";

	public enum DebugMessageType{
		Telemetry,
		Achievement,
		SDKRequest,
		Info,
		DebugLog,
		Setting,
    Save
	};
	
	private DebugMessageType m_showingType = DebugMessageType.Telemetry;
	public DebugMessageType ShowingType{
		get{ return m_showingType;}
		set{
			m_showingType = value;
			switch(m_showingType)
			{
			case DebugMessageType.Telemetry:
				m_printing = m_telemetry;
				break;
			case DebugMessageType.Achievement:
				m_printing = m_achievement;
				break;
			case DebugMessageType.SDKRequest:
				m_printing = m_sdkRequest; // !!!!!!!!! need update
				break;
			case DebugMessageType.Info:
				m_printing = m_info;
				break;
			case DebugMessageType.DebugLog:
        m_printing = m_debugLogs;
				break;
			case DebugMessageType.Setting:
				m_printing = m_blank; // Set label to blank
				break;
      case DebugMessageType.Save:
        m_printing = m_blank;
        break;
			default:
				m_printing = m_telemetry;
				break;
			}
			m_messageTail = m_printing.Count;
			m_messageHead = SmallestPossibleHead(m_messageTail);
		}
	}
	
	private List<string> m_telemetry = new List<string>();
	private List<string> m_achievement = new List<string>();
	private List<string> m_sdkRequest = new List<string>();
	private List<string> m_info = new List<string>();
	private List<string> m_debugLogs = new List<string>();
	private List<string> m_blank = new List<string>();

	private Dictionary<string, string> m_infoMap = new Dictionary<string, string>();

	private List<string> m_printing;
	private bool m_isUpdateEnabled = false;
	private int m_messageHead = 0;
	private int m_messageTail = 0;

  // Save log to local file is causing issues, disabled for now
  private bool m_isSaveEnabled = false;
  private bool m_hasLogToSave = false;
  private string m_leftOver = "";
  private const int m_leftOverBufferLength = 1000000;
  private float m_saveTick = 0f;

  private bool m_updateFlag = true;
  private object _lock = new object(); // Lock prevents conflicts in multi-threaded environments

  override protected void Awake() {
      base.Awake();
		ShowingType = DebugMessageType.Telemetry;
	}

	// Use this for initialization
  override protected void Start()
  {
      base.Start();
		ChangeButtonsColor(ShowingType);
		CollectInfo();
    //StartCoroutine(UpdateSDKRequest());
	}
	
	// Update is called once per frame
	void Update () {
    // Show lastest content
    if (m_updateFlag)
    {
      m_updateFlag = false;
      ShowLatest();
    }

    // Save log into file
    m_saveTick += Time.deltaTime;
    if (m_saveTick >= SaveToFileEverySeconds && m_hasLogToSave)
    {
      SaveLogToFile();
      m_hasLogToSave = false;
      m_saveTick = 0f;
    }
	}

	void OnEnable() {
		GlasslabSDK.TelemetryOutput += OnTelemOutput;
	}
	
	void OnDisable() {
		GlasslabSDK.TelemetryOutput -= OnTelemOutput;

    if (m_hasLogToSave)
    {
      SaveLogToFile();
      m_hasLogToSave = false;
    }
	}

    override protected void OnDestroy()
    {
    if (m_hasLogToSave)
    {
      SaveLogToFile();
      m_hasLogToSave = false;
    }
    base.OnDestroy();
  }

	void CollectInfo() {
		m_infoMap.Add("SDK_SERVER_URI", PegasusManager.SDK_SERVER_URI);
		m_infoMap.Add("SDK_CLIENT_ID", PegasusManager.SDK_CLIENT_ID);
		m_infoMap.Add("SDK_GAME_NAME", PegasusManager.SDK_GAME_NAME);
		m_infoMap.Add("SDK_GAME_LEVEL", PegasusManager.SDK_GAME_LEVEL);

		m_infoMap.Add("BundleIdentifier", GLResourceManager.InstanceOrCreate.GetProjectBundleID());
		m_infoMap.Add("BundleVersion", GLResourceManager.InstanceOrCreate.GetVersionString());

    m_infoMap.Add("CurrentAccount", AccountManager.InstanceOrCreate.GetCurrentAccount());

		m_info.Clear();
		foreach (var pair in m_infoMap)
		{
			m_info.Add(pair.Key + ": " + pair.Value);
		}
	}

	void OnTelemOutput(string output) {
    lock (_lock)
    {
  		CompareInfo myCompare = CultureInfo.InvariantCulture.CompareInfo;
  		if (myCompare.IsPrefix(output, "---- ACHIEVEMENT", CompareOptions.IgnoreCase))
  			AddAchievement(output);
  		else
  			AddTelem(output);
    }
	}
  
  IEnumerator UpdateSDKRequest() {
    while (true)
    {
      string newLog = GlasslabSDK.Instance.PopLogQueue();
      if (newLog != null && newLog != "")
        AddSDKRequest(newLog);
      yield return new WaitForSeconds(1f);
    }
  }

	public void AddTelem(string message) {
		//Debug.Log("AddTelem: " + message);
		m_telemetry.Add(CutString(message));
		if (ShowingType == DebugMessageType.Telemetry && m_isUpdateEnabled && gameObject.activeInHierarchy){
			//ShowLatest();
      m_updateFlag = true;
		}
	}

	public void AddAchievement(string message) {
		//Debug.Log("AddAchievement: " + message);
		m_achievement.Add(CutString(message));
		if (ShowingType == DebugMessageType.Achievement && m_isUpdateEnabled && gameObject.activeInHierarchy) {
      //ShowLatest();
      m_updateFlag = true;
		}
	}

  public void AddSDKRequest(string message) {
    m_sdkRequest.Add(CutString(message));
    if (ShowingType == DebugMessageType.SDKRequest && m_isUpdateEnabled && gameObject.activeInHierarchy) {
      //ShowLatest();
      m_updateFlag = true;
    }
  }

	public void AddInfo(string key, string message) {
		//Debug.Log("AddInfo: " + message);
		m_infoMap[key] = message;
		m_info.Clear();
		foreach (var pair in m_infoMap)
		{
			m_info.Add(pair.Key + ": " + pair.Value);
		}
		if (ShowingType == DebugMessageType.Info && m_isUpdateEnabled && gameObject.activeInHierarchy) {
      //ShowLatest();
      m_updateFlag = true;
		}
	}

	public void AddLog(string message) {
		//Debug.Log("AddDebugLog: " + message);
		m_debugLogs.Add(CutString(message));
    if (ShowingType == DebugMessageType.DebugLog && m_isUpdateEnabled && gameObject.activeInHierarchy) {
      //ShowLatest();
      m_updateFlag = true;
    }
    
    // Add log to save buffer
    m_leftOver = m_leftOver + FormatNumber(m_debugLogs.Count, 8) + message + System.Environment.NewLine;
    if (m_leftOver.Length > m_leftOverBufferLength)
      m_leftOver = "At least " + m_leftOver.Length + " characters went missing!" + System.Environment.NewLine;
    m_hasLogToSave = true;
	}

  void SaveLogToFile()
  {
    if (DebugSystemManager.DebugLogNum >= 0 && m_isSaveEnabled)
    {
      string dir = Application.persistentDataPath + "/";
      if (Directory.Exists(dir))
      {
        string fileName = dir + DEBUG_LOG_PREFIX + DebugSystemManager.DebugLogNum.ToString();
        try{
          File.AppendAllText(fileName, m_leftOver);
        }
        catch(IOException ex){
          m_debugLogs.Add("Save log to file failed: " + ex.ToString());
        }
        finally{
          m_leftOver = "";
        }
      }
    }
    else
    {
      m_leftOver = "";
    }
  }

	private string CutString(string message)
	{
		int length = Mathf.Min(message.Length, MaxCharactersPerLine);
		string str = message.Substring(0, length);
		if (length < message.Length)
			str += "......";
		return str;
	}

	/// <summary>
	/// Changes the label content to another type.
	/// </summary>
	/// <returns><c>true</c>, if type is different from the current type, <c>false</c> otherwise.</returns>
	/// <param name="type">Type name.</param>
	public bool ChangeDisplay(DebugMessageType type) {
		if (type.Equals(ShowingType))
			return false;
		//ShowLatestOfType(type);
    ShowingType = type;
    m_updateFlag = true;
		return true;
	}

	/// <summary>
	/// Shows the latest content of current type.
	/// </summary>
	public void ShowLatest() {
		ShowLatestOfType(ShowingType);
	}

	/// <summary>
	/// Show lastest content of a type of messages.
	/// </summary>
	/// <param name="type">Type name.</param>
	public void ShowLatestOfType(DebugMessageType type) {
		ShowingType = type;
		ReplaceLabelContent(m_printing, m_messageHead, m_messageTail);
		ChangeButtonsColor(type);
		ResetLabelPosition(Label);
	}

	void ChangeButtonsColor(DebugMessageType type) {
		DebugSelectButton[] buttons = GetComponentsInChildren<DebugSelectButton>(true);
		if (buttons != null)
		{
			foreach(var button in buttons)
			{
				if (button.MessageType == type) button.SetButtonColor(Color.red);
				else button.SetButtonColor(Color.white);
			}
		}
	}

	public void LoadMoreLines(bool isToTop)
	{
		//Debug.Log("Load to top: " + isToTop);
		if (!CheckMoreLinesExist(isToTop)) return;
		if (isToTop)
		{
			LoadLinesToTop();
		}
		else
		{
			LoadLinesToBottom();
		}
	}
	
	bool CheckMoreLinesExist(bool isToTop)
	{
		if (isToTop && m_messageHead != 0)
			return true;
		else if (!isToTop && m_messageTail < m_printing.Count)
			return true;
		return false;
	}

	void LoadLinesToTop()
	{
		int shift = (m_messageTail - m_messageHead) / 2;
		int newHead = Mathf.Max(m_messageHead - shift, 0);
		shift = m_messageHead - newHead;
		int newTail = m_messageTail - shift;
		newHead = SmallestPossibleHead(newTail);
		if (CheckHeadTailValid(newHead, newTail))
		{
			SetLabelPivot(UILabel.Pivot.BottomLeft);
			AddLabelContent(m_printing, newHead, m_messageHead, true);
			//Label.text = "h\ne\nl\nl\no\nw\no\nr\nl\nd\n" + Label.text;
			SetLabelPivot(UILabel.Pivot.TopLeft);
			ReplaceLabelContent(m_printing, newHead, newTail);
			//Label.text = Label.text + "End test\n";
			SetLabelPivot(UILabel.Pivot.BottomLeft);
			CheckLabelColliderSize(Label);
			m_messageHead = newHead;
			m_messageTail = newTail;
		}
	}

	void LoadLinesToBottom()
	{
		int shift = (m_messageTail - m_messageHead) / 2;
		int newTail = Mathf.Min(m_messageTail + shift, m_printing.Count);
		shift = newTail - m_messageTail;
		int newHead = m_messageHead + shift;
		newTail = LarestPossibleTail(newHead);
		if (CheckHeadTailValid(newHead, newTail))
		{
			SetLabelPivot(UILabel.Pivot.TopLeft);
			AddLabelContent(m_printing, m_messageTail, newTail, false);
			//Label.text = Label.text + "h\ne\nl\nl\no\nw\no\nr\nl\nd\n";
			SetLabelPivot(UILabel.Pivot.BottomLeft);
			ReplaceLabelContent(m_printing, newHead, newTail);
			//Label.text = "End test\n" + Label.text;
			CheckLabelColliderSize(Label);
			m_messageHead = newHead;
			m_messageTail = newTail;
		}
	}

	bool CheckHeadTailValid(int head, int tail)
	{
		if (head < 0 || tail > m_printing.Count
		    || head > tail)
		{
			//Debug.LogError("[DebugDataCollector] head: " + head + ", tail: " + tail);
			return false;
		}
		return true;
	}

//	int CountCharacters(int head, int tail)
//	{
//		int total = 0;
//		tail = Mathf.Min(tail, m_printing.Count);
//		for (int i = head; i < tail; i++)
//		{
//			total += m_printing[i].Length;
//		}
//		return total;
//	}
	
	int SmallestPossibleHead(int tail)
	{
		if (tail > m_printing.Count || tail < 0)
			return tail + 1;
		int charCounter = 0;
		int minHead = Mathf.Max(0, tail - MaxLines);
		for (int i = tail - 1; i >= minHead; i--)
		{
			charCounter += m_printing[i].Length;
			if (charCounter > MaxCharacters)
			{
				return i + 1;
			}
		}
		return minHead;
	}

	int LarestPossibleTail(int head)
	{
		if (head < 0 || head > m_printing.Count)
			return head - 1;
		int charCounter = 0;
		int maxTail = Mathf.Min(m_printing.Count, head + MaxLines);
		for (int i = head; i < maxTail; i++)
		{
			charCounter += m_printing[i].Length;
			if (charCounter > MaxCharacters)
			{
				return i;
			}
		}
		return maxTail;
	}

	/// <summary>
	/// Adds the content of the label.
	/// </summary>
	/// <param name="messages">List of messages.</param>
	/// <param name="head">Head position of add content in messages.</param>
	/// <param name="tail">Tail position of add content in messages. Will set to count of messages if -1</param>
	/// <param name="isToTop">If set to <c>true</c> add content before previous content.</param>
	void AddLabelContent(List<string> messages, int head = 0, int tail = -1, bool isToTop = false)
	{
		if (Label == null) return;
		if (tail == -1)
			tail = messages.Count;
		List<string> displayMsg = messages.GetRange(head, tail - head);
		if (ShowLineNumber)
		{
			for (int i = 0; i < displayMsg.Count; i++)
				displayMsg[i] = FormatNumber(head + i + 1) + "" + displayMsg[i];
		}
		if (isToTop)
			Label.text = string.Join("\n", displayMsg.ToArray()) + Label.text;
		else Label.text = Label.text + string.Join("\n", displayMsg.ToArray());
		CheckLabelColliderSize(Label);
	}
	
	/// <summary>
	/// Replaces the content of the label.
	/// </summary>
	/// <param name="messages">List of messages.</param>
	/// <param name="head">Head position of add content in messages.</param>
	/// <param name="tail">Tail position of add content in messages. Will set to count of messages if -1</param>
	void ReplaceLabelContent(List<string> messages, int head = 0, int tail = -1)
	{
		if (Label == null) return;
		if (tail == -1)
			tail = messages.Count;
		List<string> displayMsg = messages.GetRange(head, tail - head);
		if (ShowLineNumber)
		{
			for (int i = 0; i < displayMsg.Count; i++)
				displayMsg[i] = FormatNumber(head + i + 1) + "" + displayMsg[i];
		}
		if (displayMsg == null || displayMsg.Count == 0)
			Label.text = "NOTHING TO SHOW!";
		else
			Label.text = string.Join("\n", displayMsg.ToArray());
		if (IsNonDisplayType(ShowingType))
			Label.text = " ";
		CheckLabelColliderSize(Label);
	}

  bool IsNonDisplayType(DebugMessageType type)
  {
    return type == DebugMessageType.Setting || type == DebugMessageType.Save;
  }

	string FormatNumber(int n, int totalLength = 10)
	{
		string str = n.ToString();
		int len = str.Length;
    for (int i = 0; i < totalLength - len; i++)
			str += " ";
		return str;
	}

	public void SetLabelPivot(UILabel.Pivot pivot)
	{
		if (Label != null)
			Label.pivot = pivot;
	}

	public bool SwitchUpdateMessage()
	{
		m_isUpdateEnabled = !m_isUpdateEnabled;
		return m_isUpdateEnabled;
	}

	public bool SyncUpdateMessage()
	{
		return m_isUpdateEnabled;
	}

	/// <summary>
	/// Adjust the size of the label collider. Call this whenever change label content
	/// </summary>
	/// <param name="label">Label</param>
	private void CheckLabelColliderSize(UILabel label)
	{
		DebugLabelControl labelControl = label.gameObject.GetComponent<DebugLabelControl>();
		if (labelControl != null)
			labelControl.AdjustColliderSize();
	}

	private void ResetLabelPosition(UILabel label)
	{
		DebugLabelControl labelControl = label.gameObject.GetComponent<DebugLabelControl>();
		if (labelControl != null)
			labelControl.ResetScrollViewPosition();
	}
}
