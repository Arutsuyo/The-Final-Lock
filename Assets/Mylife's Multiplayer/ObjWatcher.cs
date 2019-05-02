using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public enum InterpolationType { NONE, LINEAR, DECAYING };
public class ObjWatcher : MonoBehaviour
{
    [Range(0.01f,100f)]public float threshold = 0.01f;
    [Range(0.001f, 2*Mathf.PI)] public float rotThreshold = 0.01f;
    [Range(0, 10f)] public float sendFrequency = 0.1f;
    [Range(0.0001f, 10f)] public float refreshDelay = 0.1f;
    public bool sendMovement = true;
    public bool sendRotation = true;
    public int qosStream = 1;
    public Vector3 targetPos;
    public Quaternion targetQuat;
    private Vector3 prevPosition;
    private Quaternion prevQuat;

    private Vector3 errorPosition;
    private Quaternion errorQuatern;
    private float prevT = 0; // Previous message time
    private float targT = 1; // New message time
    public InterpolationType interpType = InterpolationType.NONE;
    [HideInInspector] public long objUUID;
    [HideInInspector] public int prefabID;
    [HideInInspector] public PrefabManager PFM;

    public float decaytime = 4;
    public long ownerID;
    public bool NetworkAuthority = false;
    private int StopUpdates = 2;
    private bool updated = false;
    private float pdt = 1.0f;
    public bool FullAuthority = false; // If this is false (ON THE SERVER SIDE), then the client will be pulled back if something occurs on the server, however if true, then the client decides exactly what happens.
    public float previousRefresh = 0;
    public void Start()
    {
        prevPosition = transform.position;
        prevQuat = transform.rotation;
        targetQuat = prevQuat;
        targetPos = prevPosition;
        prevT = Time.time - 1;
        targT = Time.time;
        previousRefresh = Time.time;
        errorPosition = Vector3.zero;
        errorQuatern = Quaternion.identity;
        // Register self with Networker.
        // To register oneself pre-existing as a networked object, create a "blank", spawn it and then make it a child.
    }
    public void Update()
    {
        if (NetworkAuthority)
        {
            if ((Mathf.Abs(Quaternion.Angle(Quaternion.Inverse(errorQuatern)*transform.rotation, prevQuat)) > rotThreshold || (prevPosition - transform.position + errorPosition).magnitude > threshold) && (Time.time - targT) >= sendFrequency)
            {
                prevPosition = transform.position;
                prevQuat = transform.rotation;
                // Now send a message
                StopUpdates = 2;
                updated = true;
                PFM.SendUpdate(this);
                targT = Time.time;
            }else if (Time.time - targT < sendFrequency) { 
                // Do nothing here...
            }else if(StopUpdates >= 1 && updated)
            {
                PFM.SendUpdate(this);
                StopUpdates--;
                targT = Time.time;

            }
            else if(updated)
            {
                updated = false;
            }
        }
        else
        {
            // The idea is that if a client object has authority, try to update it here, and then relay it...
            // Simply listen for updates...
            // However, all physics will be retroactively applied to the client... (so semi-changed)
            if (FullAuthority)
            {
                float dt = 1.0f;
                if (interpType == InterpolationType.LINEAR)
                {
                    dt = 1.0f + ((Time.time - targT) / (targT - prevT));
                }
                else if (interpType == InterpolationType.DECAYING)
                {
                    dt = 1.0f - decaytime / 2 + (decaytime / (1.0f + Mathf.Exp(-(Time.time - targT) / (targT - prevT))));
                }

                if (sendMovement)
                {
                    transform.position = Vector3.LerpUnclamped(prevPosition, targetPos, dt);
                }
                if (sendRotation)
                {
                    transform.rotation = Quaternion.SlerpUnclamped(prevQuat, targetQuat, dt);
                }
            }
            else
            {
                /*
                 * Possible ways to handle client/server contradiction. If the server reports
                 * for the tank to move, the server should send the delta, which the client will update
                 * their actual position, but they will also send this delta as well for the server to compare
                 * against. once the deltas match, the server should send a "zero it" command to reset the deltas.
                 */
                if (!Networker.isHost)
                {
                    // BAD!
                    Debug.LogError("Object is assigned to not have network authority but has some authority! Fixing...");
                    FullAuthority = true;
                    return;
                }
                
                float dt = 1.0f;
                bool alerting = false;
                if (interpType == InterpolationType.LINEAR)
                {
                    dt = 1.0f + ((Time.time - targT) / (targT - prevT));
                }
                else if (interpType == InterpolationType.DECAYING)
                {
                    dt = 1.0f - decaytime / 2 + (decaytime / (1.0f + Mathf.Exp(-(Time.time - targT) / (targT - prevT))));
                }
                /*if((Vector3.LerpUnclamped(prevPosition, targetPos, pdt) - transform.position + errorPosition).magnitude >= threshold ||
                    Mathf.Abs(Quaternion.Angle(Quaternion.SlerpUnclamped(prevQuat, targetQuat, pdt), Quaternion.Inverse(errorQuatern) * transform.rotation)) >= rotThreshold)
                {
                    prevPosition = transform.position;
                }*/
                if (Mathf.Abs(Quaternion.Angle(Quaternion.Inverse(errorQuatern)*transform.rotation, Quaternion.SlerpUnclamped(prevQuat, targetQuat, pdt))) > rotThreshold)
                {
                    errorQuatern = Quaternion.Inverse(Quaternion.SlerpUnclamped(prevQuat, targetQuat, pdt)) * transform.rotation;
                    alerting = true;
                }
                // Compare actual movement vs expected. Have 10x threshold for this.
                if((transform.position - Vector3.LerpUnclamped(prevPosition, targetPos, pdt)).magnitude >= 2 * threshold){
                    errorPosition = transform.position - Vector3.LerpUnclamped(prevPosition, targetPos, pdt); // o-o
                    alerting = true;
                }

                if (alerting)
                {
                    // Send the correction
                    PFM.SendErrorUpdates(this, errorPosition, errorQuatern);
                }
                if (sendMovement)
                {
                    transform.position = Vector3.LerpUnclamped(prevPosition, targetPos, dt) + errorPosition;
                }
                if (sendRotation)
                {
                    transform.rotation = Quaternion.SlerpUnclamped(prevQuat, targetQuat, dt) * errorQuatern;
                }
                pdt = dt;
                if(Time.time - previousRefresh >= refreshDelay && Networker.isHost && (Mathf.Abs(Quaternion.Angle(Quaternion.identity, errorQuatern)) >= 0.001f || errorPosition.magnitude >= 0.001f))
                {
                    PFM.SendErrorUpdates(this, Vector3.zero, Quaternion.identity);
                    SetNewTargets(transform.position, transform.rotation, true, true, false);
                    SetNewTargets(Vector3.zero, Quaternion.identity, true, true, true);
                    PFM.SendUpdate(this);
                    previousRefresh = Time.time;
                }
                else if(Time.time - previousRefresh >= refreshDelay)
                {
                    previousRefresh = Time.time;
                }
            }
        }
    }
    public void SetNewTargets(Vector3 pos, Quaternion q, bool useP, bool useQ, bool isError)
    {
        //Debug.Log(isError);
        if (isError)
        {
            errorPosition = pos;
            errorQuatern = q;
            return;
        }
        if (useP)
        {
            prevPosition = targetPos;
            targetPos = pos;
            transform.position = pos;
        }
        if (useQ)
        {
            prevQuat = targetQuat;
            targetQuat = q;
            transform.rotation = q;
        }
        
        if(Time.time - targT >= 0.00001f) {
            prevT = targT;
            targT = Time.time;
        }
        pdt = 1.0f;


    }
}

[System.Serializable]
public class ObjNETMSG : NETMSG
{
    public ObjNETMSG()
    {
        OP = NetID.ObjMsg;
        dataSize = 0;
    }
    public long objUUID;
    public float px, py, pz;
    public float qx, qy, qz, qw;
    public bool useQ;
    public bool useP;
    public bool isError = false;
    public long ownerID; // -1 is server, 0-> is any other.
}
[System.Serializable]
public class ObjCreNETMSG : ObjNETMSG
{
    public int prefab;
}

[System.Serializable]
public class ObjRelNETMSG : ObjNETMSG
{
    // Literally no different, but contains slightly more info...
}

