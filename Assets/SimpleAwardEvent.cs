using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAwardEvent : MonoBehaviour
{
    public GameLock input;
    public string title;
    [TextArea]public string desc;
    public int ID;
    public void Start()
    {
        input.GameFinished += Input_GameFinished;
    }

    private void Input_GameFinished(CameraController cc, int eventID)
    {
        cc.AM.AwardAchievement(ID, title, desc);
    }
}
