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
	
	private int _roundsInt;

	private string newMessage = "";
	private ArrayList messages = new ArrayList();
	private List<string> redTeam = new List<string>();
	private List<string> blueTeam = new List<string>();
	private Vector2 roomScrollPosition, userScrollPosition, chatScrollPosition;
	private int screenW;
	private SmartFox smartFox;
	private bool isBlueTeam = false;
	
	public GUISkin skin;
	
	void Start () {
		smartFox = SmartFoxConnection.Connection;
		AddEventListeners();
		
		_roundsInt = rounds;
		
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
		GUI.skin = skin;
		DrawUsersGUI();
		GUI.skin = null;
		DrawChatGUI();
		DrawButtons();
		if(NetworkHelper.IsLowestID(smartFox))
			DrawHostOptions();
	}
	
	private void AddEventListeners() {
		smartFox.AddEventListener(SFSEvent.PUBLIC_MESSAGE, OnPublicMessage);
		smartFox.AddEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, OnRoomVariableUpdate);
		smartFox.AddEventListener(SFSEvent.OBJECT_MESSAGE, OnObjectMessage);
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
	}
	
	private void DrawHostOptions() {
		GUILayout.BeginArea(new Rect(490, 80, 300, 500));
		GUILayout.BeginVertical("box");
		GUILayout.Label("Host Options");
		GUILayout.Space(10);
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Rounds: ");
		if (GUILayout.Button("<", GUILayout.Width(50)))
		{
			if (_roundsInt > 1)
			{
				_roundsInt--;
			}
		}
		GUILayout.Box(_roundsInt.ToString());
		if (GUILayout.Button(">", GUILayout.Width(50)))
		{
			if (_roundsInt < 20)
			{
				_roundsInt++;
			}
		}
		GUILayout.EndHorizontal();
		
		if (redTeam.Count + blueTeam.Count >= MIN_USERS) 
		{
			if (GUILayout.Button ("Start Game")) 
			{
				ISFSObject sendJoinMessage = new SFSObject();
				sendJoinMessage.PutUtfString("type", "everyoneJoin");
				smartFox.Send(new ObjectMessageRequest(sendJoinMessage, null, smartFox.LastJoinedRoom.UserList));
				var rounds = new SFSRoomVariable("rounds", _roundsInt);
				smartFox.Send(new SetRoomVariablesRequest(new [] { rounds }, smartFox.LastJoinedRoom));
			}
		}
		
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
	
	private void DrawUsersGUI(){
		GUI.Box (new Rect (screenW - 200, 80, 180, 170), "Red Team", "redWaitStyle");
		GUILayout.BeginArea (new Rect (screenW - 190, 110, 150, 160));
			userScrollPosition = GUILayout.BeginScrollView (userScrollPosition, GUILayout.Width (150), GUILayout.Height (150));
			GUILayout.BeginVertical ();
			
				foreach (string user in redTeam)
				{
					if (smartFox.MySelf.Name == user)
					{
						GUILayout.Label(user + " (me)");
					}
					else
					{
						GUILayout.Label (user);
					}
				}
		
			GUILayout.EndVertical ();
			GUILayout.EndScrollView ();
		GUILayout.EndArea ();
		
		GUI.Box (new Rect (screenW - 200, 280, 180, 170), "Blue Team", "blueWaitStyle");
		GUILayout.BeginArea (new Rect (screenW - 190, 310, 150, 160));
			userScrollPosition = GUILayout.BeginScrollView (userScrollPosition, GUILayout.Width (150), GUILayout.Height (150));
			GUILayout.BeginVertical ();
			
				foreach (string user in blueTeam) 
				{
					if (smartFox.MySelf.Name == user)
					{
						GUILayout.Label(user + " (me)", "blueStyle");
					}
					else
					{
						GUILayout.Label (user, "blueStyle");
					} 
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
}
