using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Automatically adds the script to the gameobject
[RequireComponent(typeof(GlowObject))]

// Add this script to whatever gameobject we want to be able to interact with. 
// The events can be subscribed to or unsubscribe at any point, making it 
// fairly dynamic. Just add the code snippet at the bottom to whatever object we 
// wish to interact with and call subscribe. Unsubscribe is self-explanatory.
public class Interactable : MonoBehaviour
{
    // This is what the event subscriber should look like
    public delegate void EventHandler(CameraController cc);

    // These are the 2 events, triggered by the player camera raycast
    public event EventHandler lookEvent = delegate { };
    public event EventHandler interactEvent = delegate { };

    private GlowObject glow;
    private bool inView;

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

    // Fires Interact event
    public void Interact(CameraController cc)
    {
        //Debug.Log("Interacted with " + gameObject.name);
        interactEvent(cc);
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
