using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeLock : MonoBehaviour
{
    public bool solved = false;
    public bool left = false;
    public bool mid = false;
    public bool right = false;

    public string leftID;
    public string midID;
    public string rightID;

    public Transform leftInsert;
    public Transform midInsert;
    public Transform rightInsert;

    public Transform leftRest;
    public Transform midRest;
    public Transform rightRest;

    public int insertSpeed;

    public Interactable ia;

    void Awake()
    {
        Subscribe();
    }

    public void Animate(Transform insert, Transform rest, GameObject key)
    {
        float StartLerpTime = Time.time;
        float CurLerpTime = Time.time;
        key.transform.position = insert.position;
        while (CurLerpTime - StartLerpTime < insertSpeed)
        {
           key.transform.position = Vector3.Lerp(key.transform.position, rest.position, (CurLerpTime - StartLerpTime) / insertSpeed);
            CurLerpTime = Time.time;
        }
    }

    public void Subscribe()
    {
        // Make sure it's not null
        if (ia)
        {
            // Subscribe to the event
            ia.lookEvent += LookedAt;
            ia.interactEvent += Interacted;
            //ia.escapeInteractEvent += Ejection;
            //ia.gameInteractComplete += OpenTheDoor;
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
            //ia.escapeInteractEvent -= Ejection;
            //ia.gameInteractComplete -= OpenTheDoor;
        }
    }

    private void LookedAt(CameraController cc)
    {

    }

    private bool Interacted(CameraController cc)
    {
        // Check inventory for items

        // Update items in puzzle

        // Remove items from inventory

        // Call insert animations 


        if (solved)
            return false;
        return true;
    }
}