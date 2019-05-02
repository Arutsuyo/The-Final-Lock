using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    public static bool ShouldReportDestroyed = false;
    // Start is called before the first frame update
    public ObjWatcher[] spawnables;
    void Start()
    {
        Networker.Init();
        // Register all spawnables in the network
        Networker.RegisterPrefabs(this);
    }
    public void RESET()
    {
        // :D Time to commit ritualistic suicide!
        foreach(long l in managed.Keys)
        {
            if(managed[l] == null || managed[l].Equals(null)) { }
            else
            {
                Destroy(managed[l].gameObject);
            }
        }
        managed.Clear();
        destroyed.Clear();

    }
    public Dictionary<long, ObjWatcher> managed = new Dictionary<long, ObjWatcher>();
    public HashSet<long> destroyed = new HashSet<long>();

    public GameObject CreateObject(ObjWatcher w, Vector3 position, Quaternion q, long UUID, long ownerID)
    {
        ObjWatcher go = Instantiate(w.gameObject, position, q).GetComponent<ObjWatcher>();
        go.objUUID = UUID;
        go.prefabID = w.prefabID;
        go.PFM = this;
        managed.Add(UUID, go);
        go.ownerID = ownerID;
        if (Networker.isHost || ownerID == Networker.getUserUUID())
        {
            if (ownerID == -1)
            {
                go.NetworkAuthority = true;
                go.FullAuthority = false;
            }
            else
            {
                go.NetworkAuthority = !Networker.isHost;
                go.FullAuthority = false;
            }

        }
        else
        {
            go.NetworkAuthority = false;
            go.FullAuthority = true;
        }
        return go.gameObject;
    }
    public ObjWatcher GetNetworkedObj(long uuid)
    {
        return managed[uuid];
    }
    public void OnClientConnect(long uuid)
    {
        // Send all managed game objects to the client to spawn
        Debug.Log("Client connected!");
        foreach(long w in managed.Keys)
        {
            ObjWatcher ww = managed[w];
            ObjCreNETMSG cm = new ObjCreNETMSG()
            {
                prefab = ww.prefabID,
                objUUID = ww.objUUID,
                px = ww.transform.position.x,
                py = ww.transform.position.y,
                pz = ww.transform.position.z,
                qx = ww.transform.rotation.x,
                qy = ww.transform.rotation.y,
                qz = ww.transform.rotation.z,
                qw = ww.transform.rotation.w,
                useQ = ww.sendRotation,
                useP = ww.sendMovement,
                ownerID = ww.ownerID
            };
            Networker.SendClient(cm, uuid, 0);
        }
    }
    public void SendUpdate(ObjWatcher obj)
    {
        // Send an update!
        if (Networker.isHost)
        {
            ObjNETMSG obj1 = new ObjNETMSG()
            {
                objUUID = obj.objUUID,
                px = obj.transform.position.x,
                py = obj.transform.position.y,
                pz = obj.transform.position.z,
                qx = obj.transform.rotation.x,
                qy = obj.transform.rotation.y,
                qz = obj.transform.rotation.z,
                qw = obj.transform.rotation.w,
                useQ = obj.sendRotation,
                useP = obj.sendMovement,
                ownerID = obj.ownerID,
                isError = false
            };
            // BE CAREFUL!
            //TODO HERE
            Networker.SendAllClients(obj1, obj.qosStream);
        }
        else
        {
            ObjRelNETMSG obj1 = new ObjRelNETMSG()
            {
                objUUID = obj.objUUID,
                px = obj.transform.position.x,
                py = obj.transform.position.y,
                pz = obj.transform.position.z,
                qx = obj.transform.rotation.x,
                qy = obj.transform.rotation.y,
                qz = obj.transform.rotation.z,
                qw = obj.transform.rotation.w,
                useQ = obj.sendRotation,
                useP = obj.sendMovement,
                ownerID = obj.ownerID,
                isError = false
            };
            // BE CAREFUL!
            //TODO HERE
            Networker.SendServer(obj1, obj.qosStream);
        }
    }
    public void ManageGameObjects(ObjNETMSG msg)
    {
        if (msg is ObjCreNETMSG)
        {
            Networker.RequestNetworkSpawnObject(msg, ((ObjCreNETMSG)msg).prefab, (msg.useP ? new Vector3(msg.px, msg.py, msg.pz) : Vector3.zero), (msg.useQ ? new Quaternion(msg.qx, msg.qy, msg.qz, msg.qw): Quaternion.identity));
        }
        else
        {
            if (destroyed.Contains(msg.objUUID))
            {
                // Object was destroyed.
                if (ShouldReportDestroyed)
                {
                    Debug.LogWarning("Object " + msg.objUUID + " was destroyed. Packed was recieved denoting for position data.");
                }
                // Discard all things about this.
                return;
            }
            if (!managed.ContainsKey(msg.objUUID))
            {
                Debug.LogError("CLIENT/HOST MISMATCH! UUID NOT FOUND! ");
            }
            else
            {
                //Debug.Log("What you doing?" + Networker.isHost + " " + (msg is ObjRelNETMSG));
                // Oh god got to move!
                if(Networker.isHost && msg is ObjRelNETMSG)
                {
                    //Debug.LogWarning(msg.ownerID);
                    // This means to redistribute the message as a simple ObjNetMSG
                    Networker.SendAllClientsExcept(msg, msg.ownerID, 1);
                }
                long UD = msg.objUUID;
                managed[UD].SetNewTargets((msg.useP ? new Vector3(msg.px, msg.py, msg.pz) : Vector3.zero), (msg.useQ ? new Quaternion(msg.qx, msg.qy, msg.qz, msg.qw) : Quaternion.identity), msg.useP, msg.useQ, msg.isError);
                //Debug.Log("Got an update!");
            }
        }
    }
    public void RequestDestroy(long objUUID)
    {
        // Now this will 
        if (!managed.ContainsKey(objUUID))
        {
            // We o-0
            Debug.LogError("An object was requested to be destroyed however said object does not exist! " + objUUID);
            if (destroyed.Contains(objUUID))
            {
                Debug.LogError("--The object was found in the destroyed pile! Ensure only destroying once!");
            }
            return;
        }
        Destroy(managed[objUUID]);
        destroyed.Add(objUUID);
        managed.Remove(objUUID);
    }
    public void SendErrorUpdates(ObjWatcher obj, Vector3 v, Quaternion q)
    {
        ObjNETMSG obj1 = new ObjNETMSG()
        {
            objUUID = obj.objUUID,
            px = v.x,
            py = v.y,
            pz = v.z,
            qx = q.x,
            qy = q.y,
            qz = q.z,
            qw = q.w,
            useQ = obj.sendRotation,
            useP = obj.sendMovement,
            ownerID = obj.ownerID,
            isError = true
        };
        // Rather "redistribute"
        Networker.SendAllClients(obj1, obj.qosStream);
    }
}
