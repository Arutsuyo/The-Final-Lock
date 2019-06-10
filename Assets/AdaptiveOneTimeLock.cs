using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptiveOneTimeLock : MonoBehaviour
{
    public GameLock flipableSource;
    public GameLock finalizedDest;
    public bool oneshot = false;
    public void Start()
    {
        flipableSource.GameStateToggle += FlipableSource_GameStateToggle;
    }

    private void FlipableSource_GameStateToggle(CameraController cc, int eventID)
    {
        if (oneshot) { return; }
        oneshot = true;
        finalizedDest.GFinished(cc);
    }
}
