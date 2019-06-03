using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LatchGameEvent : MonoBehaviour
{
    public GameLock waitForFinished; // We wait for this gameobject to fire the GFinished event
    public GameLock source; // The gamelock that sends the signals that may be blocked, DOES NOT BLOCK FINISHED EVENTS!!!!
    public GameLock dest;// The gamelock that will be relayed signals if not blocked here

    public bool canSendSignals = false; // When receiving the Finished, this negates
    [HideInInspector] public bool origSignal = false;

    public void Start()
    {
        origSignal = canSendSignals;
        waitForFinished.GameFinished += WaitForFinished_GameFinished;
        source.GameStateSet += Source_GameStateSet;
        source.GameStateToggle += Source_GameStateToggle;
    }

    private void Source_GameStateToggle(CameraController cc)
    {
    }

    private void Dest_GameStateToggle(CameraController cc)
    {
    }

    private void Source_GameStateSet(CameraController cc, bool state)
    {
        throw new System.NotImplementedException();
    }

    private void WaitForFinished_GameFinished(CameraController cc)
    {
        canSendSignals = !origSignal;
    }


}
