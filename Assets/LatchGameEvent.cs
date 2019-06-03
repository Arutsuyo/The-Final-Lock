using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LatchGameEvent : MonoBehaviour
{
    public Interactable waitForFinished; // We wait for this gameobject to fire the GFinished event
    public GameLock source; // The gamelock that sends the signals that may be blocked, DOES NOT BLOCK FINISHED EVENTS!!!!
    public GameLock dest;// The gamelock that will be relayed signals if not blocked here
    public GameLock alerter; // Alerts any listeners that the latch is now in a different state.

    public bool canSendSignals = false; // When receiving the Finished, this negates
    [HideInInspector] public bool origSignal = false;

    public void Start()
    {
        origSignal = canSendSignals;
        waitForFinished.gameInteractComplete += WaitForFinished_GameFinished;
        source.GameStateSet += Source_GameStateSet;
        source.GameStateToggle += Source_GameStateToggle;
    }

    private void Source_GameStateToggle(CameraController cc, int _)
    {
        if (canSendSignals)
        {
            dest.GToggleState(cc);
        }
    }
    
    private void Source_GameStateSet(CameraController cc, bool state, int _)
    {
        if (canSendSignals)
        {
            dest.GSetState(cc, state);
        }
    }

    private void WaitForFinished_GameFinished()
    {
        canSendSignals = !origSignal;
        alerter.GFinished(RoomManager.instance.Player.cam);
    }


}
