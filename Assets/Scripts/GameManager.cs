using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
	public GameObject RTSCamera;
	public bool thirdPerson;

	// Use this for initialization
	void Start ()
	{
		if(thirdPerson)
		{
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
