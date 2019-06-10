using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScorerEvent : SimpleAwardEvent
{
    public float targetTime = 45f;
    public new void Start()
    {
        input.GameFinished += Input_GameFinished;
    }

    private void Input_GameFinished(CameraController cc, int eventID)
    {
        if (RoomManager.instance.roomTimer.TimeAllotted - targetTime <= RoomManager.instance.roomTimer.wallTimer.TimeToCountTo - Time.time)
        {
            cc.AM.AwardAchievement(ID, title, desc);
        }
    }
}
