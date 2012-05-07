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
	
	private Vector3 lastAngle;
	private Vector3 mostRecentAngle;
	private Vector3 angleDelta;
	
	private float prevTime;
	private float mostRecentTime;
	
	private bool firstUpdate;
	
	void Start () {
		firstUpdate = true;
		smartFox = SmartFoxConnection.Connection;
		smartFox.AddEventListener(SFSEvent.OBJECT_MESSAGE, onMessage);

	}
	
	void OnDestroy() {
		smartFox.RemoveEventListener(SFSEvent.OBJECT_MESSAGE, onMessage);
	}
	
	void FixedUpdate()
	{
		// Interpolate the time-step based on current velocity
		float deltaX = Time.deltaTime * distanceDelta.x;
		float deltaY = Time.deltaTime * distanceDelta.y;
		float deltaZ = Time.deltaTime * distanceDelta.z;
		
		Vector3 rotDelta = Time.deltaTime * angleDelta;
		
		transform.position = (new Vector3(transform.position.x + deltaX, transform.position.y + deltaY, transform.position.z + deltaZ));//distanceDelta/(mostRecentTime - prevTime);
		
		transform.Rotate(rotDelta);
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
		Vector3 nAngle = NetworkHelper.GetSFSRotation(obj);
		
		if (firstUpdate)
		{
			mostRecentTrans = nTrans;
			mostRecentAngle = nAngle;
			firstUpdate = false;
		}
		
		// Store last trans
		lastTransform = mostRecentTrans;
		
		// Store last angle
		lastAngle = mostRecentAngle;
		
		mostRecentAngle = nAngle;
		
		prevTime = mostRecentTime;
		
		mostRecentTrans = nTrans;
		
		mostRecentTime = obj.GetFloat("time");
		
		distanceDelta.x = (mostRecentTrans.x - lastTransform.x)/(mostRecentTime - prevTime);
		distanceDelta.y = (mostRecentTrans.y - lastTransform.y)/(mostRecentTime - prevTime);
		distanceDelta.z = (mostRecentTrans.z - lastTransform.z)/(mostRecentTime - prevTime);
		
		// Need to prevent issues from angle differences larger than 180
		if ( Math.Abs(mostRecentAngle.x - lastAngle.x) > 180.0f)
		{
			if (mostRecentAngle.x < lastAngle.x)
			{
				mostRecentAngle.x = mostRecentAngle.x + 360.0f;
			}
			else
			{
				lastAngle.x = lastAngle.x + 360.0f;
			}
		}
		
		if ( Math.Abs(mostRecentAngle.y - lastAngle.y) > 180.0f)
		{
			if (mostRecentAngle.y < lastAngle.y)
			{
				mostRecentAngle.y = mostRecentAngle.y + 360.0f;
			}
			else
			{
				lastAngle.y = lastAngle.y + 360.0f;
			}
		}
	
		if ( Math.Abs(mostRecentAngle.z - lastAngle.z) > 180.0f)
		{
			if (mostRecentAngle.z < lastAngle.z)
			{
				mostRecentAngle.z = mostRecentAngle.z + 360.0f;
			}
			else
			{
				lastAngle.z = lastAngle.z + 360.0f;
			}
		}
		
		angleDelta = (mostRecentAngle - lastAngle)/(mostRecentTime - prevTime);
		
		lastAngle = FixAngles(lastAngle);
		mostRecentAngle = FixAngles(mostRecentAngle);
	}
	
	private void updateRobotPosition(ISFSObject obj) {
		NetworkHelper.SFSObjectToTransform(obj, transform);
	}
	
	private Vector3 FixAngles(Vector3 sourceAngle)
	{
		Vector3 retAngles = new Vector3();
		
		if (sourceAngle.x >= 360.0f)
		{
			retAngles.x = sourceAngle.x - 360.0f;	
		}
		else
		{
			retAngles.x = sourceAngle.x;
		}
		if (sourceAngle.y >= 360.0f)
		{
			retAngles.y = sourceAngle.y - 360.0f;	
		}
		else
		{
			retAngles.y = sourceAngle.y;
		}
		if (sourceAngle.z >= 360.0f)
		{
			retAngles.z = sourceAngle.z - 360.0f;	
		}
		else
		{
			retAngles.z = sourceAngle.z;
		}
		
		return retAngles;
	}
}
