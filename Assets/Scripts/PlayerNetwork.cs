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
	
	private GameObject goalPlatform;
	
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
		else if (_isBlueTeam)
		{
			if (this.transform.position.x > 108)
			{
				Debug.Log("Winner!");	
			}
		}
		else
		{
			if (this.transform.position.x < -108)
			{
				Debug.Log("Winner!");	
			}	
		}
		
	}
	
	void Die()
	{
		Debug.Log("AAAAAARRRRRRGH!");
		
		float distanceAtDeath;
		List<RoomVariable> roomVars = new List<RoomVariable>();
		
		if (IsBlueTeam)
		{
			Debug.Log("Blue team death!");
			
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
			Debug.Log("Red team death!");
			
			distanceAtDeath = 112 - transform.position.x;
			
			if (distanceAtDeath > farthestDistance)
			{
				farthestDistance = distanceAtDeath;
		
				farthestDistanceScore = (int)((farthestDistance / 224) * 500);
			}
			
			roomVars.Add(new SFSRoomVariable("redStored", farthestDistanceScore));
		}
		
		smartFox.Send(new SetRoomVariablesRequest(roomVars, smartFox.LastJoinedRoom));
		
		this.transform.position = startingTransform;
	}
	
	void OnCollisionEnter(Collision collision)
	{
		
	}
}
