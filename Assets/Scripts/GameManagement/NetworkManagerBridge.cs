using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
#pragma warning disable CS0618 // Type or member is obsolete
public class CArbObj
{
    public ArbObj obj;
    public bool Cancelled = false;
}
public class NetworkManagerBridge : MonoBehaviour
{
    protected CampaignManagerMP CLPS;
    public MDNetworker net;

    [Header("MainMenuReferences")]
    // Assumes on a game "quit", this object will be removed and the instance cleared.
    public Image playerNamePrefab;
    public RectTransform scroller;


    public static short IDC = 0;

    [Header("Debugging/Changed from Scene")]
    public IPAP SocketInfo;
    private bool isClient = false;

    public Dictionary<int, UnityEngine.Networking.QosType> NetChannels = new Dictionary<int, UnityEngine.Networking.QosType>();
    public static NetworkManagerBridge instance;

    public Dictionary<int, List<System.Action<CArbObj>>> registeredConsumers = new Dictionary<int, List<System.Action<CArbObj>>>();
    
    public NetworkClient nc;
    public bool IsClient()
    {
        return isClient;
    }
    public bool SendMessageToServer(UserNETMSG nm)
    {
        if (!IsClient()) { return false; }
        return nc.Send((short)nm.subID,nm);//Networker.SendServer(nm, channel);
        
    }

    public bool SendMessageToClient(UserNETMSG nm, long UUID)
    {
        if (IsClient()) { return false; }
        NetworkServer.SendToClient(Networker.GetConnectionID(UUID), (short)nm.subID, nm);
        return true;
    }

    public bool SendMessageToAllClients(UserNETMSG nm)
    {
        if (IsClient()) { return false; }
        NetworkServer.SendToAll((short)nm.subID, nm);
        //NetworkTransport.Sen
        return true;
    }
    public bool SendMessageToAllOtherClients(UserNETMSG nm, int channel, long exceptUUID)
    {
        if (IsClient()) { return false; }
        foreach (long lluuid in Networker.playerIDs.Keys)
        {
            if(lluuid == exceptUUID)
            {
                continue;
            }
            NetworkServer.SendToClient(Networker.playerIDs[lluuid], (short)nm.subID, nm);
        }
        return true;
    }
    public void RegisterCARBHandler(int subID, System.Action<CArbObj> act)
    {
        if (!registeredConsumers.ContainsKey(subID))
        {
            registeredConsumers.Add(subID, new List<System.Action<CArbObj>>());
        }
        registeredConsumers[subID].Add(act);
    }
    public bool UnregisterCARBHandler(int subID, System.Action<CArbObj> act)
    {
        if (!registeredConsumers.ContainsKey(subID))
        {
            return false; // Failed
        }
        var v = registeredConsumers[subID];
        if (v.Contains(act))
        {
            return v.Remove(act); // Success?
        }
        else
        {
            return false; // Not found
        }
    }

    public static NetworkManagerBridge getInstance()
    {
        return instance; // :|
    }

    protected void Network_HostDC(bool connecting, ConnectionObj obj)
    {
        // Networker shut down, remove client

        // Delete this object after setting instance to null
        // Then switch scenes
        Debug.Log("Disconnected!!!");
        CLPS.pm.GetAcknowledge(CLPS.GoBack, "Lost connection!", ":(");
    }
    protected void Network_ClientDC(bool connecting, ConnectionObj obj)
    {
        Debug.Log("OI WE GOT A WHIPPERSNAPPER THAT DECIDED TO JOIN US UP HERE IN THE HOOD MY FUCKIN GOD CAN HE JUST SEND ME A FUCKIN MESSAGE YET!?");
        
    }
    protected void HandleNetMsg(int op, int subop, ArbObj obj)
    {
        if (!registeredConsumers.ContainsKey(subop))
        {
            Debug.LogError("Unregistered message was caught! Ensure _all_ messages are registered to the network manager! SUBOP: " + subop);
            return;
        }
        CArbObj c = new CArbObj() { obj = obj, Cancelled = false };
        var k = registeredConsumers[subop];
        foreach(System.Action<CArbObj> a in k)
        {
            a(c);
            if (c.Cancelled)
            {
                return;
            }
        }
    }
    public void StartServer(int Port, CampaignManagerMP CMMP)
    {
        CLPS = CMMP;

        this.SocketInfo = new IPAP("127.0.0.1:"+Port);
        Networker.port = this.SocketInfo.PORT;
        Networker.web_port = this.SocketInfo.PORT + 1;
        net.networkAddress = "localhost";
        net.networkPort = this.SocketInfo.PORT;
        nc = net.StartHost();
    }

    public IEnumerator StartClient(string IPM, CampaignManagerMP CMMP, System.Action connected)
    {
        CLPS = CMMP;

        this.SocketInfo = new IPAP(IPM);
        Networker.port = this.SocketInfo.PORT;
        Networker.web_port = this.SocketInfo.PORT + 1;
        Networker.ConnIP = this.SocketInfo.IP;
        /*Networker.StartClient();*/
        net.networkAddress = this.SocketInfo.IP;
        net.networkPort = this.SocketInfo.PORT;
        nc = net.StartClient();

        while(!net.isFin && !net.isConn)
        {
            Debug.Log(net.isFin + " " + net.isConn);
            yield return new WaitForSeconds(1);
        }
        connected();
    }
    public void Update()
    {
        /*if (Networker.isActive())
        {
            Networker.Update();
        }*/
    }
    public void Start()
    {
        if(instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        /*Networker.Init();
        instance = this;
        DontDestroyOnLoad(gameObject);
        Networker.OnClientConnect += Network_ClientDC;
        Networker.OnServerDisconnected += Network_HostDC;
        Networker.OnUserEvent += HandleNetMsg;
        Networker.OnOtherEvent += HandleNetMsg;
        Networker.setMaxUsers(16);
        NetChannels.Add(0, UnityEngine.Networking.QosType.Reliable);
        NetChannels.Add(Networker.AddChannel(UnityEngine.Networking.QosType.ReliableStateUpdate), UnityEngine.Networking.QosType.ReliableStateUpdate);
        NetChannels.Add(Networker.AddChannel(UnityEngine.Networking.QosType.StateUpdate), UnityEngine.Networking.QosType.StateUpdate);
        NetChannels.Add(Networker.AddChannel(UnityEngine.Networking.QosType.ReliableSequenced), UnityEngine.Networking.QosType.ReliableSequenced);*/

    }

    public bool ConnectToServer(string IPP, CampaignManagerMP mp)
    {
        // assumes ip and port!
        try
        {
            this.SocketInfo = new IPAP(IPP);

            Networker.port = this.SocketInfo.PORT;
            Networker.web_port = this.SocketInfo.PORT+1;
            Networker.ConnIP = this.SocketInfo.IP;
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Can't connect! Why: "+ e.Message);
            return false;
        }
    }
}
[System.Serializable]
public class IPAP
{
    public string IP;
    public int PORT;
    public IPAP(string IPP)
    {
        string[] bb = IPP.Split(':');
        if(bb.Length != 2)
        {
            throw new System.Exception("Not valid combo.");
        }
        if(!int.TryParse(bb[1], out PORT))
        {
            throw new System.Exception("Not a valid port.");
        }
        IP = bb[0];
    }
}
#pragma warning restore CS0618 // Type or member is obsolete
