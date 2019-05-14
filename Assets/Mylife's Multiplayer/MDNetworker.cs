using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
#pragma warning disable CS0618 // Type or member is obsolete

public class MDNetworker : NetworkManager
{
    public Dictionary<long, int> playerIDs = new Dictionary<long, int>(); // Given id, actual id
    private Dictionary<int, long> IDplayers = new Dictionary<int, long>(); // actual id, given id
    private List<long> curPlayerIDs = new List<long>();
    private HashSet<long> ignorePID = new HashSet<long>();
    private static long userUUID = 1; // OH this is a "current UUID"
    private static long serverUUIDs = 1;
    public bool AcceptingConnections = true;
    public Dictionary<long, NetworkConnection> connections = new Dictionary<long, NetworkConnection>();
    public Dictionary<NetworkConnection, long> Rconnections = new Dictionary<NetworkConnection, long>();
    private Dictionary<short, NetworkMessageDelegate> RNMD = new Dictionary<short, NetworkMessageDelegate>();
    public NetworkConnection clientConn;
    public delegate void ClientConnected(long userID);
    public event ClientConnected OnClientConnected;
    public event ClientConnected OnRemoteConnected;
    public event ClientConnected OnRemoteDisconnected;

    public bool isSafe { get; private set; }
    public bool isHost { get; private set; }
    public bool isConn { get; private set; }
    public bool isFin  { get; private set; }
    public bool isDual { get; private set; }

    public List<long> GetAllPlayers()
    {
        return new List<long>(curPlayerIDs);
    }
    public NetworkConnection GetConnection(long UUID)
    {
        if (!isHost) { return null; }
        if (connections.ContainsKey(UUID))
        {
            return connections[UUID];
        }
        else { return null; }
    }

    public bool SendToOtherClient(long UUID, short msgType, TaggedMessageBase msg, int channelID = 0)
    {
        if (isHost && !isDual) { Debug.LogWarning("This function is to be called from a client connection ONLY!"); return false; }
        msg.TargetUUID = UUID;
        msg.invertTarget = false;
        msg.Targetting = true;
        msg.ActualMsgType = msgType;
        Debug.Log("BO: " + isHost + " " + isDual + " " + msg.Targetting + " " + msg.TargetUUID + " " + msg.invertTarget + " " + msg.channelID + " " + msg.ActualMsgType);
        return clientConn.SendByChannel(msgType, msg, channelID);
    }

    public bool SendToAllOtherClients(short msgType, TaggedMessageBase msg, int channelID =0)
    {
        if (isHost && !isDual) { Debug.LogWarning("This function is to be called from a client connection ONLY!"); return false; }
        msg.TargetUUID = userUUID;
        msg.invertTarget = true;
        msg.Targetting = true;
        msg.ActualMsgType = msgType;
        Debug.Log("BO: " + isHost + " " + isDual + " " + msg.Targetting + " " + msg.TargetUUID + " " + msg.invertTarget + " " + msg.channelID + " " + msg.ActualMsgType);
        return clientConn.SendByChannel(msgType, msg, channelID);
    }
    public bool SendToServer(short msgType, TaggedMessageBase msg, int channelID=0)
    {
        if (isHost && !isDual) { Debug.LogWarning("This function is to be called from a client connection ONLY!"); return false; }
        msg.Targetting = false;
        Debug.Log("BO: " + isHost + " " + isDual + " " + msg.Targetting + " " + msg.TargetUUID + " " + msg.invertTarget + " " + msg.channelID + " " + msg.ActualMsgType);
        
        return clientConn.SendByChannel(msgType, msg, channelID);
    }
    public bool SendToClient(long UUID, short msgType, TaggedMessageBase msg, int channelID=0)
    {
        if (!isHost && !isDual) { Debug.LogWarning("This function is to be called from the server connection ONLY!"); return false; }
        if (connections.ContainsKey(UUID))
        {
            return connections[UUID].SendByChannel(msgType, msg, channelID);
        }
        return false;
    }
    public bool SendToAllClients(short msgType, TaggedMessageBase msg, int channelID = 0)
    {
        if (!isHost && !isDual) { Debug.LogWarning("This function is to be called from the server connection ONLY!"); return false; }
        bool isGood = true;
        foreach (long uid in connections.Keys)
        {
            if(uid < 0)
            {
                continue;
            }
            bool kk = connections[uid].SendByChannel(msgType, msg, channelID);
            isGood = isGood && kk;
        }
        return isGood;
    }
    public bool SendToAllClientsExcept(long UUID, short msgType, TaggedMessageBase msg, int channelID = 0)
    {
        if (!isHost && !isDual) { Debug.LogWarning("This function is to be called from the server connection ONLY!"); return false; }
        bool isGood = true;
        foreach (long uid in connections.Keys)
        { 
            if(uid == UUID || uid < 0)
            {
                continue;
            }
            bool kk = connections[uid].SendByChannel(msgType, msg, channelID);
            isGood = isGood && kk;
        }
        return isGood;
    }

    public bool CheckPacket(TaggedMessageBase msg)
    {
        // The idea is that if the packet is suppose to go you to, then you do so
        Debug.Log("BOI: " + isHost + " " + isDual + " " + msg.Targetting + " " + msg.TargetUUID + " " + msg.invertTarget + " " + msg.channelID + " " + msg.ActualMsgType);
        if (!isHost)
        {
            return true;
        }
        if (msg.Targetting) {
            msg.Targetting = false;
            // Actually send it off yourself
            if (msg.invertTarget)
            {
                SendToAllClientsExcept(msg.TargetUUID, msg.ActualMsgType, msg, msg.channelID);
            }
            else
            {
                SendToClient(msg.TargetUUID, msg.ActualMsgType, msg, msg.channelID);
            }
            return false;
        }
        return true;
    }

    public void RegisterHandler(short msgID, NetworkMessageDelegate NMD)
    {
        if (isHost || isDual)
        {
            RNMD.Add(msgID, NMD);
            foreach(long ll in connections.Keys)
            {
                connections[ll].RegisterHandler(msgID, NMD);
            }
        }
        else
        {
            //clientConn.RegisterHandler(msgID, NMD);
            this.client.RegisterHandler(msgID, NMD);
        }
        if (isDual)
        {
            clientConn.RegisterHandler(msgID, NMD);
        }
    }
    public void Reset()
    {
        if (!isFin) { return; }
        isHost = false;
        isConn = false;
        isDual = false;
        isFin = false;
        AcceptingConnections = true;
        clientConn = null;
        Rconnections.Clear();
        connections.Clear();
        userUUID = 0;
        ignorePID.Clear();
        curPlayerIDs.Clear();
        IDplayers.Clear();
        playerIDs.Clear();
        RNMD.Clear();
    }
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
        if (RCM.self)
        {
            userUUID = RCM.UUID;
            return; // :D
        }
        else
        {
            if (RCM.disconnected)
            {
                if (!isHost)
                {
                    curPlayerIDs.Remove(RCM.UUID);
                    ignorePID.Add(RCM.UUID);
                }
                if (OnRemoteDisconnected != null)
                    OnRemoteDisconnected(RCM.UUID);
            }
            else if (!ignorePID.Contains(RCM.UUID))
            {
                if (!isHost)
                {
                    curPlayerIDs.Add(RCM.UUID);
                }
                if (OnRemoteConnected != null)
                    OnRemoteConnected(RCM.UUID);

            }
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
    public void ForceClient(NetworkClient cl)
    {
        clientConn = cl.connection;
    }
    public override void OnStartClient(NetworkClient client)
    {
        clientConn = client.connection;
        base.OnStartClient(client);
        client.RegisterHandler(BuiltinMsgTypes.RemoteInformation, RemoteConnected);
        isDual = isHost;
        isSafe = false;
        Debug.Log("Started client...");
    }
    // Called on server when client connects
    public override void OnServerConnect(NetworkConnection conn)
    {
        if (AcceptingConnections)
        {
            base.OnServerConnect(conn);
            Debug.Log("Client connected to server..." + conn.ToString());

            isHost = true;
        }
        else
        {
            conn.Disconnect();
        }
    }
    // Called on client when connected to server
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        Debug.Log("Server is present for client!..." + conn.ToString());
        clientConn = conn;
        isDual = isHost;
        isSafe = true;
        isConn = true;
    }
    // Called on server when client/server handshaking done
    public override void OnServerReady(NetworkConnection conn)
    {
        Debug.Log("Server is done with handshakes client!...");
        if (conn.isConnected || conn.hostId == -1)
        {
            base.OnServerReady(conn);
            Debug.Log("Boys be boys! " + serverUUIDs);
            long uID = serverUUIDs;
            if (conn.hostId == -1)
            {
                uID = -3;
            }
            else
            {
                serverUUIDs = serverUUIDs + 1;
                Debug.Log("UUID: " + serverUUIDs);
            }
            connections.Add(uID, conn);
            curPlayerIDs.Add(uID); // Actually never mind o-0
            playerIDs.Add(uID, conn.connectionId);
            IDplayers.Add(conn.connectionId, uID);
            foreach (short P in RNMD.Keys)
            {
                connections[uID].RegisterHandler(P, RNMD[P]);
            }
            Rconnections.Add(conn, uID);

            // Now we flood them
            foreach (long PID in curPlayerIDs)
            {
                if (PID == uID)
                {
                    connections[uID].Send(BuiltinMsgTypes.RemoteInformation, new RemoteConnectionMsg() { UUID = uID, disconnected = false, self = true });
                    continue;
                }
                connections[PID].Send(BuiltinMsgTypes.RemoteInformation, new RemoteConnectionMsg() { UUID = uID, disconnected = false });
                connections[uID].Send(BuiltinMsgTypes.RemoteInformation, new RemoteConnectionMsg() { UUID = PID, disconnected = false });
            }
            if (OnClientConnected != null)
                OnClientConnected(uID);
            isSafe = true;
        }
        else
        {
            Debug.Log("REJECTED!");
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
        base.OnServerDisconnect(conn);
        Debug.Log("Here..." + conn.ToString() + " " + Rconnections.ContainsKey(conn));

        if (!Rconnections.ContainsKey(conn)) { return; }
        long uID = Rconnections[conn];

        Debug.Log("Player UUID: " + uID);
        // Now we flood them
        foreach (long PID in curPlayerIDs)
        {
            if (PID == uID)
            {
                continue;
            }
            connections[PID].Send(BuiltinMsgTypes.RemoteInformation, new RemoteConnectionMsg() { UUID = uID, disconnected = true });
            //connections[uID].Send(BuiltinMsgTypes.RemoteInformation, new RemoteConnectionMsg() { UUID = PID, disconnected = false });
        }
        curPlayerIDs.Remove(uID);
        playerIDs.Remove(uID);
        IDplayers.Remove(conn.connectionId);

        Rconnections.Remove(conn);
        connections.Remove(uID);
        if (OnRemoteDisconnected != null)
            OnRemoteDisconnected(uID);
        Debug.Log("Done removing...");
        
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
    public long getUUID()
    {
        return userUUID;
    }
    
}

public class BuiltinMsgTypes
{
    public static short RemoteInformation = MsgType.Highest + 1;
    public static short Connectioninformation = (short)(RemoteInformation + 1);

    public static short Highest = Connectioninformation;
}
[System.Serializable]
public class RemoteConnectionMsg : MessageBase
{
    public long UUID;
    public bool disconnected = false;
    public bool self = false;
}
[System.Serializable]
public class UserInformationMsg : TaggedMessageBase
{
    public long UUID;
    public string name;
    public override void Deserialize(NetworkReader reader)
    {
        TaggedMessageBase.BaseDeserialize(reader, this);
        UUID = reader.ReadInt64();
        name = reader.ReadString();
    }
    public override void Serialize(NetworkWriter write)
    {
        TaggedMessageBase.BaseSerialize(write, this);
        write.Write(UUID);
        write.Write(name);
    }

}
[System.Serializable]
public class SimpleStringMessage : TaggedMessageBase
{
    public string payload;
    public override void Deserialize(NetworkReader reader)
    {
        TaggedMessageBase.BaseDeserialize(reader, this);
        payload = reader.ReadString();
    }
    public override void Serialize(NetworkWriter write)
    {
        TaggedMessageBase.BaseSerialize(write, this);
        write.Write(payload);
    }

}
// ALL MESSAGES SHALL USE TAGGED MESSAGE BASE INSTEAD AS AN EXTENDER!!!
[System.Serializable]
public abstract class TaggedMessageBase : MessageBase
{
    public long TargetUUID; // UUID to send to, -1 to the server (ignores invertTarget)
    public bool invertTarget; // Target all BUT the TargetUUID
    public short ActualMsgType;
    public bool Targetting = false;
    public int channelID = 0;
    public abstract override void Serialize(NetworkWriter write);
    public abstract override void Deserialize(NetworkReader reader);
    public static void BaseSerialize(NetworkWriter write, TaggedMessageBase TMB)
    {
        write.Write(TMB.TargetUUID);
        write.Write(TMB.invertTarget);
        write.Write(TMB.ActualMsgType);
        write.Write(TMB.Targetting);
        write.Write(TMB.channelID);
    }
    public static void BaseDeserialize(NetworkReader reader, TaggedMessageBase TMB)
    {
        TMB.TargetUUID = reader.ReadInt64();
        TMB.invertTarget = reader.ReadBoolean();
        TMB.ActualMsgType = reader.ReadInt16();
        TMB.Targetting = reader.ReadBoolean();
        TMB.channelID = reader.ReadInt32();
    }
}
#pragma warning restore CS0618 // Type or member is obsolete