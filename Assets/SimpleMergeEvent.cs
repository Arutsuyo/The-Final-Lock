using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMergeEvent : MonoBehaviour
{
    public GameLock[] sources;
    public GameLock[] dest;


    public bool isLoopback = false;
    private int MRCBL = -1;
    void Start()
    {
        foreach(GameLock g in sources)
        {
            g.GameFinished += G_GameFinished;
            g.GameStateSet += G_GameStateSet;
            g.GameStateToggle += G_GameStateToggle;
        }
        
    }

    private void G_GameStateToggle(CameraController cc, int eventID)
    {
        if (isLoopback && eventID == MRCBL)
        {
            return;
        }
        MRCBL = eventID;
        foreach(GameLock g in dest)
        {
            g.Decr(cc);
            g.GToggleState(cc);
        }
    }

    private void G_GameStateSet(CameraController cc, bool state, int eventID)
    {
        if (isLoopback && eventID == MRCBL)
        {
            return;
        }
        MRCBL = eventID;
        foreach (GameLock g in dest)
        {
            g.Decr(cc);
            g.GSetState(cc, state);
        }
    }

    private void G_GameFinished(CameraController cc, int eventID)
    {
        if (isLoopback && eventID == MRCBL)
        {
            return;
        }
        MRCBL = eventID;
        foreach (GameLock g in dest)
        {
            g.Decr(cc);
            g.GFinished(cc);
        }
    }
}
