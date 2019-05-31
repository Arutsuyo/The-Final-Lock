using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHopperEvent : MonoBehaviour
{
    public GameLock locks;
    public GameLock[] toHopTo;

    void Start()
    {
        locks.GameFinished += Locks_GameFinished;
        locks.GameStateSet += Locks_Set;
        locks.GameStateToggle += Locks_Toggle;
    }

    private void Locks_GameFinished(CameraController cc)
    {
        foreach(GameLock g in toHopTo)
        {
            g.GFinished(cc);
        }
    }
    private void Locks_Set(CameraController cc, bool state)
    {
        foreach(GameLock g in toHopTo)
        {
            g.GSetState(cc, state);
        }
    }
    private void Locks_Toggle(CameraController cc)
    {
        foreach(GameLock g in toHopTo)
        {
            g.GToggleState(cc);
        }
    }
}
