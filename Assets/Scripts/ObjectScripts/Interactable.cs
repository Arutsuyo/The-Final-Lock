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

    private GlowObject glow;
    private bool inView;

    public long owner = -10; // Not used
    private void Awake()
    {
        glow = gameObject.GetComponent<GlowObject>();
        if (!glow)
            Debug.LogError("Attach a GlowObject Script to " + gameObject.name);
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
    [ClientRpc]
    public void RpcPickupContest(long winnerUUID)
    {
        Debug.Log("Pickup contest :(");
        if (winnerUUID != CampaignManagerMP.instance.nm.PLAYERUUID)
        {
            // Didn't pick it up :( 
            escapeInteractEvent();
        }
        else
        {
            // :D
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
    
        if(succ)
        {
            
            CmdTryPickUp(CampaignManagerMP.instance.nm.PLAYERUUID);
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