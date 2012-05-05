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
	public int MIN_USERS = 3;
	public int rounds = 3;
	
	private string _roundsString;

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
		
		_roundsString = rounds.ToString();
		
		UpdateTeamLists();
		
		SFSArray myTeamUpdate = new SFSArray();
		
		if (blueTeam.Count < redTeam.Count) {
			isBlueTeam = true;	
		}
		
		RoomVariable myRoomVar;
		List<RoomVariable> roomVars = new List<RoomVariable>();
		
		if (isBlueTeam)
		{
			blueTeam.Add(smartFox.MySelf.Name);
			foreach(string name in blueTeam)
				myTeamUpdate.AddUtfString(name);
			roomVars.Add(new SFSRoomVariable("blue", myTeamUpdate));
			if(!smartFox.LastJoinedRoom.ContainsVariable("blueRobot"))
				roomVars.Add(new SFSRoomVariable("blueRobot", smartFox.MySelf.Name));
			
		}
		else
		{
			redTeam.Add(smartFox.MySelf.Name);
			foreach(string name in redTeam)
				myTeamUpdate.AddUtfString(name);
			roomVars.Add(new SFSRoomVariable("red", myTeamUpdate));
			if(!smartFox.LastJoinedRoom.ContainsVariable("redRobot"))
				roomVars.Add(new SFSRoomVariable("redRobot", smartFox.MySelf.Name));

		}
		
		smartFox.Send( new SetRoomVariablesRequest(roomVars));
		
	}
	
	// Update is called once per frame
	void Update () 
	{
	}
	
	void OnGUI() 
	{
		screenW = Screen.width;
				
		DrawUsersGUI();
		DrawChatGUI();
		DrawButtons();
		if(IsLowestID())
			DrawHostOptions();
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
			Debug.LogError("Exception handling public message: "+ex.Message+ex.StackTrace);
		}
	}
	
	void OnRoomVariableUpdate(BaseEvent evt) {
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
		redTeam.Clear();
		blueTeam.Clear();
		RoomVariable redVar = smartFox.LastJoinedRoom.GetVariable("red");
		RoomVariable blueVar = smartFox.LastJoinedRoom.GetVariable("blue");
		if(redVar != null) {
			ISFSArray arr = redVar.GetSFSArrayValue();
			for(int i = 0; i < arr.Size(); i++) {
				redTeam.Add(arr.GetUtfString(i));
			}
		}
		if(blueVar != null) {
			ISFSArray arr = blueVar.GetSFSArrayValue();
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
	}
	
	private void DrawHostOptions() {
		GUILayout.BeginArea(new Rect(490, 80, 300, 500));
		GUILayout.BeginVertical("box");
		GUILayout.Label("Host Options");
		GUILayout.Space(10);
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Rounds: ");
		_roundsString = GUILayout.TextField(_roundsString, 25, GUILayout.Width(70));
		GUILayout.EndHorizontal();
		
		int currentRounds = 0;
		if (int.TryParse(_roundsString, out currentRounds)
			&& redTeam.Count + blueTeam.Count >= MIN_USERS) {
			if (GUILayout.Button ("Start Game")) {
				ISFSObject sendJoinMessage = new SFSObject();
				sendJoinMessage.PutUtfString("type", "everyoneJoin");
				smartFox.Send(new ObjectMessageRequest(sendJoinMessage, null, smartFox.LastJoinedRoom.UserList));
				var rounds = new SFSRoomVariable("rounds", currentRounds);
				smartFox.Send(new SetRoomVariablesRequest(new [] { rounds }, smartFox.LastJoinedRoom));
			}
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
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
