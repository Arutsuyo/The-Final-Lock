using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeLock : MonoBehaviour
{
    public bool solved = false;
    public bool left = false;
    public bool mid = false;
    public bool right = false;

    public GameObject LKey;
    public GameObject MKey;
    public GameObject RKey;

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
        key.SetActive(true);
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
        // Check inventory for items
        if (cc.playerMngr.inv.HasItem(leftID))
        {
            left = true;
            Animate(leftInsert, leftRest, LKey);
            cc.playerMngr.inv.SilentDelete(cc.playerMngr.inv.GetItem(leftID));
        }
        if (cc.playerMngr.inv.HasItem(midID))
        {
            mid = true;
            Animate(midInsert, midRest, MKey);
            cc.playerMngr.inv.SilentDelete(cc.playerMngr.inv.GetItem(midID));
        }
        if (cc.playerMngr.inv.HasItem(rightID))
        {
            right = true;
            Animate(rightInsert, rightRest, RKey);
            cc.playerMngr.inv.SilentDelete(cc.playerMngr.inv.GetItem(rightID));
        }
        if (left && mid && right)
            solved = true;

        if (solved)
            return false;
        return true;
    }
}