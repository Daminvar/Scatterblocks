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
	
	private float startHold;
	private const float MAX_TIME = 3.0f;
	private const float CHARGE_MULTIPLIER = 40.0f;
	private Vector3 explosionPos;
	private Vector2 size = new Vector2(20.0f, 40.0f);
	private Vector3 mousePos;
	
	private bool isCharging = false;
	private float progress = 0.0f;
	
	public Texture2D chargeBarEmpty;
	public Texture2D chargeBarFull;
	
	// Use this for initialization
	void Start () 
	{
		smartFox = SmartFoxConnection.Connection;
		chargeBarEmpty = (Texture2D)Resources.Load("chargeBackground");
		chargeBarFull = (Texture2D)Resources.Load("chargeFull");
	}
	
	// Update is called once per frame
	void Update ()
	{
		RaycastHit hit;
		if (Input.GetMouseButtonDown (0))
        {
			//Debug.Log("Click");
			
			// Ensures the player clicked within the clickable region
			if (!Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, 10000, 1 << 8))
			{
				return;
			}
			
			mousePos = Input.mousePosition;
			startHold = Time.time;
			isCharging = true;
			Screen.showCursor = false;
			
      		//our target is where we clicked
			explosionPos = hit.point;		
			//Make object message.
			//sendExplosionForce(pos, 100.0f);
		}
		if (Input.GetMouseButtonUp (0))
		{
			float totalCharge =  (Time.time - startHold) * CHARGE_MULTIPLIER;
			
			if (totalCharge > 120.0f)
			{
				totalCharge = 120.0f;	
			}
			
			sendExplosionForce(explosionPos, totalCharge);
			
			isCharging = false;
			
			Screen.showCursor = true;
			
		}
		if (isCharging)
		{
			progress = (Time.time - startHold)/MAX_TIME;
		}
	}
	
	void OnGUI()
	{
		if (isCharging)
		{
    		GUI.DrawTexture(new Rect(mousePos.x, Screen.height - mousePos.y, size.x, -size.y * Mathf.Clamp01(progress)), chargeBarFull, ScaleMode.StretchToFill);
			GUI.DrawTexture(new Rect(mousePos.x, Screen.height - mousePos.y - size.y, size.x, size.y), chargeBarEmpty);
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