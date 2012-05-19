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
	private const float MAX_TIME = 2.0f;
	private const float CHARGE_MULTIPLIER = 80.0f;
	private const float CAMERA_SPEED = 5f;
	private Vector3 explosionPos;
	private Vector2 size = new Vector2(20.0f, 40.0f);
	private Vector3 mousePos;
	
	private bool isCharging = false;
	private bool isBlueTeam;
	private float progress = 0.0f;
	
	public Texture2D chargeBarEmpty;
	public Texture2D chargeBarFull;
	
	public GameObject chargeHaloPF;
	
	private GameObject chargingHalo;
	private GameObject plane;
	
	// Use this for initialization
	void Start () 
	{
		smartFox = SmartFoxConnection.Connection;
		chargeBarEmpty = (Texture2D)Resources.Load("chargeBackground");
		plane = GameObject.FindGameObjectWithTag("Plane");
		
		isBlueTeam = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>().IsBlueTeam;
		
		if (isBlueTeam)
		{
			chargeBarFull = (Texture2D)Resources.Load("ChargeFullBlue");	
		}
		else
		{
			chargeBarFull = (Texture2D)Resources.Load("ChargeFullRed");	
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		handleCameraMovement();
		
		RaycastHit hit;
		if (Input.GetMouseButtonDown (0))
        {
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
			
			Vector3 chargePos = explosionPos;
			chargePos.y += 0.1f;
			
			chargingHalo = (Instantiate(chargeHaloPF, chargePos, Quaternion.identity) as GameObject);
			
			if (isBlueTeam)
			{
				chargingHalo.GetComponent<Light>().color = Color.blue;	
			}
			else
			{
				chargingHalo.GetComponent<Light>().color = Color.red;	
			}
		}
		if (Input.GetMouseButtonUp (0))
		{
			float totalCharge =  (Time.time - startHold) * CHARGE_MULTIPLIER;
			
			if (totalCharge > 160.0f)
			{
				totalCharge = 160.0f;	
			}
			
			Destroy(chargingHalo);
			sendExplosionForce(explosionPos, totalCharge);
			isCharging = false;
			Screen.showCursor = true;
			
		}
		if (isCharging)
		{
			progress = (Time.time - startHold)/MAX_TIME;
		}
	}
	
	private void handleCameraMovement() {
		if(isCharging)
			return;
		
		if(Input.GetKey(KeyCode.W)) {
			transform.Translate(new Vector3(0, CAMERA_SPEED, 0));
		}
		if(Input.GetKey(KeyCode.A)) {
			transform.Translate(new Vector3(-CAMERA_SPEED, 0, 0));
		}
		if(Input.GetKey(KeyCode.S)) {
			transform.Translate(new Vector3(0, -CAMERA_SPEED, 0));
		}
		if(Input.GetKey(KeyCode.D)) {
			transform.Translate(new Vector3(CAMERA_SPEED, 0, 0));
		}
		var bounds = plane.collider.bounds;
		float x = Mathf.Clamp (transform.position.x, bounds.min.x, bounds.max.x);
        float z = Mathf.Clamp (transform.position.z, bounds.min.z, bounds.max.z);
        transform.position = new Vector3(x, transform.position.y, z);
	}
	
	void OnGUI()
	{
		if (isCharging)
		{
			GUI.DrawTexture(new Rect(mousePos.x, Screen.height - mousePos.y, size.x, -size.y * Mathf.Clamp01(progress)), chargeBarFull, ScaleMode.StretchToFill);
			GUI.DrawTexture(new Rect(mousePos.x, Screen.height - mousePos.y - size.y, size.x, size.y), chargeBarEmpty);
			
			float haloProgress = progress;
			if (haloProgress > 1.0f)
			{
				haloProgress = 1.0f;	
			}
			chargingHalo.GetComponent<Light>().range = 40.0f * haloProgress;
		}
	}
	
	void sendExplosionForce(Vector3 location, float force)
	{
		ISFSObject sendExplosion = new SFSObject();
		sendExplosion.PutUtfString("type", "explosion");
		if (isBlueTeam)
		{
			sendExplosion.PutUtfString("team", "blue");	
		}
		else
		{
			sendExplosion.PutUtfString("team", "red");
		}
		sendExplosion.PutFloat("force", force);
		sendExplosion.PutFloatArray("pos", new[] {location.x, location.z});
		smartFox.Send(new ObjectMessageRequest(sendExplosion, null, smartFox.LastJoinedRoom.UserList));
	}
}