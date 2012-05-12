using UnityEngine;
using System.Collections.Generic;

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
    private int _redPoints, _redTotal, _bluePoints, _blueTotal;

	// Use this for initialization
	void Start () {
		smartFox = SmartFoxConnection.Connection;
		
		_redPoints = smartFox.LastJoinedRoom.GetVariable("redStored").GetIntValue();
		_redTotal = smartFox.LastJoinedRoom.GetVariable("redTotalScore").GetIntValue() + _redPoints;
		_bluePoints = smartFox.LastJoinedRoom.GetVariable("blueStored").GetIntValue();
		_blueTotal = smartFox.LastJoinedRoom.GetVariable("blueTotalScore").GetIntValue() + _bluePoints;
		
		_roundString = string.Format("Round {0}/{1} completed",
			smartFox.LastJoinedRoom.GetVariable("currentRound").GetIntValue(),
			smartFox.LastJoinedRoom.GetVariable("rounds").GetIntValue());
		_redString = string.Format("Red Team: {0} points ({1} points this round)", _redTotal, _redPoints);
		_blueString = string.Format("Blue Team: {0} points ({1} points this round)", _blueTotal, _bluePoints);
		_winnerString = _redTotal > _blueTotal ? "Red won!" : _redTotal == _blueTotal ? "'Twas a tie!" : "Blue won!";
		_matchOver = smartFox.LastJoinedRoom.GetVariable("currentRound").GetIntValue()
			== smartFox.LastJoinedRoom.GetVariable("rounds").GetIntValue();

        smartFox.AddEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, onRoomVarUpdate);
	}

    void OnDestroy() {
        smartFox.RemoveEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, onRoomVarUpdate);
    }
	
	// Update is called once per frame
	void Update () {
	}

    private void onRoomVarUpdate(BaseEvent evt) {
        Debug.Log("Got a room var update");
        if(!smartFox.LastJoinedRoom.ContainsVariable("countdownToggle")) {
            Application.LoadLevel(Application.loadedLevel);
            Destroy(this);
        }
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

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
		
		if(IsLowestID()) {
			if(!_matchOver && GUILayout.Button("Next Round")) {
                var toggle = new SFSRoomVariable("countdownToggle", null);
                var otherVars = new List<RoomVariable>();
                otherVars.Add(new SFSRoomVariable("currentRound", smartFox.LastJoinedRoom.GetVariable("currentRound").GetIntValue() + 1));
                otherVars.Add(new SFSRoomVariable("redTotalScore", _redTotal));
                otherVars.Add(new SFSRoomVariable("redStored", 0));
                otherVars.Add(new SFSRoomVariable("blueTotalScore", _blueTotal));
                otherVars.Add(new SFSRoomVariable("blueStored", 0));
                //This needs to be done in two requests
                smartFox.Send(new SetRoomVariablesRequest(new [] {toggle}));
                smartFox.Send(new SetRoomVariablesRequest(otherVars));
			}
		}

        if(_matchOver && GUILayout.Button("Back to Lobby")) {
            smartFox.Send(new JoinRoomRequest("The Lobby"));
        }
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}

	//TODO: This should be refactored.
	private bool IsLowestID()
	{
		int lowestUserID = int.MaxValue;
		int myID = smartFox.MySelf.GetPlayerId(smartFox.LastJoinedRoom);
		
		foreach (User u in smartFox.LastJoinedRoom.UserList) {
			int userIDToCheck = u.GetPlayerId(smartFox.LastJoinedRoom);
			if (userIDToCheck < lowestUserID)
				lowestUserID = userIDToCheck;
		}
		return myID == lowestUserID;
	}
}
