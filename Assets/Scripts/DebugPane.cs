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

	private string _roomVarName;
	private string _roomVarValue;
	private List<LogMessage> _currentLog;
	private bool _isLogVisible;
	private bool _areVarsVisible;
	private Vector2 _currentScrollPosition;

	void Start() {
		_roomVarName = "";
		_roomVarValue = "";
		_isLogVisible = false;
		_areVarsVisible = false;
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
			
			GUILayout.Label ("Value: ");
			_roomVarValue = GUILayout.TextField(_roomVarValue, 20, GUILayout.Width (100));
			if(GUILayout.Button ("Set Variable")) {
				setRoomVariable();
			}
			GUILayout.Space (10);
			
			if(GUILayout.Button("Toggle Room Vars")) {
				_areVarsVisible = !_areVarsVisible;
			}
			GUILayout.Space (10);
		}
		
		if(GUILayout.Button("Toggle Log")) {
			_isLogVisible = !_isLogVisible;
		}
		GUILayout.EndHorizontal();
		
		if(_isLogVisible) {
			logGUI();
		}
		
		if(_areVarsVisible && SmartFoxConnection.IsInitialized) {
			roomVarGUI();
		}
	}
	
	private void setRoomVariable() {
		if(_roomVarName == "" || _roomVarValue == "")
			return;
		RoomVariable roomVar;
		var typeString = _roomVarValue[0];
		switch(typeString) {
		case 'f':
			roomVar = new SFSRoomVariable(_roomVarName, float.Parse (_roomVarValue.Substring(1)));
			break;
		case 's':
			roomVar = new SFSRoomVariable(_roomVarName, _roomVarValue.Substring(1));
			break;
		default:
			Debug.Log ("Unhandled prefix");
			return;
		}
		SmartFoxConnection.Connection.Send (new SetRoomVariablesRequest(new [] {roomVar}, SmartFoxConnection.Connection.LastJoinedRoom));
	}
	
	//FIXME: Give better information for SFSArrays and other compound data types.
	private void roomVarGUI() {
		GUILayout.BeginVertical("box");
		if(SmartFoxConnection.Connection.LastJoinedRoom == null) return;
		var roomVars = SmartFoxConnection.Connection.LastJoinedRoom.GetVariables();
		var style = new GUIStyle();
		style.fontSize = 12;
		style.normal.textColor = Color.white;
		foreach(var roomVar in roomVars) {
			GUILayout.Label (string.Format ("{0}: {1}", roomVar.Name, roomVar.Value.ToString()), style);
		}
		GUILayout.EndVertical();
	}
	
	private void logGUI() {
		GUILayout.BeginVertical("box");
		_currentScrollPosition = GUILayout.BeginScrollView(_currentScrollPosition);
		foreach(var log in _currentLog) {
			var style = new GUIStyle();
			style.fontSize = 12;
			style.normal.textColor = getLogTypeColor(log.type);
			GUILayout.Label (log.message, style);
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}
	
	private Color getLogTypeColor(LogType type) {
		switch(type) {
		case LogType.Exception: return Color.red;
		case LogType.Error: return Color.red;
		case LogType.Warning: return Color.yellow;
		default: return Color.white;
		}
	}
}
