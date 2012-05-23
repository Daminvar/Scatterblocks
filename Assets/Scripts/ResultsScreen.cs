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
	private bool isBlueTeam;
	private string _roundString;
	private string _redString;
	private string _blueString;
	private string _winnerString;
    private int _redPoints, _redTotal, _bluePoints, _blueTotal;
	private GUIStyle redResultsStyle;
	private GUIStyle blueResultsStyle;
	private GUIStyle titleStyle;
	private GUIStyle roundStyle;
	private GUIStyle boxStyle;
	private GUIStyle winnerStyle;
	private Color teamColor;
	
	private float startTime;
	private float lastUpdateTime;

	// Use this for initialization
	void Start () {
		isBlueTeam = gameObject.GetComponent<GameManager>().IsBlueTeam;
		
		redResultsStyle = new GUIStyle();
		redResultsStyle.fontSize = 24;
		redResultsStyle.alignment = TextAnchor.MiddleCenter;
		
		titleStyle = new GUIStyle();
		titleStyle.fontSize = 36;
		titleStyle.alignment = TextAnchor.MiddleCenter;
		titleStyle.normal.textColor = Color.white;
		
		roundStyle = new GUIStyle();
		roundStyle.fontSize = 24;
		roundStyle.alignment = TextAnchor.MiddleCenter;
		roundStyle.normal.textColor = Color.white;
		
		if (isBlueTeam)
		{
			//teamColor = new Color(68, 137, 223, 1.0f);
			teamColor = new Color(0.27f, 0.54f, 0.87f, 1.0f);
		}
		else
		{
			//teamColor = new Color(226, 40, 32, 1.0f);
			teamColor = new Color(0.87f, 0.16f, 0.13f, 1.0f);
		}
		
		winnerStyle = new GUIStyle();
		winnerStyle.fontSize = 36;
		winnerStyle.alignment = TextAnchor.MiddleCenter;
		winnerStyle.normal.textColor = teamColor;
		
		redResultsStyle.normal.textColor = Color.red;
		
		blueResultsStyle = new GUIStyle();
		blueResultsStyle.fontSize = 24;
		blueResultsStyle.alignment = TextAnchor.MiddleCenter;
		blueResultsStyle.normal.textColor = Color.blue;
		
		smartFox = SmartFoxConnection.Connection;
		
		_redPoints = smartFox.LastJoinedRoom.GetVariable("redStored").GetIntValue();
		_redTotal = smartFox.LastJoinedRoom.GetVariable("redTotalScore").GetIntValue();
		_bluePoints = smartFox.LastJoinedRoom.GetVariable("blueStored").GetIntValue();
		_blueTotal = smartFox.LastJoinedRoom.GetVariable("blueTotalScore").GetIntValue();
		
		_roundString = string.Format("Round {0}/{1} completed",
			smartFox.LastJoinedRoom.GetVariable("currentRound").GetIntValue(),
			smartFox.LastJoinedRoom.GetVariable("rounds").GetIntValue());
		_winnerString = _redTotal > _blueTotal ? "Red won!" : _redTotal == _blueTotal ? "'Twas a tie!" : "Blue won!";
		_matchOver = smartFox.LastJoinedRoom.GetVariable("currentRound").GetIntValue()
			== smartFox.LastJoinedRoom.GetVariable("rounds").GetIntValue();

        smartFox.AddEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, onRoomVarUpdate);
		
		startTime = Time.time;
		lastUpdateTime = startTime;
	}

    void OnDestroy() {
        smartFox.RemoveEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, onRoomVarUpdate);
    }
	
	// Update is called once per frame
	void Update () {
		if ( (Time.time - startTime) > 1 )
		{
			if ( (Time.time - lastUpdateTime) > 0.1)
				{
					if (_redPoints > 0 )
					{
						_redPoints -= 1;
						_redTotal += 1;
					}
					if (_bluePoints > 0)
					{
						_bluePoints -= 1;
						_blueTotal += 1;
					}
				}
		}
	}

    private void onRoomVarUpdate(BaseEvent evt) {
        Debug.Log("Got a room var update");
        if(!smartFox.LastJoinedRoom.ContainsVariable("countdownToggle")) {
            Application.LoadLevel(Application.loadedLevel);
            Destroy(this);
        }
    }
	
	void OnGUI() {
		GUILayout.BeginArea(new Rect(400, 50, Screen.width - 600, Screen.height - 100));
		GUILayout.BeginVertical("box");

		GUILayout.Label("Results", titleStyle);
		GUILayout.Space(20);
		
		_redString = string.Format("\nRed Team\n{0} points ({1} points this round)", _redTotal, _redPoints);
		_blueString = string.Format("\nBlue Team\n{0} points ({1} points this round)", _blueTotal, _bluePoints);
		
		GUILayout.Label(_roundString, roundStyle);
		GUILayout.Label(_redString, redResultsStyle);
		GUILayout.Label(_blueString, blueResultsStyle);
		GUILayout.FlexibleSpace();
		if(_matchOver)
			GUILayout.Label(_winnerString, winnerStyle);
		GUILayout.EndVertical();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
		
		if(NetworkHelper.IsLowestID(smartFox)) {
			if(!_matchOver && GUILayout.Button("Next Round")) {
                sendNewRoundInfo();
			}
		}

        if(_matchOver && GUILayout.Button("Back to Lobby")) {
            smartFox.Send(new JoinRoomRequest("The Lobby"));
        }
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}

    private void sendNewRoundInfo() {
        var toggle = new SFSRoomVariable("countdownToggle", null);
        var otherVars = new List<RoomVariable>();
        otherVars.Add(new SFSRoomVariable("currentRound", smartFox.LastJoinedRoom.GetVariable("currentRound").GetIntValue() + 1));
        otherVars.Add(new SFSRoomVariable("redTotalScore", _redTotal + _redPoints));
        otherVars.Add(new SFSRoomVariable("redStored", 0));
        otherVars.Add(new SFSRoomVariable("blueTotalScore", _blueTotal + _bluePoints));
        otherVars.Add(new SFSRoomVariable("blueStored", 0));
        otherVars.Add(new SFSRoomVariable("redRobot", getRandomPlayer(false)));
        otherVars.Add(new SFSRoomVariable("blueRobot", getRandomPlayer(true)));
        //This needs to be done in two requests
        smartFox.Send(new SetRoomVariablesRequest(new [] {toggle}));
        smartFox.Send(new SetRoomVariablesRequest(otherVars));
    }
	
	private string getRandomPlayer(bool blue) {
		ISFSArray players;
		if(blue)
			players = smartFox.LastJoinedRoom.GetVariable("blue").GetSFSArrayValue();
		else
			players = smartFox.LastJoinedRoom.GetVariable("red").GetSFSArrayValue();
		return players.GetUtfString(new System.Random().Next(0, players.Size() - 1));
	}
}
