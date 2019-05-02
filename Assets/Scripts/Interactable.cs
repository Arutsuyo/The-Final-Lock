using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Add this script to whatever gameobject we want to be able to interact with. 
// The events can be subscribed to or unsubscribe at any point, making it 
// fairly dynamic. Just add the code snippet at the bottom to whatever object we 
// wish to interact with and call subscribe. Unsubscribe is self-explanatory.
public class Interactable : MonoBehaviour
{
    // This is what the event subscriber should look like
    public delegate void EventHandler();

    // These are the 2 events, triggered by the player camera raycast
    public event EventHandler lookEvent = delegate { };
    public event EventHandler interactEvent = delegate { };

    // Fires Interact event
    public void Interact()
    {
        //Debug.Log("Interacted with " + gameObject.name);
        interactEvent();
    }

    // Fires Look event
    public void LookingAt()
    {
        //Debug.Log("Looking at " + gameObject.name);
        lookEvent();
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
