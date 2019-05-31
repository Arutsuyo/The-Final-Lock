using System.Collections;
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
    public AnimationCurve timeCurve;
    public GameObject LKey;
    public GameObject MKey;
    public GameObject RKey;
    public GameObject LFKey;
    public GameObject MFKey;
    public GameObject RFKey;

    public GameLock gameLock;
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
            key.transform.position = Vector3.Lerp(insert.position, rest.position, timeCurve.Evaluate((CurLerpTime - StartLerpTime) / insertSpeed));
            yield return null;
            CurLerpTime = Time.time;
            //Debug.Log("CurLerpTime: " + CurLerpTime + " StartLerpTime: " + StartLerpTime);
        }
        key.transform.position = rest.position;
    }


    private bool Interacted(CameraController cc)
    {
        if (solved)
            return false;
        // Check inventory for items
        bool anyChange = false;
        if (cc.playerMngr.inv.HasItem(leftID) && !left)
        {
            left = true;
            anyChange = true;
            StartCoroutine(Animate(leftInsert, leftRest, LFKey));
            cc.playerMngr.inv.GetItem(leftID).DestroyFromNetwork();// Should fire off a packet to delete this item from all clients...
        }
        if (cc.playerMngr.inv.HasItem(midID) && !mid)
        {
            mid = true;
            anyChange = true;
            StartCoroutine(Animate(midInsert, midRest, MFKey));
            cc.playerMngr.inv.GetItem(midID).DestroyFromNetwork();
        }
        if (cc.playerMngr.inv.HasItem(rightID) && !right)
        {
            right = true;
            anyChange = true;
            StartCoroutine(Animate(rightInsert, rightRest, RFKey));
            cc.playerMngr.inv.GetItem(rightID).DestroyFromNetwork();
        }
        if (anyChange)
        {
            ia.SendUpdate("" + (left ? 1 : 0) + "" + (mid ? 1 : 0) + "" + (right ? 1 : 0));
        }
        if (left && mid && right)
        {
            solved = true;
            ia.SendSF();
        }

        if (solved)
            return false;
        return true;
    }
    private void Updated(string status)
    {
        char[] xx = status.ToCharArray();
        if(xx[0]== '1' && !left )
        {
            left = true;
            StartCoroutine(Animate(leftInsert, leftRest, LFKey));
        }
        if (xx[1] == '1' && !mid)
        {
            mid = true;
            StartCoroutine(Animate(midInsert, midRest, MFKey));
        }
        if(xx[2] == '1' && !right)
        {
            right = true;
            StartCoroutine(Animate(rightInsert, rightRest, RFKey));
        }
    }
    private void Fin()
    {
        gameLock.GFinished(RoomManager.instance.Player.cam);
    }
    public void Subscribe()
    {
        // Make sure it's not null
        if (ia)
        {
            // Subscribe to the event
            ia.lookEvent += LookedAt;
            ia.interactEvent += Interacted;
            ia.updateEvent += Updated;
            ia.gameInteractComplete += Fin;
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
            ia.updateEvent -= Updated;
        }
    }



    private void LookedAt(CameraController cc)
    {

    }

}

#pragma warning restore CS0618 // Type or member is obsolete