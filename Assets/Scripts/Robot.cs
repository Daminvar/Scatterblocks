using UnityEngine;
using System.Collections;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;
using Sfs2X.Exceptions;

public class Robot : MonoBehaviour {
	public bool IsBlueTeam;
	private SmartFox smartFox;
	
	// Use this for initialization
	void Start () {
		smartFox = SmartFoxConnection.Connection;
		smartFox.AddEventListener(SFSEvent.OBJECT_MESSAGE, onMessage);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	private void onMessage(BaseEvent evt) {
		ISFSObject msg = (SFSObject)evt.Params["message"];
		if(msg.GetUtfString("type") == "transform")
			updateRobotPosition(msg);
	}
	
	private void updateRobotPosition(ISFSObject obj) {
		NetworkTransform trans = NetworkTransform.FromSFSObject(obj);
		transform.position = trans.Position;
		transform.localEulerAngles = trans.AngleRotation;
		Debug.Log(trans.Position + " : " + transform.position);
	}
}
