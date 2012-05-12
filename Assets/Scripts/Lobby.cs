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

public class Lobby : MonoBehaviour {

	private SmartFox smartFox;
	private string zone = "m113wjz";
	private string serverName = "129.21.29.6";
	private int serverPort = 9933;
	public string username = "";
	private string loginErrorMessage = "";
	private bool isLoggedIn;
	
	private string newMessage = "";
	private ArrayList messages = new ArrayList();
	
	//keep track of room we're in
	//private Room currentActiveRoom;
	//public Room CurrentActiveRoom{ get {return currentActiveRoom;} }
				
	private Vector2 roomScrollPosition, userScrollPosition, chatScrollPosition;
	private int roomSelection = -1;	  //For clicking on list box 
	private string[] roomNameStrings; //Names of rooms
	private string[] roomFullStrings; //Names and descriptions
	private int screenW;

	
	void Start()
	{
		Security.PrefetchSocketPolicy(serverName, serverPort); 
		bool debug = true;
		if (SmartFoxConnection.IsInitialized)
		{
			//If we've been here before, the connection has already been initialized. 
			//and we don't want to re-create this scene, therefore destroy the new one
			smartFox = SmartFoxConnection.Connection;
			Destroy(gameObject); 
		}
		else
		{
			//If this is the first time we've been here, keep the Lobby around
			//even when we load another scene, this will remain with all its data
			smartFox = new SmartFox(debug);
			DontDestroyOnLoad(gameObject);
		}
		
		smartFox.AddLogListener(LogLevel.INFO, OnDebugMessage);
		//smartFox.AddLogListener(LogLevel.DEBUG, OnDebugMessage);
		smartFox.AddLogListener(LogLevel.ERROR, OnDebugMessage);
		smartFox.AddLogListener(LogLevel.WARN, OnDebugMessage);
		screenW = Screen.width;
	}
	
	private void AddEventListeners() {
		
		smartFox.RemoveAllEventListeners();
		
		smartFox.AddEventListener(SFSEvent.CONNECTION, OnConnection);
		smartFox.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
		smartFox.AddEventListener(SFSEvent.LOGIN, OnLogin);
		smartFox.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
		smartFox.AddEventListener(SFSEvent.LOGOUT, OnLogout);
		smartFox.AddEventListener(SFSEvent.ROOM_JOIN, OnJoinRoom);
		smartFox.AddEventListener(SFSEvent.ROOM_ADD, OnRoomCountChange);
		smartFox.AddEventListener(SFSEvent.ROOM_CREATION_ERROR, OnRoomCreationError);
		smartFox.AddEventListener(SFSEvent.ROOM_REMOVE, OnRoomCountChange);
		smartFox.AddEventListener(SFSEvent.PUBLIC_MESSAGE, OnPublicMessage);
		smartFox.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
		smartFox.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserLeaveRoom);
		smartFox.AddEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChange);
	}
	
	void FixedUpdate() {
		//this is necessary to have any smartfox action!
		smartFox.ProcessEvents();
	}
	
	public void OnConnection(BaseEvent evt) {
		bool success = (bool)evt.Params["success"];
		string error = (string)evt.Params["errorMessage"];
		
		Debug.Log("On Connection callback got: " + success + " (error? : <" + error + ">)");

		if (success) {
			SmartFoxConnection.Connection = smartFox;

			Debug.Log("Sending login request");
			smartFox.Send(new LoginRequest(username, "", zone));

		}
	}

	public void OnConnectionLost(BaseEvent evt) {
		Debug.Log("OnConnectionLost");
		isLoggedIn = false;
		smartFox.RemoveAllEventListeners();
		roomSelection = -1;	
		Application.LoadLevel("MainMenu");
	}

	// Various SFS callbacks
	public void OnLogin(BaseEvent evt) {
		try {
			if (evt.Params.ContainsKey("success") && !(bool)evt.Params["success"]) {
				loginErrorMessage = (string)evt.Params["errorMessage"];
				Debug.Log("Login error: "+loginErrorMessage);
			}
			else {
				Debug.Log("Logged in successfully");
				PrepareLobby();	
			}
		}
		catch (Exception ex) {
			Debug.Log("Exception handling login request: "+ex.Message+" "+ex.StackTrace);
		}
	}

	public void OnLoginError(BaseEvent evt) {
		Debug.Log("Login error: "+(string)evt.Params["errorMessage"]);
	}
	
	void OnLogout(BaseEvent evt) {
		Debug.Log("OnLogout");
		isLoggedIn = false;
		//currentActiveRoom = null;
		smartFox.Disconnect();
	}
	
	public void OnDebugMessage(BaseEvent evt) {
		string message = (string)evt.Params["message"];
		Debug.Log("[SFS DEBUG] " + message);
	}
	
	public void OnRoomCountChange(BaseEvent evt)
	{
		SetupRoomList();
	}
	
	public void OnRoomCreationError(BaseEvent evt)
	{
		Debug.LogError("Error creating room");
	}
	
	public void OnJoinRoom(BaseEvent evt)
	{
		AddEventListeners();
		SetupRoomList();
		Room room = (Room)evt.Params["room"];
		//currentActiveRoom = room;
		Debug.Log(" Lobby.OnJoinRoom joined "+room.Name);
		if(room.Name=="The Lobby" )
			Application.LoadLevel("MainMenu");
		else
		{
			Application.LoadLevel("WaitScene");
		}
	}
	
	public void OnUserEnterRoom(BaseEvent evt) {
		User user = (User)evt.Params["user"];
			messages.Add( user.Name + " has entered the room.");
	}

	private void OnUserLeaveRoom(BaseEvent evt) {
		User user = (User)evt.Params["user"];
		if(user.Name!=username){
			messages.Add( user.Name + " has left the room.");
		}	
	}

	public void OnUserCountChange(BaseEvent evt) {
		SetupRoomList();
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
	
	//PrepareLobby is called from OnLogin, the callback for login
	//so we can be assured that login was successful
	private void PrepareLobby() {
		Debug.Log("Setting up the lobby");
		SetupRoomList();
		isLoggedIn = true;
	}
	
	void OnGUI() 
	{
		if (smartFox == null) return;
		screenW = Screen.width;
				
		// Login
		if (!isLoggedIn) {
			DrawLoginGUI();
		} else if (smartFox.LastJoinedRoom != null) {
			// Show full interface only in the Lobby
			if (smartFox.LastJoinedRoom.Name == "The Lobby" && Application.loadedLevelName == "MainMenu")
			{
				DrawLobbyGUI();
				DrawRoomsGUI();
			}
		}
	}
	
	private void DrawLoginGUI(){
		GUI.Label(new Rect(10, 90, 100, 100), "Username: ");
		username = GUI.TextField(new Rect(100, 90, 200, 20), username, 25); 
	
		GUI.Label(new Rect(10, 180, 100, 100), "Server: ");
		serverName = GUI.TextField(new Rect(100, 180, 200, 20), serverName, 25);

		GUI.Label(new Rect(10, 210, 100, 100), "Port: ");
		serverPort = int.Parse(GUI.TextField(new Rect(100, 210, 200, 20), serverPort.ToString(), 4));

		GUI.Label(new Rect(10, 240, 100, 100), loginErrorMessage);

		if (GUI.Button(new Rect(100, 270, 100, 24), "Login")  || 
	    (Event.current.type == EventType.keyDown && Event.current.character == '\n'))
		{
			AddEventListeners();
			smartFox.Connect(serverName, serverPort);
		}	
	}
			
	private void DrawLobbyGUI(){
		DrawUsersGUI();	
		DrawChatGUI();
		
		// Send message
		newMessage = GUI.TextField(new Rect(10, 480, 370, 20), newMessage, 50);
		if (GUI.Button(new Rect(390, 478, 90, 24), "Send")  || (Event.current.type == EventType.keyDown && Event.current.character == '\n'))
		{
			smartFox.Send( new PublicMessageRequest(newMessage) );
			newMessage = "";
		}
		// Logout button
		if (GUI.Button (new Rect (screenW - 115, 20, 85, 24), "Logout")) {
			smartFox.Send( new LogoutRequest() );
		}
	}
		
	private void DrawUsersGUI(){
		GUI.Box (new Rect (screenW - 250, 80, 230, 170), "Users");
		GUILayout.BeginArea (new Rect (screenW - 230, 110, 150, 160));
			userScrollPosition = GUILayout.BeginScrollView (userScrollPosition, GUILayout.Width (150), GUILayout.Height (150));
			GUILayout.BeginVertical ();
			
				List<User> userList = smartFox.LastJoinedRoom.UserList;
				foreach (User user in userList) {
					GUILayout.Label (user.Name); 
				}
			GUILayout.EndVertical ();
			GUILayout.EndScrollView ();
		GUILayout.EndArea ();
	}
	
	private void DrawRoomsGUI(){
		roomSelection = -1;
		GUI.Box (new Rect (screenW - 250, 260, 230, 130), "Room List");
		GUILayout.BeginArea (new Rect (screenW - 230, 290, 210, 150));
			if (smartFox.RoomList.Count >= 1) {		
				roomScrollPosition = GUILayout.BeginScrollView (roomScrollPosition, GUILayout.Width (190), GUILayout.Height (130));
					roomSelection = GUILayout.SelectionGrid (roomSelection, roomFullStrings, 1);
					
					if (roomSelection >= 0 && roomNameStrings[roomSelection] != smartFox.LastJoinedRoom.Name) {
						smartFox.Send(new JoinRoomRequest(roomNameStrings[roomSelection]));
					}
				GUILayout.EndScrollView ();
				
			} else {
				GUILayout.Label ("No rooms available to join");
			}
			
			// Game Room button
			if (smartFox.LastJoinedRoom.Name == "The Lobby"){
				if (GUI.Button (new Rect (60, 110, 85, 24), "Make Game")) {		
					// ****** Create new room ******* //
					Debug.Log("new room "+username + "'s Room");
					
					//let smartfox take care of error if duplicate name
					RoomSettings settings = new RoomSettings(username + "'s Room");
					// how many players allowed
					settings.MaxUsers = 8;
					settings.MaxVariables = 50;
					settings.IsGame = true;
				
					List<RoomVariable> roomVars = new List<RoomVariable>();
					roomVars.Add(new SFSRoomVariable("redStored", 0));
					roomVars.Add(new SFSRoomVariable("blueStored", 0));
					roomVars.Add(new SFSRoomVariable("redTotalScore", 0));
					roomVars.Add(new SFSRoomVariable("blueTotalScore", 0));
					roomVars.Add(new SFSRoomVariable("redRobot", ""));
					roomVars.Add(new SFSRoomVariable("blueRobot", ""));
					roomVars.Add(new SFSRoomVariable("blue", new SFSArray()));
					roomVars.Add(new SFSRoomVariable("red", new SFSArray()));
					roomVars.Add(new SFSRoomVariable("rounds", 0));
					roomVars.Add(new SFSRoomVariable("currentRound", 1));
				
					settings.Variables = roomVars;
				
					smartFox.Send(new CreateRoomRequest(settings, true, smartFox.LastJoinedRoom));
				}
			}
		GUILayout.EndArea();
	}
	
	private void DrawChatGUI()
	{
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
	}
	
	private void SetupRoomList () {
		List<string> rooms = new List<string> ();
		List<string> roomsFull = new List<string> ();
		
		List<Room> allRooms = smartFox.RoomManager.GetRoomList();
		
		foreach (Room room in allRooms) {
			rooms.Add(room.Name);
			roomsFull.Add(room.Name + " (" + room.UserCount + "/" + room.MaxUsers + ")");
		}
		
		roomNameStrings = rooms.ToArray();
		roomFullStrings = roomsFull.ToArray();
		
		if (smartFox.LastJoinedRoom==null) {
			smartFox.Send(new JoinRoomRequest("The Lobby"));
		}
	}
}
