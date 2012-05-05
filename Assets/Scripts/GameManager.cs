using UnityEngine;
using System;
using System.Collections;

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

	// Use this for initialization
	void Start ()
	{
		smartFox = SmartFoxConnection.Connection;
		smartFox.AddEventListener(SFSEvent.OBJECT_MESSAGE, onMessage);
		setCurrentTeam();
				
		targetPosition = new Vector3(0,0,0);	
		_blocks = GameObject.FindGameObjectsWithTag("Block");
		
		if(isRobot())
			thirdPerson = true;
		
		if(thirdPerson)
		{
			GameObject player;
			if(isBlueTeam) {
				player = Instantiate(Player, BLUE_START, Quaternion.identity) as GameObject;
				player.GetComponent<PlayerNetwork>().IsBlueTeam = true;
				var redRobot = Instantiate(RobotPrefab, RED_START, Quaternion.identity) as GameObject;
				redRobot.GetComponent<Robot>().IsBlueTeam = false;
			} else {
				player = Instantiate(Player, RED_START, Quaternion.identity) as GameObject;
				player.GetComponent<PlayerNetwork>().IsBlueTeam = false;
				var blueRobot = Instantiate(RobotPrefab, BLUE_START, Quaternion.identity) as GameObject;
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
			GameObject redRobot = Instantiate(RobotPrefab, RED_START, Quaternion.identity) as GameObject;
			GameObject blueRobot = Instantiate(RobotPrefab, BLUE_START, Quaternion.identity) as GameObject;
			redRobot.GetComponent<Robot>().IsBlueTeam = false;
			blueRobot.GetComponent<Robot>().IsBlueTeam = true;
		}
		
		if(IsLowestID())
			InvokeRepeating("sendBlockData", BLOCK_SYNC_INTERVAL, BLOCK_SYNC_INTERVAL);
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
		if (smartFox.LastJoinedRoom.ContainsVariable("blueStored") && smartFox.LastJoinedRoom.ContainsVariable("redStored"))
		{
			GUI.Label(new Rect(50, 50, 100, 50),"Blue: " + smartFox.LastJoinedRoom.GetVariable("blueStored").GetIntValue());
			GUI.Label(new Rect(50, 70, 100, 50),"Red: " + smartFox.LastJoinedRoom.GetVariable("redStored").GetIntValue());
		}
		
		GUI.BeginGroup(new Rect(0, 200, 125, 100));
		
		GUI.Box(new Rect(0, 0, 125, 100), "Scoreboard");
		GUI.Label(new Rect(15, 25, 100, 50), "Blue: " + smartFox.LastJoinedRoom.GetVariable("blueTotalScore").GetIntValue() + " + [" + smartFox.LastJoinedRoom.GetVariable("blueStored").GetIntValue() + "]");
		GUI.Label(new Rect(15, 50, 100, 50), "Red: " + smartFox.LastJoinedRoom.GetVariable("redTotalScore").GetIntValue() + " + [" + smartFox.LastJoinedRoom.GetVariable("redStored").GetIntValue() + "]");
		
		GUI.EndGroup();
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
	
	private void onMessage(BaseEvent evt) {
		ISFSObject msg = (SFSObject)evt.Params["message"];
		if(msg.GetUtfString("type") == "explosion")
			recieveExplosionForce(msg);
		if(msg.GetUtfString("type") == "sync")
			syncBlocks(msg);
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
			block.GetComponent<Rigidbody>().AddExplosionForce(100.0f, targetPosition, 25.0f, 0.0f, ForceMode.Impulse);
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
