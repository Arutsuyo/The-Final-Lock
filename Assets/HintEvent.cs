using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintEvent : MonoBehaviour
{
    public GameLock enableHint;
    public GameLock disableHint;

    public string[] hints;
    public bool finished = false; // 
    void Start()
    {
        enableHint.GameFinished += EnableHint_GameFinished;
        if (disableHint != null)
        {
            disableHint.GameFinished += DisableHint_GameFinished;
        }
    }

    private void DisableHint_GameFinished(CameraController cc, int eventID)
    {
        if (finished)
        {
            foreach (string s in hints) {
                cc.hintRemoved[cc.hintPos[s]] = true;
                cc.availableHints.Remove(cc.hintPos[s]);
            }
            cc.RecalculateHints();
            return;
        }
        finished = true;
        
    }

    private void EnableHint_GameFinished(CameraController cc, int eventID)
    {
        if(finished == true)
        {
            // No more hints! Already "removed".
            return;
        }
        finished = true;
        foreach (string s in hints)
        {
            cc.hints.Add(s);
            cc.hintPos.Add(s, cc.hints.Count - 1);
            cc.hintRemoved.Add(false);
            cc.decodedPercent.Add(cc.hints.Count - 1, 0);
            cc.availableHints.Add(cc.hints.Count - 1);
        }
        cc.RecalculateHints(); // Just simply allows the hint controller to actually do its thing...
    }
}
