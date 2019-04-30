using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
/*
 * Created by Chase Craig (Mylifefighter/Mylife)
 * Handles the Networking for the player using LLAPI.
 */

#pragma warning disable CS0618 // Type or member is obsolete
public class Networker
{
    private static List<QosType> channels = new List<QosType>();
    private static byte[] allocChannels;
    private static int max_users;
    public static int port = 26000;
    public static int web_port = 26001;
    public const int BYTE_SIZE = 512; // Any larger and fragments are needed T_T
    private static int hostID;
    private static int webID;
    private static byte error;
    private static bool isStarted = false;
    private static bool hasConnected = false;
    private static ConnectionConfig config;
    private static HostTopology topo;
    public static int maxAllowedPlayers { get; private set; }
    public static bool isHost { get; private set; }
    private static int connectionID;
    public static bool debugging = true;
    private static Dictionary<long, int> playerHID = new Dictionary<long, int>(); // given Id, actual id
    private static Dictionary<long, int> playerIDs = new Dictionary<long, int>(); // Given id, actual id
    private static Dictionary<int, long> IDplayers = new Dictionary<int, long>(); // actual id, given id
    private static List<long> curPlayerIDs = new List<long>();
    private static long userUUID = 0; // OH this is a "current UUID"
    public static string ConnIP = "127.0.0.1";
    private static long objectUUID = 0;
    private static bool hasRegistered = false;
    private static PrefabManager PFM;
    private static long curUserUUID = -2;
    public static long getUserUUID()
    {
        return isHost? -1 : curUserUUID;
    }
    public static GameObject NetworkSpawnObject(int i, Vector3 position, Quaternion rot, long ownerID) // Spawns the prefab at the prefab index
    {
        if (!Networker.isHost)
        {
            Debug.LogError("Clients are unable to create objects! All networked objects must be created on the server!");
            return null;
        }
        if(prefabs.Count <= i || i < 0)
        {
            Debug.LogError("Spawn prefab instance out of bounds!");
            return null;
        }
        // Send a message to the other clients to create this object.
        long curUUID = objectUUID++;
        ObjCreNETMSG cm = new ObjCreNETMSG() {ownerID = ownerID, prefab = i, objUUID = curUUID, px = position.x, py = position.y, pz = position.z
            ,qx = rot.x, qy = rot.y, qz = rot.z, qw = rot.w, useQ=prefabs[i].sendRotation, useP = prefabs[i].sendMovement};
        Networker.SendAllClients(cm);
        return PFM.CreateObject(prefabs[i], position, rot, curUUID, ownerID);
    }
    public static GameObject RequestNetworkSpawnObject(ObjNETMSG msg, int i, Vector3 position, Quaternion rot)
    {
        if (Networker.isHost || msg == null)
        {
            Debug.LogError("The host should not request to spawn an object! A client should issue a message command to the server and the SERVER should create one itself.");
            return null;
        }
        if(prefabs.Count <= i || i < 0)
        {
            Debug.LogError("Spawn prefab instance out of bounds!");
            return null;
        }
        return PFM.CreateObject(prefabs[i], position, rot, msg.objUUID, msg.ownerID);
    }
    public static void Init()
    {
        // Handle initalization!
        Networker.Start();
    }

    public delegate void MsgHook(MsgObj obj);
    public static event MsgHook OnMsgEvent;
    public static void RESET()
    {
        // WARNING: DO NOT CALL UNLESS YOU DON'T INTEND ON USING ANYTHING ANYMORE :D
        channels.Clear();
        allocChannels = null;
        max_users = 0;
        port = 26000;
        web_port = 26001;
        hostID = 0;
        webID = 0;
        error = 0;
        isStarted = false;
        hasConnected = false;
        config = null;
        topo = null;
        maxAllowedPlayers = 0;
        isHost = false;
        connectionID = 0;
        ConnIP = "127.0.0.1";
        userUUID = 0;
        hasRegistered = false;
        PFM.RESET();
        PFM = null;
        curUserUUID = -2;
        curPlayerIDs.Clear();
        IDplayers.Clear();
        playerHID.Clear();
        playerIDs.Clear();
    }
    public delegate void ArbHook(int op,int subop, ArbObj obj);
    public static event ArbHook OnUserEvent;
    public static event ArbHook OnOtherEvent;
    private static long FetchUUID(int sid)
    {
        return isHost ? IDplayers[sid] : -1;
    }
    private static void HandleDataEvent(int rec, int sID, int cID, byte[] msg, int size)
    {
        BinaryFormatter form = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(msg);
        
        NETMSG msg1 = form.Deserialize(ms) as NETMSG;
        //Debug.Log(Time.time + " " + msg1.GetType().Name.ToString());
        ms.Close();
        switch (msg1.OP)
        {
            case NetID.None:
                break;
            case NetID.Buffer:
                break;
            case NetID.ConnID:
                if(msg1 is ConnNETMSG)
                {
                    Debug.Log("Client connection UUID: " + ((ConnNETMSG)msg1).uuid);
                    curUserUUID = ((ConnNETMSG)msg1).uuid;
                    // NOW set the user as connected.
                    hasConnected = true;

                    return;
                }
                break;
            case NetID.ObjMsg:
                //Handle
                PFM.ManageGameObjects((ObjNETMSG)msg1);
                return;
            case NetID.Msg:
                //Handle
                OnMsgEvent(new MsgObj() { msg = ((MessageNETMSG)msg1).msg, uuid = FetchUUID(sID) });
                return;
            case NetID.User:
                // User defined.
                // All user events should inherit from the UserNETMSG msg
                if (msg1 is UserNETMSG)
                {
                    //Debug.Log("o-o " + sID + " " + IDplayers.ContainsKey(sID));
                    OnUserEvent(msg1.OP,((UserNETMSG)msg1).subID, new ArbObj() { msg = msg1, id = msg1.OP, uuid = FetchUUID(sID) });
                }
                else
                {
                    Debug.LogError("Unregistered packet is not a subclass of UserNETMSG! All user/user defined IDs must be of this type!");
                }
                return;
            default:
                // All user events should inherit from the UserNetMSG msg
                if (msg1 is UserNETMSG)
                {
                    OnOtherEvent(msg1.OP, ((UserNETMSG)msg1).subID, new ArbObj() { msg = msg1, id = msg1.OP, uuid = FetchUUID(sID) });
                }
                else
                {
                    Debug.LogError("Unregistered packet is not a subclass of UserNETMSG! All user/user defined IDs must be of this type!");
                }
                return;
        }
        Debug.LogError("UNHANDLED MESSAGE: " + msg1.OP);
    }
    private static List<ObjWatcher> prefabs = new List<ObjWatcher>();
    public static void RegisterPrefabs(PrefabManager r)
    {
        if (hasRegistered)
        {
            Debug.LogError("Prefabs have already been registered! Try to prevent registering them twice!");
            return;
        }
        foreach(ObjWatcher pre in r.spawnables){
            pre.prefabID = prefabs.Count;
            prefabs.Add(pre);
        }

        hasRegistered = true;
        PFM = r;

    }
    public static List<long> getCurrentPlayers()
    {
        return new List<long>((long[])curPlayerIDs.ToArray().Clone());
    }
    public static int getNumPlayers()
    {
        return curPlayerIDs.Count;
    }
    public static void setMaxAllowedUsers(int users)
    {
        // Note this function is different from below in that this simply limits connections. Not topology
        maxAllowedPlayers = users;
    
    }
    public static void setMaxUsers(int users)
    {
        if (isStarted)
        {
            Debug.LogError("Attempting to change number of users while running!");
        }
        maxAllowedPlayers = users;
        max_users = users;
    }
    public static List<QosType> GetChannels()
    {
        return new List<QosType>((QosType[])channels.ToArray().Clone());
    }
    public static int AddChannel(QosType channelType)
    {
        if (isStarted)
        {
            Debug.LogError("Attempted to add a channel when running!");
            return -1;
        }
        channels.Add(channelType);
        return channels.Count;
    }
    public static void Shutdown()
    {
        bool washost = isHost;
        isStarted = false;
        isHost = false;
        hasConnected = false;
        NetworkTransport.Shutdown();
        OnServerDisconnected(washost, new ConnectionObj() { connID = -1 });
        
    }
    public static void Update() // Needs to be called by other function
    {
        UpdateMessagePump();
    }
    public static void Start()
    {
        OnClientConnect += Debugger2;
        OnClientDisconnect += Debugger2;
        OnErrorEvent += Debugger3;
        OnServerDisconnected += Debugger2;
    }
    private static void Debugger3(ErrorObj o)
    {
        if (debugging)
        {
            Debug.Log("DEBUGGING AT " + Time.time +": " + o.ToString() + "\n\t-" + o.type.ToString() + " " + o.error + " " + o.conUUID );
        }
    }
    private static void Debugger2(bool c, ConnectionObj o)
    {
        if (debugging)
        {
            Debug.Log("DEBUGGING AT " + Time.time + ": " + o.ToString() + " \n\t- " + c.ToString());
        }
    }
    public static void StartHost()
    {
        if (isStarted)
        {
            Debug.LogError("Attempted to start when already running!");
            return;
        }
        NetworkTransport.Init();
        config= new ConnectionConfig();
        allocChannels = new byte[channels.Count];
        for(int i =0; i< channels.Count; i++)
        {
            allocChannels[i] = config.AddChannel(channels[i]);
        }
        topo = new HostTopology(config, max_users);
        
        hostID = NetworkTransport.AddHost(topo, port, null);
        webID = NetworkTransport.AddWebsocketHost(topo, web_port, null);
        isHost = true;
        isStarted = true;
        hasRegistered = true;
    }
    public static void StartClient()
    {
        if (isStarted)
        {
            Debug.LogError("Attempted to start when already running!");
            return;
        }
        NetworkTransport.Init();
        config = new ConnectionConfig();
        allocChannels = new byte[channels.Count];
        for (int i = 0; i < channels.Count; i++)
        {
            allocChannels[i] = config.AddChannel(channels[i]);
        }
        topo = new HostTopology(config, max_users);
        hostID = NetworkTransport.AddHost(topo, 0);
#if UNITY_WEBGL && !UNITY_EDITOR
        connectionID = NetworkTransport.Connect(hostID, ConnIP, web_port, 0, out error);
#else
        connectionID = NetworkTransport.Connect(hostID, ConnIP, port, 0, out error);
#endif
        if (error != 0)
        {
            Debug.LogError("Unable to connect, error: " + error);
        }
        else
        {
            isStarted = true;
            hasRegistered = true;
            isHost = false;
        }
    }
    public static bool isConnected()
    {
        return hasConnected;
    }
    public static bool isActive()
    {
        return isStarted;
    }
    private static void UpdateMessagePump()
    {
        if (!isStarted)
        {
            return;
        }
        int recHostId;
        int connectionId;
        int channelId;

        byte[] recBuffer = new byte[BYTE_SIZE];
        int dataSize;
        while (true)
        {
            NetworkEventType ty = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, BYTE_SIZE, out dataSize, out error);

            if (error != 0)
            {
                Debug.Log(connectionId);
                OnErrorEvent(new ErrorObj() { error = error, conUUID = FetchUUID(connectionId), type = ty });
                if (error == 6)
                {
                    Debug.Log("Disconnection error.");
                    if (!isHost)
                    {
                        isStarted = false;
                        hasConnected = false;
                        Shutdown();
                        HandleDisconnectEvent(0, connectionId, 0);
                    }
                    else
                    {
                        HandleDisconnectEvent(0, connectionId, 0);
                    }
                }
                Debug.LogError("Error on recieve: " + error);
                return;
            }

            switch (ty)
            {
                case NetworkEventType.Nothing:
                    // :D
                    return;
                case NetworkEventType.ConnectEvent:
                    HandleConnectionEvent(recHostId, connectionId, channelId);
                    break;
                case NetworkEventType.DataEvent:
                    HandleDataEvent(recHostId, connectionId, channelId, recBuffer, dataSize);
                    break;
                case NetworkEventType.DisconnectEvent:
                    // Never called.
                    HandleDisconnectEvent(recHostId, connectionId, channelId);
                    break;
                case NetworkEventType.BroadcastEvent:
                    Debug.Log("Broadcast event! Unheard of!");
                    break;
            }
        }
    }
    private static void HandleConnectionEvent(int rec, int sID, int cID)
    {
        if (!isHost)
        {
            // Lets us know that we are connected
            //hasConnected = true; 
            // NO!
            // :D :D
            // :D
            // Note we are listening for a connection ID packet.
        }
        else
        {
            // Store the user as a potential connection id...
            // Though should we accept?
            if(maxAllowedPlayers <= curPlayerIDs.Count)
            {
                // Reject player, send disconnect message
                NetworkTransport.Disconnect(rec, sID, out error);
                // Silently drop it
                return;
            }
            else
            {
                // Handle it...
                Debug.Log(userUUID);
                curPlayerIDs.Add(userUUID);
                playerHID.Add(userUUID, rec);
                playerIDs.Add(userUUID, sID);
                IDplayers.Add(sID, userUUID);
                userUUID++;
                SendClient(new ConnNETMSG() { uuid = userUUID - 1 }, userUUID-1,0);
                OnClientConnect(true, new ConnectionObj() { connID = userUUID - 1 });
                PFM.OnClientConnect(userUUID - 1);
            }
        }
    }
    public static bool SendServer(NETMSG mg, int channelID = 0)
    {
        if (!isStarted)
        {
            Debug.LogError("You need to start a server first.");
            return false;
        }
        if (!hasConnected)
        {
            Debug.LogError("Can't send to host if you haven't connected yet!");
            return false;
        }
        if (isHost)
        {
            Debug.LogError("Can't send to the host if you are the host! " + isHost.ToString());
            return false;
        }
        if (channelID < 0 || allocChannels.Length <= channelID)
        {
            Debug.LogError("Invalid channel ID!" + channelID);
            return false;
        }
        if(BYTE_SIZE + mg.dataSize > 1028 )
        {
            switch (channels[channelID])
            {
                case QosType.ReliableFragmented:
                case QosType.ReliableFragmentedSequenced:
                case QosType.UnreliableFragmented:
                case QosType.UnreliableFragmentedSequenced:
                    break;
                default:
                    Debug.LogError("Packet size is too large for this channel! Please use a fragmented channel.");
                    return false;
            }
        }
        byte[] buffer = new byte[BYTE_SIZE + mg.dataSize];
        BinaryFormatter form = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        form.Serialize(ms, mg);

        bool dj =  NetworkTransport.Send(hostID, connectionID, channelID, buffer, BYTE_SIZE + mg.dataSize, out error);
        ms.Close();
        return dj;
    }
    public static void SendAllClientsExcept(NETMSG mg, long except, int channelID = 0)
    {
        if (!isStarted)
        {
            Debug.LogError("You need to start a server first.");
            return;
        }
        if (!isHost)
        {
            Debug.LogError("Is not the host! " + isHost.ToString());
            return;
        }
        if (channelID < 0 || allocChannels.Length <= channelID)
        {
            Debug.LogError("Invalid channel ID!" + channelID);
            return;
        }
        byte[] buffer = new byte[BYTE_SIZE + mg.dataSize];
        BinaryFormatter form = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        form.Serialize(ms, mg);

        // Attempt to send to all clients
        int i = 0;

        while (i < curPlayerIDs.Count)
        {
            if(curPlayerIDs[i] == except)
            {
                i++;
                continue;
            }
            NetworkTransport.Send(playerHID[curPlayerIDs[i]], playerIDs[curPlayerIDs[i]], channelID, buffer, BYTE_SIZE + mg.dataSize, out error);
            if (error == 6 || error ==2 )
            {
                HandleDisconnectEvent(0, playerIDs[curPlayerIDs[i]],0);
                //curPlayerIDs.RemoveAt(i);
                continue;
            }
            i++;
        }
        ms.Close();
    }
    public static void SendAllClients(NETMSG mg, int channelID = 0)
    {
        if (!isStarted)
        {
            Debug.LogError("You need to start a server first.");
            return;
        }
        if (!isHost)
        {
            Debug.LogError("Is not the host! " + isHost.ToString());
            return;
        }
        if (channelID < 0 || allocChannels.Length <= channelID)
        {
            Debug.LogError("Invalid channel ID!" + channelID);
            return;
        }
        byte[] buffer = new byte[BYTE_SIZE + mg.dataSize];
        BinaryFormatter form = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        form.Serialize(ms, mg);

        // Attempt to send to all clients
        int i = 0;
        while(i < curPlayerIDs.Count)
        {
            NetworkTransport.Send(playerHID[curPlayerIDs[i]], playerIDs[curPlayerIDs[i]], channelID, buffer, BYTE_SIZE + mg.dataSize, out error);
            if(error == 6 || error ==2)
            {
                curPlayerIDs.RemoveAt(i);
                // OH SHIT!! Need to truely process client removal!
                // TODO
                continue;
            }
            i++;
        }
        ms.Close();
    }
    public static bool SendClient(NETMSG mg, long userID, int channelID = 0)
    {
        if (!isStarted)
        {
            Debug.LogError("You need to start a server first.");
            return false;
        }
        if (!isHost || !playerIDs.ContainsKey(userID))
        {
            Debug.LogError("Invalid player ID or is not the host! " + isHost.ToString() + ", " + userID + " " + mg.OP);
            return false;
        }
        if(channelID < 0 || allocChannels.Length <= channelID)
        {
            Debug.LogError("Invalid channel ID!" + channelID);
            return false;
        }
        byte[] buffer = new byte[BYTE_SIZE + mg.dataSize];
        BinaryFormatter form = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        form.Serialize(ms, mg);

        bool bj = NetworkTransport.Send(playerHID[userID], playerIDs[userID], channelID, buffer, BYTE_SIZE + mg.dataSize, out error);
        ms.Close();
        return bj;
    }
    public static void Disconnect()
    {
        if(!(isStarted && hasConnected) || isHost)
        {
            Debug.LogError("Either this was called on the server or the user isn't connected!");
        }

        NetworkTransport.Disconnect(hostID, connectionID, out error);
        hasConnected = false;
        Networker.Shutdown();
    }
    public delegate void ConnectionHook(bool connecting, ConnectionObj obj);
    public static event ConnectionHook OnClientConnect;
    public static event ConnectionHook OnClientDisconnect; // Called on server for when client disconnected from server
    public static event ConnectionHook OnServerDisconnected; // Called on client for when disconnected from server
    public delegate void ErrorHook(ErrorObj obj);
    public static event ErrorHook OnErrorEvent;
    private static void HandleDisconnectEvent(int rec, int sID, int cID)
    {
        if (!isHost)
        {
            hasConnected = false;
            isStarted = false;
            Networker.Shutdown();
            OnServerDisconnected(false, new ConnectionObj() { connID = -1});

        }
        else
        {
            long v = IDplayers[sID];
            IDplayers.Remove(sID);
            curPlayerIDs.Remove(v);
            playerHID.Remove(v);
            playerIDs.Remove(v);
            
            OnClientDisconnect(false, new ConnectionObj() { connID = userUUID - 1 });

        }
    }
    
}
public class ArbObj
{
    public byte id;
    public long uuid;
    public NETMSG msg;
}
public class ErrorObj
{
    public byte error;
    public NetworkEventType type;
    public long conUUID;
    
}
public class ConnectionObj
{
    public long connID;
}
public class MsgObj
{
    public string msg;
    public long uuid;
}
[System.Serializable]
class ConnNETMSG : NETMSG
{
    public long uuid;
    public ConnNETMSG()
    {
        OP = NetID.ConnID;
    }
}
#pragma warning restore CS0618

