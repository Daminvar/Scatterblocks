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
	public GameObject ExplosionPrefab;
	
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
			
			if(_isBlueTeam)
				this.GetComponentInChildren<Renderer>().material.color = Color.blue;
			else
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
	
	private GameObject plane;
	private bool _dead = false;
	private Vector3 _positionOfDeath;
	
	// Use this for initialization
	void Start () {
		smartFox = SmartFoxConnection.Connection;
		
		startingTransform = this.transform.position;
		farthestDistance = 0;
		farthestDistanceScore = 0;
		
		plane = GameObject.FindGameObjectWithTag("Plane");
		//Debug.Log(goalPlatform);
	}
	
	// Update is called once per frame
	void Update () {
		if(_dead) {
			transform.position = _positionOfDeath;
		}
	}
	
	IEnumerator deathDelay() {
		yield return new WaitForSeconds(2);
		_dead = false;
		GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
		SendMessage("MakeIdle", SendMessageOptions.DontRequireReceiver);
		transform.position = startingTransform;
	}
	
	void Die()
	{
		CalculateScore();
		
		_dead = true;
		GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
		_positionOfDeath = transform.position;
		StartCoroutine(deathDelay());
		var explosion = Instantiate(ExplosionPrefab, transform.position, Quaternion.identity) as GameObject;
		Destroy(explosion, 1.5f);
		
		List<GameObject> blockList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Block"));
		int blockIndex = blockList.IndexOf(lastCollision);
		unlockBlock(blockIndex);
	}
	
	public void CalculateScore()
	{
		if (!smartFox.LastJoinedRoom.GetVariable("countdownToggle").GetBoolValue())
		{
			float distanceAtDeath;
			List<RoomVariable> roomVars = new List<RoomVariable>();
			
			if (IsBlueTeam)
			{
				distanceAtDeath = transform.position.x - (-plane.collider.bounds.extents.x);
				
				if (distanceAtDeath > farthestDistance)
				{
					farthestDistance = distanceAtDeath;
			
					farthestDistanceScore = (int)((farthestDistance / plane.collider.bounds.size.x) * 500);
				}
				
				roomVars.Add(new SFSRoomVariable("blueStored", farthestDistanceScore));
			}
			else
			{
				distanceAtDeath = plane.collider.bounds.extents.x - transform.position.x;
				
				if (distanceAtDeath > farthestDistance)
				{
					farthestDistance = distanceAtDeath;
			
					farthestDistanceScore = (int)((farthestDistance / plane.collider.bounds.size.x) * 500);
				}
				
				roomVars.Add(new SFSRoomVariable("redStored", farthestDistanceScore));
			}
			
			smartFox.Send(new SetRoomVariablesRequest(roomVars, smartFox.LastJoinedRoom));
		}	
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if(_dead)
			return;
		
		if (hit.collider.gameObject == lastCollision)
			return;
		
		if(hit.collider.CompareTag("Plane")) {
			Die();
			return;
		}
		else if (hit.collider.gameObject.tag == "Block")
		{
			if (hit.collider.gameObject.GetComponent<BlockScript>().isDangerous)
			{
				Die();
				return;
			}
			
			List<GameObject> blockList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Block"));
			
			int blockIndex;
			
			if (lastCollision != null) {
				blockIndex = blockList.IndexOf(lastCollision);
				unlockBlock(blockIndex);
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
				List<GameObject> blockList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Block"));
				int blockIndex = blockList.IndexOf(lastCollision);
				unlockBlock(blockIndex);
				
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
				List<GameObject> blockList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Block"));
				int blockIndex = blockList.IndexOf(lastCollision);
				unlockBlock(blockIndex);
				
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
	
	private void unlockBlock(int index) {
		if(index == -1)
			return;
		var unlockBlock = new SFSObject();
	
		unlockBlock.PutInt("index", index);
		unlockBlock.PutUtfString("type", "unlock");
	
		smartFox.Send(new ObjectMessageRequest(unlockBlock, null, smartFox.LastJoinedRoom.UserList));
	}
}
