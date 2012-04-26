using UnityEngine;
using System.Collections;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Variables;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using Sfs2X.Logging;

public class ClickPlane : MonoBehaviour {
	private SmartFox smartFox;
	
	// Use this for initialization
	void Start () 
	{
		smartFox = SmartFoxConnection.Connection;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		RaycastHit hit;
		if (Input.GetMouseButtonDown (0))
        {
			//Debug.Log("Click");
			if (!Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, 10000, 1 << 8))
			{
				return;
			}
			
      		//our target is where we clicked
			var pos = hit.point;
			
			//Make object message.
			sendExplosionForce(pos, 100.0f);
		}
	}
	
	void sendExplosionForce(Vector3 location, float force)
	{
		ISFSObject sendExplosion = new SFSObject();
		sendExplosion.PutUtfString("type", "explosion");
		sendExplosion.PutFloat("force", force);
		sendExplosion.PutFloatArray("pos", new[] {location.x, location.z});
		smartFox.Send(new ObjectMessageRequest(sendExplosion, null, smartFox.LastJoinedRoom.UserList));
	}
}