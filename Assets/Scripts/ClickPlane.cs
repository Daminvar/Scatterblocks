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
	public const float Y_PLANE = 3f;
	
	private Vector3 targetPosition;
	
	private SmartFox smartFox;
	
	// Use this for initialization
	void Start () 
	{
		smartFox = SmartFoxConnection.Connection;
		smartFox.AddEventListener(SFSEvent.OBJECT_MESSAGE, onMessage);
		
		targetPosition = new Vector3(0,0,0);	
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
			pos.Set(pos.x, Y_PLANE, pos.z);
			
			//Make object message.
			sendExplosionForce(pos, 100.0f);
		}
	}
	
	void sendExplosionForce(Vector3 location, float force)
	{
		ISFSObject sendExplosion = new SFSObject();
		sendExplosion.PutBool("explosion", true);
		sendExplosion.PutFloat("force", force);
		sendExplosion.PutFloatArray("pos", new[] {location.x, location.z});
		smartFox.Send(new ObjectMessageRequest(sendExplosion, null, smartFox.LastJoinedRoom.UserList));
	}
	
	private void onMessage(BaseEvent evt) {
		ISFSObject msg = (SFSObject)evt.Params["message"];
		if(msg.ContainsKey("explosion"))
			recieveExplosionForce(msg);
	}
	
	private void recieveExplosionForce(ISFSObject msg) {
		var force = msg.GetFloat("force");
		var pos = msg.GetFloatArray("pos");
		targetPosition = new Vector3(pos[0], Y_PLANE, pos[1]);
		GameObject[] blocksArray = GameObject.FindGameObjectsWithTag("Block");
		foreach (GameObject block in blocksArray)
		{
			if (block.GetComponent<BoxCollider>().bounds.Contains(targetPosition))
			{
				var extents = block.GetComponent<BoxCollider>().bounds.extents;
				if(targetPosition.x > block.transform.position.x)
					targetPosition += new Vector3(extents.x, 0, 0);
				else
					targetPosition -= new Vector3(extents.x, 0, 0);
				
				if(targetPosition.z > block.transform.position.z)
					targetPosition += new Vector3(0, 0, extents.z);
				else
					targetPosition -= new Vector3(0, 0, extents.z);
			}
			block.GetComponent<Rigidbody>().AddExplosionForce(100.0f, targetPosition, 25.0f, 0.0f, ForceMode.Impulse);
		}
	}
}
