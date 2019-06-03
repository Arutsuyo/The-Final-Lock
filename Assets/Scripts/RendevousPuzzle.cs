using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RendevousPuzzle : MonoBehaviour
{
    public GameLock trigger;
    public GameLock toFireUpon;
    public Collider CubeToCheck;
    public bool initState = false;
    public bool checkOnlyOnTrue = false;
    public Interactable fInt;
    public bool FO = false;
    public bool temp = false;
    // Literally we try to finish....
    public void Start()
    {
        trigger.GameStateToggle += Trigger_GameStateToggle;
        fInt.gameInteractComplete += FInt_gameInteractComplete;
    }

    private void FInt_gameInteractComplete()
    {
        FO = true;
        toFireUpon.GFinished(RoomManager.instance.Player.cam);
    }

    private void Trigger_GameStateToggle(CameraController cc, int _)
    {
        if (temp) { temp = false;  return; } // To prevent loopback :|
        if (FO)
        {
            return;
        }
        // Time to check :D

        initState = !initState;
        if ((checkOnlyOnTrue && initState) || !checkOnlyOnTrue)
        {
            // Check the bounding box for all player objects....
            GameObject[] g = GameObject.FindGameObjectsWithTag("Player");
            // If players are somehow "killed", they should be set to "spectator" tag...
            foreach(GameObject gi in g)
            {
                Debug.Log(gi.transform.position + " " + CubeToCheck.bounds);
                if (!CubeToCheck.bounds.Contains(gi.transform.position))
                {
                    Debug.Log("Not everyone is here D:");
                    trigger.GToggleState(cc);
                    temp = true;
                    return;
                }
            }
            toFireUpon.GToggleState(cc);
            FO = true;
            fInt.SendSF();
        }
    }
}
