using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeKey : MonoBehaviour
{
    public string ID;

    public Interactable ia;

    public void Awake()
    {
        Subscribe();
    }

    public void Subscribe()
    {
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
        // Make sure it's not null
        if (ia)
        {
            // Unsubscribe to the event
            ia.lookEvent -= LookedAt;
            ia.interactEvent -= Interacted;
        }
    }

    private void LookedAt(CameraController cc)
    {

    }

    private bool Interacted(CameraController cc)
    {
        // Hide item
        // add to inventory

        return true;
    }
}
