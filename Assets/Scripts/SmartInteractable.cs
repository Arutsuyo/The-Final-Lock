using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SmartInteractable : Interactable
{
    public bool canInteract = false;
    [HideInInspector] public bool origInter = false;
    public override event EventHandler lookEvent = delegate { };
    public override event TryEventHandler interactEvent;
    public override event SimpleEventHandler escapeInteractEvent = delegate { };
    public override event UpdateHandler updateEvent = delegate { };
    public override event SimpleEventHandler gameInteractComplete = delegate { };
    public override event SimpleEventHandler interactDestroyed = delegate { };

    public GameLock inputLocker;

    public void Start()
    {
        inputLocker.GameFinished += InputLocker_GameFinished;
        inputLocker.GameStateSet += InputLocker_GameStateSet;
        inputLocker.GameStateToggle += InputLocker_GameStateToggle;
        origInter = canInteract;
    }

    private void InputLocker_GameStateToggle(CameraController cc, int eventID)
    {
        canInteract = !canInteract;
    }

    private void InputLocker_GameStateSet(CameraController cc, bool state, int eventID)
    {
        canInteract = state;
    }

    private void InputLocker_GameFinished(CameraController cc, int eventID)
    {
        canInteract = !origInter;
    }

    [ClientRpc]
    public override void RpcServerFinished()
    {
        gameInteractComplete();
    }
    public override void Interact(CameraController cc)
    {
        Debug.LogWarning("INTERACTING!!!" + canInteract);
        if (canInteract)
        {
            Debug.Log("Hi?");
            bool succ = false;
            if (interactEvent != null)
                succ = interactEvent(cc);
            if (!RoomManager.instance.CMMP.nm.net.IsConnected())
            {
                succ = false;
            }
            if (succ)
            {
                // Instead, send a packet. 
                Debug.Log("PLAYER UUID: " + CampaignManagerMP.instance.nm.PLAYERUUID);
                RoomManager.instance.CMMP.nm.net.SendToServer(MPMsgTypes.Interactions, new InteractablePacket() { objectID = (int)this.netId.Value, playerRequesting = CampaignManagerMP.instance.nm.PLAYERUUID });
                //this.netId.Value

                //CmdTryPickUp(CampaignManagerMP.instance.nm.PLAYERUUID);
            }
        }
    }
}
