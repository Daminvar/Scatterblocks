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
		
		if (this.transform.position.y < 7.1f)
		{
			Die();	
		}
	}
	
	void Die()
	{
		Debug.Log("AAAAAARRRRRRGH!");
		
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
		
		this.transform.position = startingTransform;
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if(hit.collider.gameObject.tag == "Block")
		{
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
		else if (IsBlueTeam)
		{
			if (hit.collider.gameObject.tag == "BlueGoal" && transform.position.y >= 26.9)
			{
				farthestDistance = 224;
				
				List<RoomVariable> roomVars = new List<RoomVariable>();
				roomVars.Add(new SFSRoomVariable("blueStored", 500));
				
				ISFSObject sendRoundOverObject = new SFSObject();
				sendRoundOverObject.PutUtfString("type", "roundOver");
					
				smartFox.Send(new ObjectMessageRequest(sendRoundOverObject, null, smartFox.LastJoinedRoom.UserList));		
				smartFox.Send(new SetRoomVariablesRequest(roomVars, smartFox.LastJoinedRoom));
			}
		}
		else
		{
			if (hit.collider.gameObject.tag == "RedGoal" && transform.position.y >= 26.9)
			{
				farthestDistance = 224;
				
				List<RoomVariable> roomVars = new List<RoomVariable>();
				roomVars.Add(new SFSRoomVariable("redStored", 500));
				
				ISFSObject sendRoundOverObject = new SFSObject();
				sendRoundOverObject.PutUtfString("type", "roundOver");
					
				smartFox.Send(new ObjectMessageRequest(sendRoundOverObject, null, smartFox.LastJoinedRoom.UserList));
				smartFox.Send(new SetRoomVariablesRequest(roomVars, smartFox.LastJoinedRoom));
			}
		}
	}
}
