
using System;
using System.Collections;
using UnityEngine;
using Sfs2X.Entities.Data;

// This class holds transform values and can load and send the data to server
public class NetworkTransform
{
	
	private Vector3 position; // Position as Vector3
	private Vector3 angleRotation; // Rotation as 3 euler angles - x, y, z
			
	public Vector3 Position {
		get {
			return position;
		}
	}
		
	public Vector3 AngleRotation {
		get {
			return angleRotation;
		}
	}
	
	// Check if this transform is different from given one with specified accuracy
	public bool IsDifferent(Transform transform, float accuracy) {
		float posDif = Vector3.Distance(this.position, transform.position);
		float angDif = Vector3.Distance(this.AngleRotation, transform.localEulerAngles);
		
		return (posDif>accuracy || angDif > accuracy);
	}
	
	// Stores the transform values to SFSObject to send them to server
	public static void ToSFSObject(ISFSObject data, Vector3 pos, Vector3 angles) {
		ISFSObject tr = new SFSObject();
				
		data.PutFloat("x", pos.x);
		data.PutFloat ("y", pos.y);
		data.PutFloat ("z", pos.z);
		
		data.PutFloat("rx", angles.x);
		data.PutFloat ("ry", angles.y);
		data.PutFloat ("rz", angles.z);
	}
	
	// Copies another NetworkTransform to itself
	public void Load(NetworkTransform ntransform) {
		this.position = ntransform.position;
		this.angleRotation = ntransform.angleRotation;
	}
	
	// Copy the Unity transform to itself
	public void UpdateTransform(Transform trans) {
		trans.position = this.Position;
		trans.localEulerAngles = this.AngleRotation;
	}
	
	// Creating NetworkTransform from SFS object
	public static NetworkTransform FromSFSObject(ISFSObject data) {
		NetworkTransform trans = new NetworkTransform();
		ISFSObject transformData = data.GetSFSObject("transform");
		
		float x = transformData.GetFloat("x");
		float y = transformData.GetFloat ("y");
		float z = transformData.GetFloat ("z");
		
		float rx = transformData.GetFloat ("rx");
		float ry = transformData.GetFloat ("ry");
		float rz = transformData.GetFloat ("rz");
					
		trans.position = new Vector3(x, y, z);
		trans.angleRotation = new Vector3(rx, ry, rz);

		return trans;
	}
	
	// Creating NetworkTransform from Unity transform
	public static NetworkTransform FromTransform(Transform transform) {
		NetworkTransform trans = new NetworkTransform();
				
		trans.position = transform.position;
		trans.angleRotation = transform.localEulerAngles;
				
		return trans;
	}
	
	// Clone itself
	public static NetworkTransform Clone(NetworkTransform ntransform) {
		NetworkTransform trans = new NetworkTransform();
		trans.Load(ntransform);
		return trans;
	}
	
}
