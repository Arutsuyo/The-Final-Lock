﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618 // Type or member is obsolete
public class ShapeLock : NetworkBehaviour
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

    public float insertSpeed;

    public Interactable ia;

    public delegate void OnLockReady();
    public event OnLockReady PuzzleReady = delegate { };

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Set starting Values
        left = false;
        mid = false;
        right = false;
        solved = false;
        
        PuzzleReady();
    }

    void Awake()
    {
        Subscribe();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        StartCoroutine(SCC());
    }

    IEnumerator SCC()
    {
        while (RoomManager.instance == null || RoomManager.instance.CMMP == null)
            yield return null;

        if (!RoomManager.instance.CMMP.nm.net.isHost)
            PuzzleReady();
    }

    void CC(SyncListInt.Operation op, int itemIndex)
    {
        if (!RoomManager.instance.CMMP.nm.net.isHost)
            StartCoroutine(SCC());
    }

    private IEnumerator Animate(Transform insert, Transform rest, GameObject key)
    {
        float StartLerpTime = Time.time;
        float CurLerpTime = Time.time;
        key.transform.position = insert.position;
        key.SetActive(true);
        while (CurLerpTime - StartLerpTime < insertSpeed)
        {
            key.transform.position = Vector3.Lerp(insert.position, rest.position, (CurLerpTime - StartLerpTime) / insertSpeed);
            yield return null;
            CurLerpTime = Time.time;
            Debug.Log("CurLerpTime: " + CurLerpTime + " StartLerpTime: " + StartLerpTime);
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
            StartCoroutine(Animate(leftInsert, leftRest, LKey));
            cc.playerMngr.inv.SilentDelete(cc.playerMngr.inv.GetItem(leftID));
        }
        if (cc.playerMngr.inv.HasItem(midID))
        {
            mid = true;
            StartCoroutine(Animate(midInsert, midRest, MKey));
            cc.playerMngr.inv.SilentDelete(cc.playerMngr.inv.GetItem(midID));
        }
        if (cc.playerMngr.inv.HasItem(rightID))
        {
            right = true;
            StartCoroutine(Animate(rightInsert, rightRest, RKey));
            cc.playerMngr.inv.SilentDelete(cc.playerMngr.inv.GetItem(rightID));
        }
        if (left && mid && right)
            solved = true;

        if (solved)
            return false;
        return true;
    }
}

#pragma warning restore CS0618 // Type or member is obsolete