using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
#pragma warning disable CS0618 // Type or member is obsolete

public class BuiltinMsgTypes
{
    public static short RemoteInformation = MsgType.Highest + 1;
}

public class RemoteConnectionMsg : MessageBase
{
    public long UUID;
    public bool disconnected = false;
}
public class MDNetworker : NetworkManager
{
    public Dictionary<long, int> playerIDs = new Dictionary<long, int>(); // Given id, actual id
    private Dictionary<int, long> IDplayers = new Dictionary<int, long>(); // actual id, given id
    private List<long> curPlayerIDs = new List<long>();
    private HashSet<long> ignorePID = new HashSet<long>();
    private static long userUUID = 0; // OH this is a "current UUID"
    public Dictionary<long, NetworkConnection> connections = new Dictionary<long, NetworkConnection>();
    public Dictionary<NetworkConnection, long> Rconnections = new Dictionary<NetworkConnection, long>();

    public NetworkConnection clientConn;
    public delegate void ClientConnected(long userID);
    public bool isHost { get; private set; }
    public bool isConn { get; private set; }
    public bool isFin  { get; private set; }
    public bool IsHost()
    {
        return isHost;
    }
    public bool IsConnected()
    {
        return isConn;
    }
    public bool IsFinished()
    {
        return isFin;
    }
    public void RemoteConnected(NetworkMessage nm)
    {
        RemoteConnectionMsg RCM = nm.ReadMessage<RemoteConnectionMsg>();
        if (RCM.disconnected)
        {
            curPlayerIDs.Remove(RCM.UUID);
            ignorePID.Add(RCM.UUID);
        }
        else if(!ignorePID.Contains(RCM.UUID))
        {
            curPlayerIDs.Add(RCM.UUID);
        }
        Debug.Log("Remote connected! " + RCM.UUID);
    }
    // On the host starting
    public override void OnStartHost()
    {
        base.OnStartHost();
        isHost = true;
        isConn = true;
        Debug.Log("Started hosting...");
    }
    public override void OnStartClient(NetworkClient client)
    {
        base.OnStartClient(client);
        client.RegisterHandler(BuiltinMsgTypes.RemoteInformation, RemoteConnected);
        isHost = false;
        Debug.Log("Started client...");
    }
    // Called on server when client connects
    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        Debug.Log("Client connected to server...");
        isHost = true;
    }
    // Called on client when connected to server
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log("Server is present for client!...");
        clientConn = conn;
        isHost = false;
        isConn = true;
    }
    // Called on server when client/server handshaking done
    public override void OnServerReady(NetworkConnection conn)
    {
        Debug.Log("Server is done with handshakes client!...");
        base.OnServerReady(conn);
        long uID = userUUID++;

        curPlayerIDs.Add(uID);
        playerIDs.Add(uID, conn.connectionId);
        IDplayers.Add(conn.connectionId, uID);
        connections.Add(uID, conn);
        Rconnections.Add(conn, uID);
        
        // Now we flood them
        foreach(long PID in curPlayerIDs)
        {
            if(PID == uID)
            {
                continue;
            }
            connections[PID].Send(BuiltinMsgTypes.RemoteInformation, new RemoteConnectionMsg() { UUID = uID, disconnected = false });
            connections[uID].Send(BuiltinMsgTypes.RemoteInformation, new RemoteConnectionMsg() { UUID = PID, disconnected = false });
        }
    }
    // Called when hosting stopped
    public override void OnStopHost()
    {
        base.OnStopHost();
        curPlayerIDs.Clear();
        playerIDs.Clear();
        IDplayers.Clear();
        Rconnections.Clear();
        connections.Clear();
        userUUID = 0;
        isFin = true;
    }
    // Called when client stopped
    public override void OnStopClient()
    {
        base.OnStopClient();
        curPlayerIDs.Clear();
        playerIDs.Clear();
        IDplayers.Clear();
        Rconnections.Clear();
        connections.Clear();
        userUUID = 0;
        isFin = true;
    }
    // Called on server when client DCs
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        long uID = Rconnections[conn];
        

        // Now we flood them
        foreach (long PID in curPlayerIDs)
        {
            if (PID == uID)
            {
                continue;
            }
            connections[PID].Send(BuiltinMsgTypes.RemoteInformation, new RemoteConnectionMsg() { UUID = uID, disconnected = false });
            //connections[uID].Send(BuiltinMsgTypes.RemoteInformation, new RemoteConnectionMsg() { UUID = PID, disconnected = false });
        }
        curPlayerIDs.Remove(uID);
        playerIDs.Remove(uID);
        IDplayers.Remove(conn.connectionId);

        Rconnections.Remove(connections[uID]);
        connections.Remove(uID);
        base.OnServerDisconnect(conn);
    }
    // Disconnected from server (On clients)
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        curPlayerIDs.Clear();
        playerIDs.Clear();
        IDplayers.Clear();
        Rconnections.Clear();
        connections.Clear();
        userUUID = 0;
        isFin = true;

    }

    
}
#pragma warning restore CS0618 // Type or member is obsolete