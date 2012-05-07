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

public class GameManager : MonoBehaviour {
	public readonly Vector3 BLUE_START = new Vector3(-112, 27, 15);
	public readonly Vector3 RED_START = new Vector3(112, 27, 15);
	public const float Y_PLANE = 3f;
	public const float BLOCK_SYNC_INTERVAL = 0.5f;
	
	public GameObject RTSCamera;
	public GameObject Player;
	public GameObject RobotPrefab;
	public GameObject PlayerCamera;
	public bool thirdPerson;
	
	private SmartFox smartFox;
	private bool isBlueTeam;
	private GameObject[] _blocks;
		
	private Vector3 targetPosition;
	
	private bool countDownStarted = false;
	private float countDownTime;
	private const int COUNT_DOWN_SECONDS = 10;
	
	private GameObject player;
	
	private GameObject redRobot;
	private GameObject blueRobot;
	
	private bool isHost = false;
	
	// Use this for initialization
	void Start ()
	{
		smartFox = SmartFoxConnection.Connection;
		
		setCurrentTeam();
				
		targetPosition = new Vector3(0,0,0);	
		_blocks = GameObject.FindGameObjectsWithTag("Block");
		
		if(isRobot())
			thirdPerson = true;
		
		if(thirdPerson)
		{
			
			if(isBlueTeam) {
				player = Instantiate(Player, BLUE_START, Quaternion.identity) as GameObject;
				player.GetComponent<PlayerNetwork>().IsBlueTeam = true;
				redRobot = Instantiate(RobotPrefab, RED_START, Quaternion.identity) as GameObject;
				redRobot.GetComponent<Robot>().IsBlueTeam = false;
			} else {
				player = Instantiate(Player, RED_START, Quaternion.identity) as GameObject;
				player.GetComponent<PlayerNetwork>().IsBlueTeam = false;
				blueRobot = Instantiate(RobotPrefab, BLUE_START, Quaternion.identity) as GameObject;
				blueRobot.GetComponent<Robot>().IsBlueTeam = true;
			}
			var cam = Instantiate(PlayerCamera) as GameObject;
			var smoothFollow = cam.GetComponent<SmoothFollow>();
			Debug.Log(smoothFollow);
			smoothFollow.target = player.transform;
		}
		else
		{
			var cam = Instantiate(RTSCamera, new Vector3(0, 200, 0), Quaternion.identity) as GameObject;
			cam.transform.LookAt(new Vector3(0, 0, 0));
			redRobot = Instantiate(RobotPrefab, RED_START, Quaternion.identity) as GameObject;
			blueRobot = Instantiate(RobotPrefab, BLUE_START, Quaternion.identity) as GameObject;
			redRobot.GetComponent<Robot>().IsBlueTeam = false;
			blueRobot.GetComponent<Robot>().IsBlueTeam = true;
		}
		
		
		if(IsLowestID())
			InvokeRepeating("sendBlockData", BLOCK_SYNC_INTERVAL, BLOCK_SYNC_INTERVAL);
		
		
		ResetEventListeners();
		
		RoomVariable countdownChecker = smartFox.LastJoinedRoom.GetVariable("countdownToggle");
		
		if (countdownChecker == null)
		{
			Debug.Log("countdownchecker is null");
			
			if (IsLowestID())
			{
				List<RoomVariable> roomVars = new List<RoomVariable>();
				RoomVariable countdownToggle = new SFSRoomVariable("countdownToggle", true);
				roomVars.Add(countdownToggle);
				smartFox.Send(new SetRoomVariablesRequest(roomVars, smartFox.LastJoinedRoom));
			}
		}
	}
	
	private void ResetEventListeners()
	{
		smartFox.RemoveEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, OnRoomVariableUpdate);
		smartFox.AddEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, OnRoomVariableUpdate);
		
		smartFox.RemoveEventListener(SFSEvent.OBJECT_MESSAGE, OnMessage);
		smartFox.AddEventListener(SFSEvent.OBJECT_MESSAGE, OnMessage);
	}
	
	private void OnRoomVariableUpdate( BaseEvent evt )
	{
		ArrayList changedVars = (ArrayList)evt.Params["changedVars"];
		
		foreach (string item in changedVars) {
			
			if (item == "countdownToggle")
			{
				if (smartFox.LastJoinedRoom.GetVariable(item).GetBoolValue())
				{
					countDownStarted = true;
					countDownTime = Time.time;
				}
				else
				{
					smartFox.RemoveEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, OnRoomVariableUpdate);
					smartFox.RemoveEventListener(SFSEvent.OBJECT_MESSAGE, OnMessage);
					Application.LoadLevel(Application.loadedLevel);
				}
			}
		}
	
	}
	
	private void setCurrentTeam() {
		ISFSArray reds = smartFox.LastJoinedRoom.GetVariable("red").GetSFSArrayValue();
		ISFSArray blues = smartFox.LastJoinedRoom.GetVariable("blue").GetSFSArrayValue();
		for(int i = 0; i < reds.Size(); i++) {
			if(reds.GetUtfString(i) == smartFox.MySelf.Name) {
				isBlueTeam = false;
				return;
			}
		}
		isBlueTeam = true;
	}
	
	private bool isRobot() {
		RoomVariable redbot = smartFox.LastJoinedRoom.GetVariable("redRobot");
		RoomVariable bluebot = smartFox.LastJoinedRoom.GetVariable("blueRobot");
		
		Debug.Log(redbot);
		Debug.Log(bluebot);
		
		if(redbot.GetStringValue() == smartFox.MySelf.Name)
			return true;
		else if(bluebot.GetStringValue() == smartFox.MySelf.Name)
			return true;
		return false;
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
	
	void OnGUI ()
	{
		if (countDownStarted)
		{
			DrawCountDown();	
		}
		
		GUI.BeginGroup(new Rect(0, 200, 125, 100));
		
		GUI.Box(new Rect(0, 0, 125, 100), "Scoreboard");
		
		GUI.Label(new Rect(15, 25, 100, 50), "Blue: " + smartFox.LastJoinedRoom.GetVariable("blueTotalScore").GetIntValue() + " [+" + smartFox.LastJoinedRoom.GetVariable("blueStored").GetIntValue() + "]");
		GUI.Label(new Rect(15, 50, 100, 50), "Red: " + smartFox.LastJoinedRoom.GetVariable("redTotalScore").GetIntValue() + " [+" + smartFox.LastJoinedRoom.GetVariable("redStored").GetIntValue() + "]");
		
		GUI.EndGroup();
	}
	
	private void DrawCountDown()
	{
		if (countDownStarted)
		{
			int timeleft = (int)(COUNT_DOWN_SECONDS - (Time.time - countDownTime));
			
			GUIStyle funstyle = new GUIStyle();
			funstyle.fontSize = 50;
			funstyle.normal.textColor = Color.white;
			GUILayout.BeginArea(new Rect(Screen.width/2 - 160, 250, 450, 70));
			GUILayout.Label("Starting in " + timeleft + "s...", funstyle);
			GUILayout.EndArea();
			
			if (timeleft <= 0)
			{
				if (IsLowestID())
				{
					List<RoomVariable> roomVars = new List<RoomVariable>();
					RoomVariable countdownToggle = new SFSRoomVariable("countdownToggle", false);
					roomVars.Add(countdownToggle);
					smartFox.Send(new SetRoomVariablesRequest(roomVars, smartFox.LastJoinedRoom));
					countDownStarted = false;
				}
			}
		}
	}
	
	private void sendBlockData() {
		var obj = new SFSObject();
		obj.PutUtfString("type", "sync");
		var blocksArray = new SFSArray();
		foreach (GameObject block in _blocks) {
			var blockData = new SFSObject();
			blockData.PutFloatArray("position", new[] {block.transform.position.x, block.transform.position.z});
            blockData.PutFloatArray("velocity", new[] {block.rigidbody.velocity.x, block.rigidbody.velocity.z});
			blocksArray.AddSFSObject(blockData);
		}
		obj.PutSFSArray("blocks", blocksArray);
		smartFox.Send (new ObjectMessageRequest(obj));
	}
	
	private void OnMessage(BaseEvent evt) {
		ISFSObject msg = (SFSObject)evt.Params["message"];
		if(msg.GetUtfString("type") == "explosion")
			recieveExplosionForce(msg);
		if(msg.GetUtfString("type") == "sync")
			syncBlocks(msg);
		if(msg.GetUtfString("type") == "lock")
			lockBlock(msg);
		if(msg.GetUtfString("type") == "unlock")
			unlockBlock(msg);
	}
	
	private void recieveExplosionForce(ISFSObject msg) {
		var force = msg.GetFloat("force");
		var pos = msg.GetFloatArray("pos");
		targetPosition = new Vector3(pos[0], Y_PLANE, pos[1]);
		GameObject[] blocksArray = GameObject.FindGameObjectsWithTag("Block");
		foreach (GameObject block in blocksArray)
		{
			if (block.GetComponent<BoxCollider>().bounds.Contains(targetPosition))
			{
				var extents = block.GetComponent<BoxCollider>().bounds.extents;
				if(targetPosition.x > block.transform.position.x)
					targetPosition += new Vector3(extents.x, 0, 0);
				else
					targetPosition -= new Vector3(extents.x, 0, 0);
				
				if(targetPosition.z > block.transform.position.z)
					targetPosition += new Vector3(0, 0, extents.z);
				else
					targetPosition -= new Vector3(0, 0, extents.z);
			}
			block.GetComponent<Rigidbody>().AddExplosionForce(50.0f, targetPosition, 25.0f, 0.0f, ForceMode.Impulse);
		}
	}
	
	private void syncBlocks(ISFSObject msg) {
		var networkBlocks = msg.GetSFSArray("blocks");
		for(int i = 0; i < networkBlocks.Size (); i++) {
			float[] coordinates = networkBlocks.GetSFSObject(i).GetFloatArray("position");
			float[] velocityComponents = networkBlocks.GetSFSObject(i).GetFloatArray("velocity");
			_blocks[i].transform.position = new Vector3(coordinates[0], _blocks[i].transform.position.y, coordinates[1]);
			_blocks[i].rigidbody.velocity = new Vector3(velocityComponents[0], 0, velocityComponents[1]);
		}
	}
	
	private void lockBlock(ISFSObject msg)
	{
		_blocks[msg.GetInt("index")].rigidbody.isKinematic = true;
	}
	
	private void unlockBlock(ISFSObject msg)
	{
		_blocks[msg.GetInt("index")].rigidbody.isKinematic = false;
	}
	
	//TODO: This should be refactored.
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
