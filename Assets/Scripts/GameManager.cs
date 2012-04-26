using UnityEngine;
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
	
	public GameObject RTSCamera;
	public GameObject Player;
	public GameObject RobotPrefab;
	public GameObject PlayerCamera;
	public bool thirdPerson;
	
	private SmartFox smartFox;
	private bool isBlueTeam;
		
	private Vector3 targetPosition;

	// Use this for initialization
	void Start ()
	{
		smartFox = SmartFoxConnection.Connection;
		smartFox.AddEventListener(SFSEvent.OBJECT_MESSAGE, onMessage);
		setCurrentTeam();
				
		targetPosition = new Vector3(0,0,0);	
		
		if(isRobot())
			thirdPerson = true;
		
		if(thirdPerson && !isBlueTeam)
		{
			GameObject player;
			if(isBlueTeam)
				player = Instantiate(Player, BLUE_START, Quaternion.identity) as GameObject;
			else
				player = Instantiate(Player, RED_START, Quaternion.identity) as GameObject;
			var cam = Instantiate(PlayerCamera) as GameObject;
			var smoothFollow = cam.GetComponent<SmoothFollow>();
			Debug.Log(smoothFollow);
			smoothFollow.target = player.transform;
		}
		else
		{
			var cam = Instantiate(RTSCamera, new Vector3(0, 200, 0), Quaternion.identity) as GameObject;
			cam.transform.LookAt(new Vector3(0, 0, 0));
			GameObject robot = Instantiate(RobotPrefab, RED_START, Quaternion.identity) as GameObject;
			robot.GetComponent<Robot>().IsBlueTeam = false;
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
		if(redbot.GetStringValue() == smartFox.MySelf.Name)
			return true;
		if(bluebot.GetStringValue() == smartFox.MySelf.Name)
			return true;
		return false;
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
	
	private void onMessage(BaseEvent evt) {
		ISFSObject msg = (SFSObject)evt.Params["message"];
		if(msg.GetUtfString("type") == "explosion")
			recieveExplosionForce(msg);
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
}
