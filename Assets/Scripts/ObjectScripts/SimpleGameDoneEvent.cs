using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGameDoneEvent : MonoBehaviour
{
    public GameLock locks;

    void Start()
    {
        locks.GameFinished += Locks_GameFinished;
    }
    private void Locks_GameFinished(CameraController cc)
    {
        if (RoomManager.instance.CMMP.nm.net.isHost)
        {
            RoomManager.instance.roomTimer.CmdStopTimer();
        }
    }
}
