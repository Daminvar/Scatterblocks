using UnityEngine;
using System.Collections;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Variables;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using Sfs2X.Logging;

public class ResultsScreen : MonoBehaviour {
	private SmartFox smartFox;
	private bool _matchOver;
	private string _roundString;
	private string _redString;
	private string _blueString;
	private string _winnerString;

	// Use this for initialization
	void Start () {
		smartFox = SmartFoxConnection.Connection;
		
		int redPoints = smartFox.LastJoinedRoom.GetVariable("redStored").GetIntValue();
		int redTotal = smartFox.LastJoinedRoom.GetVariable("redTotalScore").GetIntValue();
		int bluePoints = smartFox.LastJoinedRoom.GetVariable("blueStored").GetIntValue();
		int blueTotal = smartFox.LastJoinedRoom.GetVariable("blueTotalScore").GetIntValue();
		
		_roundString = string.Format("Round {0}/{1} completed",
			smartFox.LastJoinedRoom.GetVariable("currentRound").GetIntValue(),
			smartFox.LastJoinedRoom.GetVariable("rounds").GetIntValue());
		_redString = string.Format("Red Team: {0} points ({1} points this round)", redTotal, redPoints);
		_blueString = string.Format("Blue Team: {0} points ({1} points this round)", blueTotal, bluePoints);
		_winnerString = redTotal > blueTotal ? "Red won!" : redTotal == blueTotal ? "'Twas a tie!" : "Blue won!";
		_matchOver = smartFox.LastJoinedRoom.GetVariable("currentRound").GetIntValue()
			== smartFox.LastJoinedRoom.GetVariable("rounds").GetIntValue();
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	void OnGUI() {
		var style = new GUIStyle();
		style.fontSize = 24;
		style.normal.textColor = Color.white;
		
		GUILayout.BeginArea(new Rect(50, 50, Screen.width - 100, Screen.height - 100));
		GUILayout.BeginVertical("box");
		GUILayout.Label("Results", style);
		GUILayout.Space(50);
		
		GUILayout.Label(_roundString, style);
		GUILayout.Label(_redString, style);
		GUILayout.Label(_blueString, style);
		GUILayout.FlexibleSpace();
		if(_matchOver)
			GUILayout.Label(_winnerString, style);
		GUILayout.EndVertical();
		
		if(smartFox.MySelf.Id == 1) {
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			
			if(GUILayout.Button("Next Round")) {
				var roomVar = new SFSRoomVariable("countdownToggle", null);
				smartFox.Send(new SetRoomVariablesRequest(new [] {roomVar}));
			}
			
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		
		GUILayout.EndArea();
	}
}
