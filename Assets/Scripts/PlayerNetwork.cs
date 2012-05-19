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

public class PlayerNetwork : MonoBehaviour {
	private SmartFox smartFox;
	
	private Vector3 startingTransform;
	public Vector3 StartingTransform
	{
		get { return startingTransform; }	
	}
	
	private GameObject goalPlatform;
	private GameObject lastCollision;
	
	private bool _isBlueTeam;
	public bool IsBlueTeam {
		get { return _isBlueTeam; }
		set {
			_isBlueTeam = value;
			
			GetComponent<NetworkTransformSender>().IsBlueTeam = value;
			
			if(!_isBlueTeam)
				this.GetComponentInChildren<Renderer>().material.color = Color.red;
		}
	}
	
	private float farthestDistance;
	public float FarthestDistance
	{
		get { return farthestDistance; }
	}
	
	private int farthestDistanceScore;
	public int FarthestDistanceScore
	{
		get { return farthestDistanceScore; }
	}
	
	// Use this for initialization
	void Start () {
		smartFox = SmartFoxConnection.Connection;
		
		startingTransform = this.transform.position;
		farthestDistance = 0;
		farthestDistanceScore = 0;
		
		//Debug.Log(goalPlatform);
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	void Die()
	{
		CalculateScore();
		if(lastCollision == null)
			return;
			
		this.transform.position = startingTransform;
		var unlockMessage = new SFSObject();
		unlockMessage.PutUtfString("type", "unlock");
		List<GameObject> blockList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Block"));
		int blockIndex = blockList.IndexOf(lastCollision);
		unlockMessage.PutInt("index", blockIndex);
		smartFox.Send(new ObjectMessageRequest(unlockMessage, null, smartFox.LastJoinedRoom.UserList));
	}
	
	public void CalculateScore()
	{
		if (!smartFox.LastJoinedRoom.GetVariable("countdownToggle").GetBoolValue())
		{
			float distanceAtDeath;
			List<RoomVariable> roomVars = new List<RoomVariable>();
			
			if (IsBlueTeam)
			{
				distanceAtDeath = transform.position.x - (-112);
				
				if (distanceAtDeath > farthestDistance)
				{
					farthestDistance = distanceAtDeath;
			
					farthestDistanceScore = (int)((farthestDistance / 224) * 500);
				}
				
				roomVars.Add(new SFSRoomVariable("blueStored", farthestDistanceScore));
			}
			else
			{
				distanceAtDeath = 112 - transform.position.x;
				
				if (distanceAtDeath > farthestDistance)
				{
					farthestDistance = distanceAtDeath;
			
					farthestDistanceScore = (int)((farthestDistance / 224) * 500);
				}
				
				roomVars.Add(new SFSRoomVariable("redStored", farthestDistanceScore));
			}
			
			smartFox.Send(new SetRoomVariablesRequest(roomVars, smartFox.LastJoinedRoom));
		}	
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if(hit.collider.CompareTag("Plane")) {
			Die();
			return;
		}
		if (hit.collider.gameObject.tag == "Block")
		{
			if (hit.collider.gameObject.GetComponent<BlockScript>().isDangerous)
			{
				Die();
				return;
			}
			
			List<GameObject> blockList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Block"));
			
			int blockIndex;
			
			//Unlock old block, if any.
			if (lastCollision != null && hit.collider.gameObject != lastCollision)
			{
				blockIndex = blockList.IndexOf(lastCollision);
				
				var unlockBlock = new SFSObject();
			
				unlockBlock.PutInt("index", blockIndex);
				unlockBlock.PutUtfString("type", "unlock");
			
				smartFox.Send(new ObjectMessageRequest(unlockBlock, null, smartFox.LastJoinedRoom.UserList));
			}
			
			//Lock new block.
			
			blockIndex = blockList.IndexOf(hit.collider.gameObject);
				
			lastCollision = blockList[blockIndex];
			
			var lockBlock = new SFSObject();
			
			lockBlock.PutInt("index", blockIndex);
			lockBlock.PutUtfString("type", "lock");
			
			smartFox.Send(new ObjectMessageRequest(lockBlock, null, smartFox.LastJoinedRoom.UserList));
		}
	}
	
	void OnTriggerEnter(Collider other) {
		if (IsBlueTeam)
		{
			if (other.gameObject.tag == "BlueGoal")
			{
				farthestDistance = Int32.MaxValue;
				
				List<RoomVariable> roomVars = new List<RoomVariable>();
				roomVars.Add(new SFSRoomVariable("blueStored", 550));
				
				ISFSObject roundOverObject = new SFSObject();
				roundOverObject.PutUtfString("type", "iWon");
					
				smartFox.Send(new ObjectMessageRequest(roundOverObject, null, smartFox.LastJoinedRoom.UserList));		
				smartFox.Send(new SetRoomVariablesRequest(roomVars, smartFox.LastJoinedRoom));
			}
		}
		else
		{
			if (other.gameObject.tag == "RedGoal")
			{
				farthestDistance = Int32.MaxValue;
				
				List<RoomVariable> roomVars = new List<RoomVariable>();
				roomVars.Add(new SFSRoomVariable("redStored", 550));
				
				ISFSObject roundOverObject = new SFSObject();
				roundOverObject.PutUtfString("type", "iWon");
					
				smartFox.Send(new ObjectMessageRequest(roundOverObject, null, smartFox.LastJoinedRoom.UserList));
				smartFox.Send(new SetRoomVariablesRequest(roomVars, smartFox.LastJoinedRoom));
			}
		}
	}
}
