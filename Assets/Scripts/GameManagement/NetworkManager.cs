using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CArbObj
{
    public ArbObj obj;
    public bool Cancelled = false;
}
public class NetworkManager : MonoBehaviour
{
    CampaignManagerMP CLPS;
    public int Port;
    public Dictionary<int, UnityEngine.Networking.QosType> NetChannels = new Dictionary<int, UnityEngine.Networking.QosType>();
    public static NetworkManager instance;

    public Dictionary<int, List<System.Action<CArbObj>>> registeredConsumers = new Dictionary<int, List<System.Action<CArbObj>> >();

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

    public static NetworkManager getInstance()
    {
        return instance; // :|
    }

    private void Network_HostDC(bool connecting, ConnectionObj obj)
    {

    }
    private void Network_ClientDC(bool connecting, ConnectionObj obj)
    {

    }
    private void HandleNetMsg(int op, int subop, ArbObj obj)
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
        this.Port = Port;
        
    }
    public void Update()
    {
        if (Networker.isActive())
        {
            Networker.Update();
        }
    }
    public void Start()
    {
        if(instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
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
        NetChannels.Add(Networker.AddChannel(UnityEngine.Networking.QosType.ReliableSequenced), UnityEngine.Networking.QosType.ReliableSequenced);
    }
}
