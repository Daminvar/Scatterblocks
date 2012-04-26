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
	public GameObject RTSCamera;
	public GameObject Player;
	public GameObject PlayerCamera;
	public bool thirdPerson;
	
	private SmartFox smartFox;

	// Use this for initialization
	void Start ()
	{
		smartFox = SmartFoxConnection.Connection;
		
		if(thirdPerson)
		{
			var player = Instantiate(Player, new Vector3(-112, 27, 15), Quaternion.identity) as GameObject;
			var cam = Instantiate(PlayerCamera) as GameObject;
			var smoothFollow = cam.GetComponent<SmoothFollow>();
			Debug.Log(smoothFollow);
			smoothFollow.target = player.transform;
		}
		else
		{
			var cam = Instantiate(RTSCamera, new Vector3(0, 200, 0), Quaternion.identity) as GameObject;
			cam.transform.LookAt(new Vector3(0, 0, 0));
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
