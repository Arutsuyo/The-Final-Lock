using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public delegate void EventHandler();

    public event EventHandler lookEvent = delegate { };
    public event EventHandler interactEvent = delegate { };

    public void Interact()
    {
        Debug.Log("Interacted with " + gameObject.name);
        interactEvent();
    }

    public void LookingAt()
    {
        Debug.Log("Looking at " + gameObject.name);
        lookEvent();
    }
}


/* How to subscribe to events!
 * Add this code snippet to the script you want to handle the event, and call
 * subscribe in Start()
    public void Subscribe()
    {
        Interactable ia = gameObject.GetComponent<Interactable>();
        if (ia)
        {
            ia.lookEvent += () => { LookedAt(); };
            ia.interactEvent += () => { Interacted(); };
        }
    }

    private void LookedAt()
    {
        //Handle event.....

    }

    private void Interacted()
    {
        //Handle event.....

    }
*/
