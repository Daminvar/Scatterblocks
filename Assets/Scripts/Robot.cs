using UnityEngine;
using System.Collections;
using System;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;
using Sfs2X.Exceptions;

public class Robot : MonoBehaviour {
	private bool _isBlueTeam;
	public bool IsBlueTeam {
		get { return _isBlueTeam; }
		set {
			_isBlueTeam = value;
			if(!_isBlueTeam)
				this.GetComponentInChildren<Renderer>().material.color = Color.red;
		}
	}
	private SmartFox smartFox;
	
	private Vector3 lastTransform;
	private Vector3 mostRecentTrans;
	private Vector3 distanceDelta;
	
	private float prevTime;
	private float mostRecentTime;
	
	private bool firstUpdate = true;
	
	void Start () {
		smartFox = SmartFoxConnection.Connection;
		smartFox.AddEventListener(SFSEvent.OBJECT_MESSAGE, onMessage);

	}
	
	void FixedUpdate()
	{
		// Interpolate the time-step based on current velocity
		float deltaX = Time.deltaTime * distanceDelta.x;
		float deltaY = Time.deltaTime * distanceDelta.y;
		float deltaZ = Time.deltaTime * distanceDelta.z;
		
		if (Math.Abs(deltaX) < 100000.0f && Math.Abs(deltaY) < 100000.0f && Math.Abs(deltaZ) < 100000.0f)
		{
			transform.position = new Vector3(transform.position.x + deltaX, transform.position.y + deltaY, transform.position.z + deltaZ);
		}
	}
	
	private void onMessage(BaseEvent evt) {
		ISFSObject msg = (SFSObject)evt.Params["message"];
		if(msg.GetUtfString("type") == "transform" && msg.GetBool("isBlue") == IsBlueTeam)
		{
			updateInterpolationData(msg);
			updateRobotPosition(msg);
		}
	}
	
	private void updateInterpolationData(ISFSObject obj) {
		Vector3 nTrans = NetworkHelper.GetSFSTransform(obj);
		
		if (firstUpdate)
		{
			mostRecentTrans = nTrans;
			
			firstUpdate = false;
		}
		
		// Store last trans
		lastTransform = mostRecentTrans;
		
		prevTime = mostRecentTime;
		
		mostRecentTrans = nTrans;
		
		mostRecentTime = Time.time;
		
		distanceDelta.x = (mostRecentTrans.x - lastTransform.x)/(mostRecentTime - prevTime);
		distanceDelta.y = (mostRecentTrans.y - lastTransform.y)/(mostRecentTime - prevTime);
		distanceDelta.z = (mostRecentTrans.z - lastTransform.z)/(mostRecentTime - prevTime);
	}
	
	private void updateRobotPosition(ISFSObject obj) {
		NetworkHelper.SFSObjectToTransform(obj, transform);
	}
}
