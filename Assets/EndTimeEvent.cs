using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTimeEvent : MonoBehaviour
{
    public GameLock exe;
    public WallTimer ti;
    // Start is called before the first frame update
    void Start()
    {
        ti.TimerExpired += Ti_TimerExpired;
    }

    private void Ti_TimerExpired()
    {
        exe.GFinished(RoomManager.instance.Player.cam);
    }
}
