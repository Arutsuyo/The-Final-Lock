using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickGame : MonoBehaviour
{

    public Interactable interact;
    public GameLock gameLock;
    public GameObject PinHolderPos;
    public GameObject[] toDelete;
    public Transform PickTrans;
    public int offsetID = 0;
    public float[] pinHeights;
    public int numPins;
    public bool[] isSecure;
    public float[] pinPositions;
    public float tumblerPosition;
    public float[] pinTolerance;
    public bool[] isLatched;
    public int testingPinID = 0;
    public int bindingPin = 0;
    private float[] origPinHeights;
    // Start is called before the first frame update
    public Transform pickPosition;
    private Vector3 prevPosition;
    private Quaternion prevRotation;
    public CameraController curPlayer;
    public float LerpTime = 0;
    private float CurLerpTime = 0;
    private float StartLerpTime = 0;
    public bool cutsceneFinished = false;
    public bool isPicked = false;
    public bool isTryingObject = false;
    public bool isKey = false;
    public Transform keyTransform;
    public Transform pickTransform;
    public Transform handleTransform;

    public LockPickUI PLUI;
    public EnableDisableOverTime PLEDOT;

    void Start()
    {
        interact.lookEvent += LookAt;
        interact.interactEvent += InterAt;
        interact.escapeInteractEvent += Eject;
        interact.gameInteractComplete += PickFin;
        pinHeights = new float[numPins];
        isSecure = new bool[numPins];
        isLatched = new bool[numPins];
        pinPositions = new float[numPins];
        pinTolerance = new float[numPins];
        origPinHeights = new float[numPins];
       
        tumblerPosition = 0;
        for(int i = 0; i < numPins; i++)
        {
            pinHeights[i] = Random.Range(0.3f, 0.9f);
            isSecure[i] = false;
            isLatched[i] = false;
            pinPositions[i] = Random.Range(0.0f, 1.0f);
            origPinHeights[i] = pinHeights[i];
            pinTolerance[i] = Random.Range(0.01f, 0.05f);
        }
    }
    IEnumerator PlayZoomInForward()
    {
        StartLerpTime = Time.time;
        CurLerpTime = Time.time;
        while(CurLerpTime - StartLerpTime < LerpTime)
        {
            curPlayer.cam.transform.position = Vector3.Lerp(prevPosition, pickPosition.position, (CurLerpTime - StartLerpTime)/LerpTime);
            curPlayer.cam.transform.rotation = Quaternion.Slerp(prevRotation, pickPosition.rotation, (CurLerpTime - StartLerpTime) / LerpTime);
            yield return null;
            CurLerpTime = Time.time;
        }
        curPlayer.cam.transform.position = pickPosition.position;
        curPlayer.cam.transform.rotation = pickPosition.rotation;
        curPlayer.AllowMouse();
        cutsceneFinished = true;
    }
    IEnumerator PlayZoomInBackward()
    {
        StartLerpTime = Time.time;
        CurLerpTime = Time.time;
        while (CurLerpTime - StartLerpTime < LerpTime)
        {
            curPlayer.cam.transform.position = Vector3.Lerp(prevPosition, pickPosition.position, 1.0f - ((CurLerpTime - StartLerpTime) / LerpTime));
            curPlayer.cam.transform.rotation = Quaternion.Slerp(prevRotation, pickPosition.rotation, 1.0f - ((CurLerpTime - StartLerpTime) / LerpTime));
            yield return null;
            CurLerpTime = Time.time;
        }
        curPlayer.cam.transform.position = prevPosition;
        curPlayer.cam.transform.rotation = prevRotation;
        curPlayer.isInCutscene = false;
        curPlayer.RestrictMouse();
        curPlayer = null;
    }
    private void LookAt(CameraController cc)
    {

    }
    private bool InterAt(CameraController cc)
    {
        if(isPicked) { return false; }
        
        bool hasPick = cc.playerMngr.inv.HasItem("KeyPick" + offsetID);
        bool hasHandle = cc.playerMngr.inv.HasItem("KeyHandle" + offsetID);
        bool hasKey = cc.playerMngr.inv.HasItem("Key" + offsetID);
        if (hasKey)
        {
            isKey = true;
            // Spawn the key :D
            Item ii = cc.playerMngr.inv.GetItem("Key" + offsetID);
            Item dj = Instantiate<Item>(ii, keyTransform);
            dj.enabled = false;
            dj.gameObject.SetActive(true);
            turnSpeed *= 30;
        }
        else
        {
            if (!hasPick)
            {
                Debug.Log("I would need the key or a pick to get it open.");
                return false;
            }
            else if (!hasHandle)
            {
                Debug.Log("I will need a tension wrench to try to force it open.");
                return false;
            }
            // Spawn the pick and handle..
            Item ii = cc.playerMngr.inv.GetItem("KeyPick" + offsetID);
            Item dj = Instantiate<Item>(ii, pickTransform);
            dj.enabled = false;
            Item ii1 = cc.playerMngr.inv.GetItem("KeyHandle" + offsetID);
            Item dj1 = Instantiate<Item>(ii1, handleTransform);
            dj1.enabled = false;
            dj1.gameObject.SetActive(true);
            dj.gameObject.SetActive(true);
            PLEDOT.StartChange();
            PLUI.height = 0;
            for (int i = 0; i < 8; i++) {
                PLUI.pickPos = i+1;
                PLUI.heldHead[i] = false;
                PLUI.UpdatePins();
            }
            PLUI.pickPos = 1;
            PLUI.UpdatePins();
            isKey = false;
        }
        isTryingObject = true;
        // Go into game
        Debug.Log("Picking lock...");
        cc.isInCutscene = true;
        curPlayer = cc;
        cutsceneFinished = false;
        prevPosition = cc.cam.transform.position;
        prevRotation = cc.cam.transform.rotation;
        float LPBP = pinPositions[0];
        bindingPin = 0;
        for(int i = 0; i < numPins; i++)
        {
            pinHeights[i] = origPinHeights[i];
            tumblerPosition = 0;
            if(LPBP > pinPositions[i])
            {
                LPBP = pinPositions[i];
                bindingPin = i;
            }
        }
        StartCoroutine("PlayZoomInForward");
        Debug.Log("Information about minigame:\n Hold left mouse button down to apply tension. Release to allow for easier picks.\n Press and hold right mouse button to apply counter-rotation (useful for security pins).\n Press W to advance a pin. Press S to go back a pin.\nPress T to tap the pin (useful to check if binding or not)\nPress G to press the pin (pushes the pin up, remember to release tension).\nPress B for force a pin up, this will tell you if the pin can move or if it is jamming on something (it may be a security pin!) \n\nTo pick, you must exploit the fact that these locks aren't perfect, and that by forcing the interior of the lock to rotate, one pin will be \"pressed\" up against the cylinder. If you find this pin and can push it into the correct position, the cylinder will rotate, allowing the pin to be held up by the cylinder itself (hence removing it from the list to be picked).\n Some locks has security pins, which are meant to act as \"false sets\", these have to be tested for and it will never allow the pin to be pushed up all the way.\nHowever, normal pins can be pushed too far up the chamber, causing them to bind as well.\n\nIf you believe you have a security pin that is active, or if you accidently pushed a pin up too far (or both), you can perform a counter rotation to let the pins fall back into resting position (note, you technically can lose progress...).");
        return true;
        //cc.AllowCursorFreedom();
    }
    public float turnSpeed = 0.002f;
    private void Eject()
    {
        Debug.Log("Stopping pick attempt.");
        if (!isTryingObject) { return; }
        isTryingObject = false;
        cutsceneFinished = false;
        StopAllCoroutines();
        StartCoroutine("PlayZoomInBackward");
        foreach (Transform t in keyTransform)
        {
            Destroy(t.gameObject);
        }
        foreach (Transform t in pickTransform)
        {
            Destroy(t.gameObject);
        }
        foreach (Transform t in handleTransform)
        {
            Destroy(t.gameObject);
        }
        PLEDOT.StartBackwards();
        PLUI.height = 0;
        for (int i = 0; i < 8; i++)
        {
            PLUI.pickPos = i + 1;
            PLUI.heldHead[i] = false;
            PLUI.UpdatePins();
        }
    }
    private void PickFin()
    {
        gameLock.GFinished(curPlayer);
    }
    private void Update()
    {
        if (curPlayer != null && cutsceneFinished)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Stopping pick attempt.");
                cutsceneFinished = false;
                isTryingObject = false;
                StartCoroutine("PlayZoomInBackward");
                interact.Finished();
                if (isPicked)
                {
                    interact.SendSF();
                }
                foreach (Transform t in keyTransform)
                {
                    Destroy(t.gameObject);
                }
                foreach (Transform t in pickTransform)
                {
                    Destroy(t.gameObject);
                }
                foreach (Transform t in handleTransform)
                {
                    Destroy(t.gameObject);
                }
                PLEDOT.StartBackwards();
                PLUI.height = 0;
                for (int i = 0; i < 8; i++)
                {
                    PLUI.pickPos = i + 1;
                    PLUI.heldHead[i] = false;
                    PLUI.UpdatePins();
                }
            }
            
            if (Input.GetMouseButton(0))
            {
                // Turn to binding pin...
                //PinHolderPos.transform.localRotation.eulerAngles;
                tumblerPosition += turnSpeed;
                Vector3 vr = new Vector3(0, tumblerPosition * 10, 0);
                if (!isKey)
                {
                    if (bindingPin != -1)
                    {
                        if (tumblerPosition >= pinPositions[bindingPin] && !(Mathf.Abs(pinHeights[bindingPin]) < pinTolerance[bindingPin]))
                        {
                            tumblerPosition = pinPositions[bindingPin];
                            if (Input.GetMouseButtonDown(0)) { Debug.Log("A pin is binding!"); }
                        }
                        else if ((tumblerPosition >= pinPositions[bindingPin]) && bindingPin == testingPinID && Mathf.Abs(pinHeights[bindingPin]) < pinTolerance[bindingPin])
                        {
                            Debug.Log("Click!");
                            isLatched[testingPinID] = true;
                            PLUI.heldHead[testingPinID] = true;
                            tumblerPosition += turnSpeed;

                            vr = new Vector3(0, tumblerPosition * 10, 0);
                            //PinHolderPos.transform.localRotation = Quaternion.Euler(vr);
                            float ffv = pinHeights[testingPinID];
                            findNextBinding();
                            pinHeights[testingPinID] = ffv;
                        }
                    }
                }
                if (tumblerPosition > 5)
                {
                    Debug.Log("Minigame finished!");
                    interact.SendSF();
                    cutsceneFinished = false;
                    isTryingObject = false;
                    isPicked = true;
                    StartCoroutine("PlayZoomInBackward");
                    foreach (Transform t in keyTransform)
                    {
                        Destroy(t.gameObject);
                    }
                    foreach (Transform t in pickTransform)
                    {
                        Destroy(t.gameObject);
                    }
                    foreach (Transform t in handleTransform)
                    {
                        Destroy(t.gameObject);
                    }
                    PLEDOT.StartBackwards();
                    PLUI.height = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        PLUI.pickPos = i + 1;
                        PLUI.heldHead[i] = false;
                        PLUI.UpdatePins();
                    }
                }
                
                PinHolderPos.transform.localRotation = Quaternion.Euler(vr);
            }
            if (isPicked == false && !isKey)
            {
                if (Input.GetKeyDown(KeyCode.W))
                {
                    if (testingPinID == numPins - 1)
                    {
                        Debug.Log("I hit the back of the lock.");
                        return;
                    }
                    pinHeights[testingPinID] = origPinHeights[testingPinID];
                    testingPinID++;
                    PLUI.pickPos = testingPinID + 1;
                    PLUI.height = 0;
                    PLUI.UpdatePins();
                    Debug.Log("Moved to pin " + (testingPinID + 1));
                    PickTrans.localPosition += new Vector3(0, -0.015f, 0);
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    if (testingPinID == 0)
                    {
                        Debug.Log("I'm at the beginning of the lock.");
                        return;
                    }
                    pinHeights[testingPinID] = origPinHeights[testingPinID];
                    testingPinID--;
                    PLUI.pickPos = testingPinID + 1;
                    PLUI.height = 0;
                    PLUI.UpdatePins();
                    Debug.Log("Moved to pin " + (testingPinID + 1));
                    PickTrans.localPosition += new Vector3(0, 0.015f, 0);
                }
                if (Input.GetKeyDown(KeyCode.T))
                {
                    if (!Input.GetMouseButton(0))
                    {
                        Debug.Log("I can check a pin, but it won't do any good unless I apply some force on the pins.");
                    }
                    if (testingPinID == bindingPin && Mathf.Abs(tumblerPosition - pinPositions[bindingPin]) < 0.0001f && Input.GetMouseButton(0))
                    {
                        Debug.Log("This pin is binding!");
                        //TODO
                    }
                    else
                    {
                        Debug.Log("This pin is free.");
                        // TODO
                    }
                }
                if (Input.GetKey(KeyCode.G))
                {
                    if (Input.GetMouseButton(0) && testingPinID == bindingPin && Mathf.Abs(tumblerPosition - pinPositions[bindingPin]) < 0.0001f)
                    {
                        pinHeights[testingPinID] -= 0.001f;
                        PLUI.height = (origPinHeights[testingPinID] - pinHeights[testingPinID]) * 100.0f / 0.95f;
                        PLUI.UpdatePins();
                    }
                    else if (testingPinID == bindingPin && Mathf.Abs(tumblerPosition - pinPositions[bindingPin]) < 0.0001f)
                    {
                        pinHeights[testingPinID] -= 0.015f;
                        PLUI.height = (origPinHeights[testingPinID] - pinHeights[testingPinID]) * 100.0f / 0.95f;
                        PLUI.UpdatePins();
                    }
                    else
                    {
                        pinHeights[testingPinID] -= 0.0075f;
                        if (pinHeights[testingPinID] < -pinTolerance[testingPinID])
                        {
                            Debug.Log("This pin won't go up any further!");
                            pinHeights[testingPinID] = -pinTolerance[testingPinID];
                        }
                        PLUI.height = (origPinHeights[testingPinID] - pinHeights[testingPinID]) * 100.0f / 0.95f;
                        PLUI.UpdatePins();
                    }
                }
            }
        }
    }

    public void findNextBinding()
    {
        float LPBP = float.MaxValue;
        bindingPin = -1;
        for (int i = 0; i < numPins; i++)
        {
            pinHeights[i] = origPinHeights[i];
            if (LPBP > pinPositions[i] - tumblerPosition && !isLatched[i])
            {
                LPBP = pinPositions[i] - tumblerPosition;
                bindingPin = i;
            }
        }
        if(bindingPin == -1)
        {
            Debug.Log("The lock was picked!");
            isPicked = true;
            
            turnSpeed *= 10;
        }
    }

}
