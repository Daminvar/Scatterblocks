using UnityEngine;
using System.Collections;
using System;
using System.ComponentModel;

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

	public static float sendingPeriod = 0.08f; 
	public bool IsBlueTeam;
	
	void Start ()
	{
		smartFox = SmartFoxConnection.Connection;
		InvokeRepeating("SendTransform", sendingPeriod, sendingPeriod);
	}
		
	void SendTransform() {
		var pos = transform.position;
		var angles = transform.localEulerAngles;
		var worker = new BackgroundWorker();
		var time = Time.time;
		worker.DoWork += (sender, e) => {
			ISFSObject obj = NetworkHelper.TransformToSFSObject(pos, angles);
			obj.PutBool("isBlue", IsBlueTeam);
			obj.PutFloat("time", time);
			smartFox.Send(new ObjectMessageRequest(obj));
		};
		worker.RunWorkerAsync();
	}
		
}
