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

	private string newMessage = "";
	private ArrayList messages = new ArrayList();
	
	private List<string> redTeam = new List<string>();
	private List<string> blueTeam = new List<string>();
	
	private Vector2 roomScrollPosition, userScrollPosition, chatScrollPosition;
	private int screenW;
	
	private SmartFox smartFox;
	
	private bool isBlueTeam = false;
	
	// Use this for initialization
	void Start () {
		
		smartFox = SmartFoxConnection.Connection;
		AddEventListeners();
		
		UpdateTeamLists();
		
		SFSArray myTeamUpdate = new SFSArray();
		
		if (blueTeam.Count < redTeam.Count)
		{
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
	}
	
	private void AddEventListeners() {
		
		//smartFox.RemoveAllEventListeners();
		
		smartFox.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
		smartFox.AddEventListener(SFSEvent.LOGOUT, OnLogout);
		smartFox.AddEventListener(SFSEvent.PUBLIC_MESSAGE, OnPublicMessage);
		smartFox.AddEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, OnRoomVariableUpdate);
		smartFox.AddEventListener(SFSEvent.OBJECT_MESSAGE, OnObjectMessage);
		//smartFox.AddEventListener(SFSEvent.ROOM_JOIN, OnJoinRoom);
	}
	
	public void OnConnectionLost(BaseEvent evt) {
		/*Debug.Log("OnConnectionLost");
		isLoggedIn = false;
		UnregisterSFSSceneCallbacks();
		currentActiveRoom = null;
		roomSelection = -1;	
		Application.LoadLevel("The Lobby");
		*/
	}
	
	void OnLogout(BaseEvent evt) {
		/*Debug.Log("OnLogout");
		isLoggedIn = false;
		currentActiveRoom = null;
		smartFox.Disconnect();
		*/
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
		//Do things.
		UpdateTeamLists();
	}
	
	/*public void OnJoinRoom(BaseEvent evt)
	{
		Room room = (Room)evt.Params["room"];
		Debug.Log("joined "+room.Name);
		if(room.Name=="The Lobby")
		{
			Application.LoadLevel("MainMenu");
		}
	}*/
	
	public void OnObjectMessage(BaseEvent evt)
	{
		ISFSObject message = (SFSObject)evt.Params["message"];
		
		if (message.GetUtfString("type") == "everyoneJoin")
		{
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
		if (redTeam.Count + blueTeam.Count > 2 && IsLowestID())
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
		int userIDToCheck = 100;
		int myID = smartFox.MySelf.GetPlayerId(smartFox.LastJoinedRoom);
		
		foreach (User u in smartFox.LastJoinedRoom.UserList)
		{
			userIDToCheck = u.GetPlayerId(smartFox.LastJoinedRoom);
			
			if (userIDToCheck < lowestUserID)
			{
				lowestUserID = userIDToCheck;
			}
		}
		
		if (myID == lowestUserID)
		{
			return true;
		}
		else
		{
			return false;	
		}
	}
}
