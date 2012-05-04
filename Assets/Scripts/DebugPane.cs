using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Variables;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using Sfs2X.Logging;

struct LogMessage {
	public LogType type;
	public string message;
	
	public LogMessage(LogType t, string m) {
		type = t;
		message = m;
	}
}

public class DebugPane : MonoBehaviour {
	public int MAX_LOG = 100;

	private SmartFox smartFox;
	
	private string _roomVarName;
	private string _roomVarValue;
	private List<LogMessage> _currentLog;
	private bool _isLogVisible;
	private Vector2 _currentScrollPosition;

	// Use this for initialization
	void Start() {
		smartFox = SmartFoxConnection.Connection;
		_roomVarName = "";
		_roomVarValue = "";
		_isLogVisible = false;
		_currentLog = new List<LogMessage>(MAX_LOG);
	}
	
	private void handleLog(string log, string stackTrace, LogType type) {
		_currentScrollPosition.y = Mathf.Infinity;
		if(_currentLog.Count == MAX_LOG)
			_currentLog.RemoveAt(0);
		_currentLog.Add (new LogMessage(type, string.Format ("[{0}] {1}\n{2}", type.ToString(), log, stackTrace)));
	}
	
	void OnDisable() {
		Application.RegisterLogCallback(null);
	}
	
	// Update is called once per frame
	void Update() {
		//TODO: There seem to be issues with this method. See if it's a bottleneck
		Application.RegisterLogCallback(handleLog);
	}
	
	void OnGUI() {
		//Appear in front of other controls
		GUI.depth = 0;
		GUILayout.BeginHorizontal("box");
		//Note: this doesn't work in the lobby because of the trickery it uses.
		if(GUILayout.Button ("Reload")) {
			Application.LoadLevel(Application.loadedLevel);
		}
		GUILayout.Space(10);
		
		if(SmartFoxConnection.IsInitialized) {
			GUILayout.Label ("Room Variable: ");
			_roomVarName = GUILayout.TextField(_roomVarName, 20, GUILayout.Width (100));
			GUILayout.Space (10);
			
			GUILayout.Label ("Value (prefix with type): ");
			_roomVarValue = GUILayout.TextField(_roomVarValue, 20, GUILayout.Width (100));
			if(GUILayout.Button ("Set Variable")) {
				setRoomVariable();
			}
			GUILayout.Space (10);
		}
		
		if(GUILayout.Button("Toggle Log")) {
			_isLogVisible = !_isLogVisible;
		}
		GUILayout.EndHorizontal();
		
		if(_isLogVisible) {
			GUILayout.BeginVertical("box");
			windowGUI(0);
			GUILayout.EndVertical();
		}
	}
	
	private void setRoomVariable() {
		//TODO: Implement
		return;
	}
	
	private void windowGUI(int id) {
		_currentScrollPosition = GUILayout.BeginScrollView(_currentScrollPosition);
		foreach(var log in _currentLog) {
			var style = new GUIStyle();
			style.fontSize = 14;
			style.normal.textColor = getLogTypeColor(log.type);
			GUILayout.Label (log.message, style);
		}
		GUILayout.EndScrollView();
	}
	
	private Color getLogTypeColor(LogType type) {
		switch(type) {
			case LogType.Error: return Color.red;
			case LogType.Warning: return Color.yellow;
			default: return Color.white;
		}
	}
}
