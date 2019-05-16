using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
#pragma warning disable CS0618 // Type or member is obsolete
// Automatically adds the script to the gameobject

// Add this script to whatever gameobject we want to be able to interact with. 
// The events can be subscribed to or unsubscribe at any point, making it 
// fairly dynamic. Just add the code snippet at the bottom to whatever object we 
// wish to interact with and call subscribe. Unsubscribe is self-explanatory.
[RequireComponent(typeof(GlowObject))]
[RequireComponent(typeof(NetworkIdentity))]
public class Interactable : NetworkBehaviour
{
    // This is what the event subscriber should look like
    public delegate void EventHandler(CameraController cc);
    public delegate bool TryEventHandler(CameraController cc);
    public delegate void SimpleEventHandler();

    // These are the 2 events, triggered by the player camera raycast
    public event EventHandler lookEvent = delegate { };
    public event TryEventHandler interactEvent;
    public event SimpleEventHandler escapeInteractEvent = delegate { };

    public event SimpleEventHandler gameInteractComplete = delegate { };

    private GlowObject glow;
    private bool inView;
    public bool NeedCheckOwner = true;
    private bool hasAwaked = false;
    public long owner = -10; // Not used
    private void Awake()
    {
        glow = gameObject.GetComponent<GlowObject>();
        if (!glow)
            Debug.LogError("Attach a GlowObject Script to " + gameObject.name);
    }
    [ClientRpc]
    public void RpcServerFinished()
    {
        gameInteractComplete();
    }
    [Command]
    public void CmdServerFinished(long ID)
    {
        if(ID == owner || !NeedCheckOwner)
        {
            CmdReleaseHold();
            RpcServerFinished();
        }
        else
        {
            Debug.Log("Excuse me what the fuck? Hacker "+ ID + " go home you got caught.");
        }
    }

    public void SendSF()
    {
        RoomManager.instance.CMMP.nm.net.SendToServer(MPMsgTypes.FinInteractions, new InteractablePacket() { objectID = (int)this.netId.Value, playerRequesting = CampaignManagerMP.instance.nm.PLAYERUUID });
    }

    public void Update()
    {
        if (!hasAwaked)
        {
            hasAwaked = true;
            if (RoomManager.instance.interactables.ContainsKey(this.netId.Value))
            {
                Debug.Log("Detecting that not a server!");
                return;
            }
            RoomManager.instance.interactables.Add(this.netId.Value, this);
        }
    }
    

    private void LateUpdate()
    {
        if (!inView)
            glow.DisableGlow();
        else
            inView = false;
    }
    [Command]
    public void CmdTryPickUp(long PLAYERUUID)
    {
        Debug.Log("Player " + PLAYERUUID + " has contested!");
        if(owner == -10)
        {
            owner = PLAYERUUID;
            RpcPickupContest(PLAYERUUID);
        }
        else
        {
            RpcPickupContest(owner);
        }
    }
    public void Finished()
    {

    }
    [ClientRpc]
    public void RpcPickupContest(long winnerUUID)
    {
        Debug.Log("Pickup contest :(" + winnerUUID + " " + CampaignManagerMP.instance.nm.PLAYERUUID);
        owner = winnerUUID;
        if (winnerUUID != CampaignManagerMP.instance.nm.PLAYERUUID)
        {
            // Didn't pick it up :( 
            escapeInteractEvent();
            Debug.Log("I have to put it down :(");
        }
        else
        {
            // :D
            Debug.Log("I got to pick it up :D");
        }
    }
    [ClientRpc]
    public void RpcReleaseHold()
    {
        owner = -10;
    }
    [Command]
    public void CmdReleaseHold()
    {
        owner = -10;
        RpcReleaseHold();
    }

    // Fires Interact event
    public void Interact(CameraController cc)
    {
        //Debug.Log("Interacted with " + gameObject.name);
        bool succ = false;
        if(interactEvent!= null)
            succ = interactEvent(cc);
        if (!RoomManager.instance.CMMP.nm.net.IsConnected())
        {
            succ = false;
        }
        if(succ)
        {
            // Instead, send a packet. 
            Debug.Log("PLAYER UUID: " + CampaignManagerMP.instance.nm.PLAYERUUID);
            RoomManager.instance.CMMP.nm.net.SendToServer(MPMsgTypes.Interactions, new InteractablePacket() {objectID = (int)this.netId.Value, playerRequesting = CampaignManagerMP.instance.nm.PLAYERUUID });
            //this.netId.Value

            //CmdTryPickUp(CampaignManagerMP.instance.nm.PLAYERUUID);
        }
    }

    // Fires Look event
    public void LookingAt(CameraController cc)
    {
        //Debug.Log("Looking at " + gameObject.name);
        lookEvent(cc);
        glow.EnableGlow();
        inView = true;
    }
}

public class InteractablePacket : TaggedMessageBase
{
    public int objectID;
    public long playerRequesting;

    public override void Deserialize(NetworkReader reader)
    {
        TaggedMessageBase.BaseDeserialize(reader, this);
        objectID = reader.ReadInt32();
        playerRequesting = reader.ReadInt64();
    }

    public override void Serialize(NetworkWriter write)
    {
        TaggedMessageBase.BaseSerialize(write, this);
        write.Write(objectID);
        write.Write(playerRequesting);
    }
}


/* How to subscribe to events!
 * Add this code snippet to the script you want to handle the event, and call
 * subscribe in Start() or whenever you want to enable interacting!
    
    public void Subscribe()
    {
        // Get the Interactable script reference
        Interactable ia = gameObject.GetComponent<Interactable>();

        // Make sure it's not null
        if (ia)
        {
            // Subscribe to the event
            ia.lookEvent += LookedAt;
            ia.interactEvent += Interacted;
        }
    }

    public void Unsubscribe()
    {
        // Get the Interactable script reference
        Interactable ia = gameObject.GetComponent<Interactable>();

        // Make sure it's not null
        if (ia)
        {
            // Unsubscribe to the event
            ia.lookEvent -= LookedAt;
            ia.interactEvent -= Interacted;
        }
    }

    private void LookedAt()
    {
        // Handle event. . . Set whatever you want. . .
        // Call whatever function you want. . .
    }

    private void Interacted()
    {
        // Handle event. . . Set whatever you want. . .
        // Call whatever function you want. . . 
    }
*/

#pragma warning restore CS0618 // Type or member is obsolete