using UnityEngine;
using System.Collections;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Variables;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using Sfs2X.Logging;

public class PlayerNetwork : MonoBehaviour {
	private SmartFox smartFox;

	// Use this for initialization
	void Start () {
		smartFox = SmartFoxConnection.Connection;
	}
	
	// Update is called once per frame
	void Update () {
	}
}
