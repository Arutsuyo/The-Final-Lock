using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleComponentEvent : MonoBehaviour
{
    public GameLock locks;
    public Behaviour[] toEnable;
    public Behaviour[] toDisable;
    public bool toggle = false;

    // Start is called before the first frame update
    void Start()
    {
        locks.GameFinished += Locks_GameFinished;
        locks.GameStateSet += Locks_Set;
        locks.GameStateToggle += Locks_Toggle;
    }

    private void Locks_GameFinished(CameraController cc, int _)
    {
        // Dispatch a game finished... o-o
        foreach (Behaviour go in toDisable)
        {
            go.enabled = false;
        }

        foreach (Behaviour go in toEnable)
        {
            go.enabled = true;
        }
            
    }
    private void Locks_Set(CameraController cc, bool state, int _)
    {
        if (toggle == state)
        {
            return;
        }
        foreach (Behaviour go in toDisable)
            go.enabled = (!state);

        foreach (Behaviour go in toEnable)
            go.enabled = (state);
    }
    private void Locks_Toggle(CameraController cc, int _)
    {
        toggle = !toggle;
        foreach (Behaviour go in toDisable)
            go.enabled = (!toggle);

        foreach (Behaviour go in toEnable)
            go.enabled = (toggle);
    }
}
