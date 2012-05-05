using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Variables;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using Sfs2X.Logging;


public class WaitScreenScript : MonoBehaviour {
	public const int MIN_USERS = 3;

	private string newMessage = "";
	private ArrayList messages = new ArrayList();
	private List<string> redTeam = new List<string>();
	private List<string> blueTeam = new List<string>();
	private Vector2 roomScrollPosition, userScrollPosition, chatScrollPosition;
	private int screenW;
	private SmartFox smartFox;
	private bool isBlueTeam = false;
	
	void Start () {
		smartFox = SmartFoxConnection.Connection;
		AddEventListeners();
		
		UpdateTeamLists();
		
		SFSArray myTeamUpdate = new SFSArray();
		
		if (blueTeam.Count < redTeam.Count) {
			isBlueTeam = true;	
		}
		
		List<RoomVariable> roomVars = new List<RoomVariable>();
		
		if (isBlueTeam)
		{
			blueTeam.Add(smartFox.MySelf.Name);
			foreach(string name in blueTeam)
			{
				myTeamUpdate.AddUtfString(name);
			}
			roomVars.Add(new SFSRoomVariable("blue", myTeamUpdate));
			
			if (smartFox.LastJoinedRoom.GetVariable("blueRobot").GetStringValue() == "")
			{
				roomVars.Add(new SFSRoomVariable("blueRobot", smartFox.MySelf.Name));
			}
		}
		else
		{
			redTeam.Add(smartFox.MySelf.Name);
			foreach(string name in redTeam)
			{
				myTeamUpdate.AddUtfString(name);
			}
			roomVars.Add(new SFSRoomVariable("red", myTeamUpdate));
			
			if (smartFox.LastJoinedRoom.GetVariable("redRobot").GetStringValue() == "")
			{
				roomVars.Add(new SFSRoomVariable("redRobot", smartFox.MySelf.Name));
			}
		}
		
		List<RoomVariable> variables = smartFox.LastJoinedRoom.GetVariables();
		
		foreach(RoomVariable roomVar in variables)
		{
			Debug.Log(roomVar.Name);
		}
		
		smartFox.Send(new SetRoomVariablesRequest(roomVars,smartFox.LastJoinedRoom));
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
	
	void OnGUI() 
	{
		screenW = Screen.width;
				
		if (smartFox.LastJoinedRoom.ContainsVariable("blueRobot"))
		{
			GUI.Label(new Rect(100, 100, 300, 300), "Blue: " + smartFox.LastJoinedRoom.GetVariable("blueRobot").GetStringValue());
		}
		if (smartFox.LastJoinedRoom.ContainsVariable("redRobot"))
		{
			GUI.Label(new Rect(100, 200, 300, 300), "Red: " + smartFox.LastJoinedRoom.GetVariable("redRobot").GetStringValue());
		}
		
		DrawUsersGUI();
		DrawChatGUI();
		DrawButtons();
	}
	
	private void AddEventListeners() {
		smartFox.AddEventListener(SFSEvent.PUBLIC_MESSAGE, OnPublicMessage);
		smartFox.AddEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, OnRoomVariableUpdate);
		smartFox.AddEventListener(SFSEvent.OBJECT_MESSAGE, OnObjectMessage);
		//smartFox.AddEventListener(SFSEvent.ROOM_JOIN, OnJoinRoom);
	}
	
	void OnPublicMessage(BaseEvent evt) {
		try {
			string message = (string)evt.Params["message"];
			User sender = (User)evt.Params["sender"];
			messages.Add(sender.Name +": "+ message);
			
			chatScrollPosition.y = Mathf.Infinity;
			Debug.Log("User " + sender.Name + " said: " + message); 
		}
		catch (Exception ex) {
			Debug.Log("Exception handling public message: "+ex.Message+ex.StackTrace);
		}
	}
	
	void OnRoomVariableUpdate(BaseEvent evt)
	{
		UpdateTeamLists();
	}
	
	public void OnObjectMessage(BaseEvent evt)
	{
		ISFSObject message = (SFSObject)evt.Params["message"];
		
		if (message.GetUtfString("type") == "everyoneJoin") {
			Application.LoadLevel("GameScene");
		}
	}
	
	private void UpdateTeamLists()
	{
		ISFSArray arr = new SFSArray();
		
		redTeam.Clear();
		blueTeam.Clear();
		RoomVariable redVar = smartFox.LastJoinedRoom.GetVariable("red");
		RoomVariable blueVar = smartFox.LastJoinedRoom.GetVariable("blue");
		if(redVar != null) {
			arr = redVar.GetSFSArrayValue();
			for(int i = 0; i < arr.Size(); i++) {
				redTeam.Add(arr.GetUtfString(i));
			}
		}
		if(blueVar != null) {
			arr = blueVar.GetSFSArrayValue();
			for(int i = 0; i < arr.Size(); i++) {
				blueTeam.Add(arr.GetUtfString(i));
			}
		}
	}
	
	private void DrawButtons()
	{
		if (GUI.Button (new Rect (screenW - 200, 470, 180, 30), "Leave Room")) 
		{
			smartFox.Send(new JoinRoomRequest("The Lobby"));
		}
		if (redTeam.Count + blueTeam.Count >= MIN_USERS && IsLowestID())
		{
			if (GUI.Button (new Rect (screenW - 350, 400, 100, 100), "Start Game"))
			{
				ISFSObject sendJoinMessage = new SFSObject();
				sendJoinMessage.PutUtfString("type", "everyoneJoin");
				smartFox.Send(new ObjectMessageRequest(sendJoinMessage, null, smartFox.LastJoinedRoom.UserList));
			}
		}
	}
	
	private void DrawUsersGUI(){
		GUI.Box (new Rect (screenW - 200, 80, 180, 170), "Red Team");
		GUILayout.BeginArea (new Rect (screenW - 190, 110, 150, 160));
			userScrollPosition = GUILayout.BeginScrollView (userScrollPosition, GUILayout.Width (150), GUILayout.Height (150));
			GUILayout.BeginVertical ();
			
				foreach (string user in redTeam) {
					GUILayout.Label (user); 
				}
		
			GUILayout.EndVertical ();
			GUILayout.EndScrollView ();
		GUILayout.EndArea ();
		
		GUI.Box (new Rect (screenW - 200, 280, 180, 170), "Blue Team");
		GUILayout.BeginArea (new Rect (screenW - 190, 310, 150, 160));
			userScrollPosition = GUILayout.BeginScrollView (userScrollPosition, GUILayout.Width (150), GUILayout.Height (150));
			GUILayout.BeginVertical ();
			
				foreach (string user in blueTeam) {
					GUILayout.Label (user); 
				}
		
			GUILayout.EndVertical ();
			GUILayout.EndScrollView ();
		GUILayout.EndArea ();
		
	}
	
	private void DrawChatGUI(){
		GUI.Box(new Rect(10, 80, 470, 390), "Chat");

		GUILayout.BeginArea (new Rect(20, 110, 450, 350));
			chatScrollPosition = GUILayout.BeginScrollView (chatScrollPosition, GUILayout.Width (450), GUILayout.Height (350));
				GUILayout.BeginVertical();
					foreach (string message in messages) {
						//this displays text from messages arraylist in the chat window
						GUILayout.Label(message);
				}
				GUILayout.EndVertical();
			GUILayout.EndScrollView ();
		GUILayout.EndArea();
		
		// Send message
		newMessage = GUI.TextField(new Rect(10, 480, 370, 20), newMessage, 50);
		if (GUI.Button(new Rect(390, 478, 90, 24), "Send")  || (Event.current.type == EventType.keyDown && Event.current.character == '\n'))
		{
			smartFox.Send( new PublicMessageRequest(newMessage) );
			newMessage = "";
		}
	}
	
	private bool IsLowestID()
	{
		int lowestUserID = Int32.MaxValue;
		int myID = smartFox.MySelf.GetPlayerId(smartFox.LastJoinedRoom);
		
		foreach (User u in smartFox.LastJoinedRoom.UserList) {
			int userIDToCheck = u.GetPlayerId(smartFox.LastJoinedRoom);
			if (userIDToCheck < lowestUserID)
				lowestUserID = userIDToCheck;
		}
		return myID == lowestUserID;
	}
}
