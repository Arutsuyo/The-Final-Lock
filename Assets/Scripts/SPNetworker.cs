using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
#pragma warning disable CS0618 // Type or member is obsolete
public class SPNetworker : MDNetworker
{
    public void Start()
    {
        curPlayerIDs.Add(-10);
        userUUID = -10;
        serverUUIDs = -10;
    }

    public Dictionary<short, NetworkMessageDelegate> ARRG = new Dictionary<short, NetworkMessageDelegate>();


    public new NetworkConnection GetConnection(long UUID)
    {
        return null;
    }
    public new bool SendToOtherClient(long UUID, short msgType, TaggedMessageBase msg, int channelID=0)
    {
        return false;
    }
    public new bool SendToAllOtherClients(short msgType, TaggedMessageBase msg, int channelID = 0)
    {
        return false;
    }
    public new bool SendToServer(short msgType, TaggedMessageBase msg, int channelID = 0)
    {
        if (!ARRG.ContainsKey(msgType))
        {
            Debug.LogWarning("No such msg registered!");
            return false;
        }
        else
        {
            if(ARRG[msgType] != null)
            {
                NetworkWriter nw = new NetworkWriter();
                nw.StartMessage(msgType);
                msg.Serialize(nw);
                nw.FinishMessage();
                ARRG[msgType](new NetworkMessage() {msgType=msgType, channelId = channelID, conn=null, reader = new NetworkReader(nw) });
                return true;
            }
            return false;
        }
    }
    public new bool SendToClient(long UUID, short msgType, TaggedMessageBase msg, int channelID = 0)
    {
        if(UUID == -10)
        {
            if (!ARRG.ContainsKey(msgType))
            {
                Debug.LogWarning("No such msg registered!");
                return false;
            }
            else
            {
                if (ARRG[msgType] != null)
                {
                    NetworkWriter nw = new NetworkWriter();
                    nw.StartMessage(msgType);
                    msg.Serialize(nw);
                    nw.FinishMessage();
                    ARRG[msgType](new NetworkMessage() { msgType = msgType, channelId = channelID, conn = null, reader = new NetworkReader(nw) });
                    return true;
                }
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    public new bool SendToAllClients(short msgType, TaggedMessageBase msg, int channelID = 0)
    {
        if (!ARRG.ContainsKey(msgType))
        {
            Debug.LogWarning("No such msg registered!");
            return false;
        }
        else
        {
            if (ARRG[msgType] != null)
            {
                NetworkWriter nw = new NetworkWriter();
                nw.StartMessage(msgType);
                msg.Serialize(nw);
                nw.FinishMessage();
                ARRG[msgType](new NetworkMessage() { msgType = msgType, channelId = channelID, conn = null, reader = new NetworkReader(nw) });
                return true;
            }
            return false;
        }
    }
    public new bool SendToAllClientsExcept(long UUID, short msgType, TaggedMessageBase msg, int channelID = 0)
    {
        if (UUID != -10)
        {
            if (!ARRG.ContainsKey(msgType))
            {
                Debug.LogWarning("No such msg registered!");
                return false;
            }
            else
            {
                if (ARRG[msgType] != null)
                {
                    NetworkWriter nw = new NetworkWriter();
                    nw.StartMessage(msgType);
                    msg.Serialize(nw);
                    nw.FinishMessage();
                    ARRG[msgType](new NetworkMessage() { msgType = msgType, channelId = channelID, conn = null, reader = new NetworkReader(nw) });
                    return true;
                }
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    public new bool CheckPacket(TaggedMessageBase msg)
    {
        return true;
    }
    public new void RegisterHandler(short msgID, NetworkMessageDelegate NMD)
    {
        ARRG.Add(msgID, NMD);
    }
    public new bool IsHost() { return true; }
    public new bool IsConnected() { return true; }
    public new bool IsFinished() { return false; }
    public void OnStartSP() { 
        // done :D
    }
}
#pragma warning restore CS0618 // Type or member is obsolete
