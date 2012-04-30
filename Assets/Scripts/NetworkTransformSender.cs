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

// Sends the transform of the local player to server
public class NetworkTransformSender : MonoBehaviour {
	private SmartFox smartFox;

	// We will send transform each 0.1 second. To make transform 
	//synchronization smoother consider writing interpolation algorithm 
	//instead of making smaller period.
	public static float sendingPeriod = 0.03f; 
	
	private float timeLastSending = 0.0f;
	private bool send = false;
	public bool IsBlueTeam;
	
	void Start ()
	{
		smartFox = SmartFoxConnection.Connection;
		StartSendTransform();
	}
		
	// We call it on local player to start sending his transform
	public void StartSendTransform() {
		send = true;
	}
		
	void FixedUpdate ()
	{
		if (send) {
			SendTransform ();
			Debug.Log("Sending the transform");
		}
	}
	
	void SendTransform() {
		if (timeLastSending >= sendingPeriod) 
		{
			ISFSObject obj = NetworkHelper.TransformToSFSObject(transform.position, transform.localEulerAngles);
			obj.PutBool("isBlue", IsBlueTeam);
			obj.PutFloat("time", Time.time);
			smartFox.Send(new ObjectMessageRequest(obj));
			timeLastSending = 0;
			return;
		}
		timeLastSending += Time.deltaTime;
	}
		
}
