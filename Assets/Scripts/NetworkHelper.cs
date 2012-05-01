using System;
using System.Collections;
using UnityEngine;
using Sfs2X.Entities.Data;

public class NetworkHelper {
	public static ISFSObject TransformToSFSObject(Vector3 pos, Vector3 angles) {
		ISFSObject data = new SFSObject();
		
		data.PutUtfString("type", "transform");
		data.PutFloat("x", pos.x);
		data.PutFloat("y", pos.y);
		data.PutFloat("z", pos.z);
		
		data.PutFloat("rx", angles.x);
		data.PutFloat("ry", angles.y);
		data.PutFloat("rz", angles.z);
		return data;
	}
	
	public static ISFSObject MakeSFSObject(string type, params dynamic[] args) {
		return new SFSObject();
	}
	
	public static void SFSObjectToTransform(ISFSObject data, Transform trans) {
		trans.position = GetSFSTransform(data);
		trans.localEulerAngles = GetSFSRotation(data);
	}
	
	public static Vector3 GetSFSTransform(ISFSObject data) {
		float x = data.GetFloat("x");
		float y = data.GetFloat("y");
		float z = data.GetFloat("z");
		
		return new Vector3(x,y,z);
	}
	
	public static Vector3 GetSFSRotation(ISFSObject data) {
		float rx = data.GetFloat("rx");
		float ry = data.GetFloat("ry");
		float rz = data.GetFloat("rz");
		
		return new Vector3(rx, ry, rz);
	}
}
