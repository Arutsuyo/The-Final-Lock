using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;
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
    public List<Image> playersReg = new List<Image>();
    public Dictionary<long, Image> playerLabelss = new Dictionary<long, Image>();


    public static short IDC = 0;

    [Header("Debugging/Changed from Scene")]
    public IPAP SocketInfo;
    private bool isClient = false;

    public Dictionary<int, UnityEngine.Networking.QosType> NetChannels = new Dictionary<int, UnityEngine.Networking.QosType>();
    public static NetworkManagerBridge instance;

    public Dictionary<int, List<System.Action<CArbObj>>> registeredConsumers = new Dictionary<int, List<System.Action<CArbObj>>>();
    public Dictionary<long, string> userNames = new Dictionary<long, string>();
    public NetworkClient nc;
    public NetworkClient webc;
    public long PLAYERUUID = -10;
    public bool IsClient()
    {
        return isClient;
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
        Debug.Log("Starting Server...");
        //addUserName(new UserInformationMsg() { UUID = -3, name = PlayerPrefs.GetString("PlayerName") + " (<color=\"#cccc00\">Host</color>)" });
        this.SocketInfo = new IPAP("127.0.0.1:"+Port);
        //Networker.port = this.SocketInfo.PORT;
        //Networker.web_port = this.SocketInfo.PORT + 1;
        net.networkAddress = "localhost";
        net.networkPort = this.SocketInfo.PORT;
#if UNITY_WEBGL && !UNITY_EDITOR
        //net.useWebSockets = true;
        //net.networkPort = this.SocketInfo.PORT + 1;
        //webc = net.StartHost();
        Debug.LogError("You can not host from a WEBGL game! IGNORING REQUEST!");
        return;
#else

        //net.useWebSockets = true;
        //net.networkPort = this.SocketInfo.PORT + 1;
        //net.StartServer();
        // Actually try both....
        net.useWebSockets = false;
        net.networkPort = this.SocketInfo.PORT;
        nc = net.StartHost();
        net.RegisterHandler(BuiltinMsgTypes.Connectioninformation, HandleName);
        
        StartCoroutine(SendNameSoon());
#endif
    }
    public IEnumerator SendNameSoon()
    {
        yield return null;
        StartCoroutine(SendName());
    }
    public void DisplayConnecting()
    {
        // Does nothing T_T
    }
    public void OnClientUUIDGet(long UUID)
    {
        if (playerLabelss.ContainsKey(-4))
        {
            removeUserName(-4);
            
            addUserName(new UserInformationMsg() { UUID = UUID, name = PlayerPrefs.GetString("PlayerName") });
        }
        PLAYERUUID = UUID;
    }
    public IEnumerator StartClient(string IPM, CampaignManagerMP CMMP, System.Action connected)
    {
        CLPS = CMMP;
        addUserName(new UserInformationMsg() { UUID = -4, name = PlayerPrefs.GetString("PlayerName") });
        this.SocketInfo = new IPAP(IPM);
        Debug.Log("Starting Client..." + this.SocketInfo.IP + " :: " + this.SocketInfo.PORT);
        /*Networker.port = this.SocketInfo.PORT;
        Networker.web_port = this.SocketInfo.PORT + 1;
        Networker.ConnIP = this.SocketInfo.IP;
        /*Networker.StartClient();*/
        net.networkAddress = this.SocketInfo.IP;

#if UNITY_WEBGL && !UNITY_EDITOR
        net.networkPort = this.SocketInfo.PORT+1;
        net.useWebSockets = true;
#else
        net.networkPort = this.SocketInfo.PORT;
        // Actually try both....
#endif
        net.UserUUIDObtained += OnClientUUIDGet;
        //net.useWebSockets = true;
        nc = net.StartClient();
        
        net.ForceClient(nc);
        
        net.RegisterHandler(BuiltinMsgTypes.Connectioninformation, HandleName);
        DisplayConnecting();
        while(!net.isFin && !net.isConn)
        {
            Debug.Log(net.isFin + " " + net.isConn +" " +net.IsClientConnected());

            yield return new WaitForSeconds(1);
        }
        Debug.Log("Skipped?! " + net.isFin + " " + net.isConn + " " + net.IsClientConnected());
        if (net.isFin)
        {
            // Reset it.
            net.Reset();
#if UNITY_WEBGL && !UNITY_EDITOR
            yield break;
#else
            // try again but with the websockets...
            net.networkPort+= 1;
            net.useWebSockets = true;
            nc = net.StartClient();
            net.ForceClient(nc);
            net.RegisterHandler(BuiltinMsgTypes.Connectioninformation, HandleName);
            while (!net.isFin && !net.isConn)
            {
                Debug.Log(net.isFin + " " + net.isConn);
                yield return new WaitForSeconds(1);
            }
            if (net.isFin)
            {
                net.Reset();
                yield break;
            }
#endif

        }

        StartCoroutine(SendName());
        connected();
    }
    public string HOSTSTR()
    {
        return "(<color=\"#cccc00\">Host</color>)";
    }
    public void addUserName(UserInformationMsg UIM)
    {
        Debug.Log("woot" + UIM.UUID);
        if (playerLabelss.ContainsKey(UIM.UUID)) { Debug.Log("I already have that key though :("); return; }
        Image ii = Instantiate<Image>(playerNamePrefab, scroller);
        Debug.Log(ii.rectTransform.sizeDelta);
        ii.rectTransform.anchoredPosition = new Vector2(0,-ii.rectTransform.sizeDelta.y * (playersReg.Count+.5f));
        playersReg.Add(ii);
        playerLabelss.Add(UIM.UUID, ii);
        Text tt = ii.GetComponentInChildren<Text>();
        tt.text = UIM.name;
        userNames.Add(UIM.UUID, UIM.name);
    }
    public void removeUserName(long uuid)
    {
        Debug.Log("Boooooo!!!" + uuid);
        Image ii = playerLabelss[uuid];
        int pos = playersReg.IndexOf(ii);
        playersReg.Remove(ii);
        Destroy(playerLabelss[uuid].gameObject);
        playerLabelss.Remove(uuid);
        userNames.Remove(uuid);
        for (int i = pos; i < playersReg.Count; i++)
        {
            // Move them up by the delta...
            playersReg[i].rectTransform.anchoredPosition = new Vector2(0, -playersReg[i].rectTransform.sizeDelta.y * (i + .5f));
        }
    }
    public void HandleName(NetworkMessage nm)
    {
        Debug.Log(nm.reader.Length + " " + nm.reader.Position);
        uint op = nm.reader.Position;
        nm.reader.SeekZero();
        byte[] bbs = nm.reader.ReadBytes(nm.reader.Length);
        StringBuilder sbb = new StringBuilder();
        foreach (byte b in bbs)
        {
            sbb.Append(b + " ");
        }
        Debug.Log(sbb.ToString());
        nm.reader.SeekZero();
        nm.reader.ReadBytes((int)op);
        UserInformationMsg TMB = nm.ReadMessage<UserInformationMsg>();
        if (net.CheckPacket(TMB))
        {
            Debug.Log("Handling a name request :D " + TMB.name + " " + TMB.UUID);
            addUserName(TMB);
        }
        else
        {
            // You ignore it, it wasn't for you :(
            Debug.Log("Ignoring packet!");
        }
    }
    public IEnumerator SendName()
    {
        string sst = PlayerPrefs.GetString("PlayerName");
        Debug.Log("Here?");
        while (!net.isSafe)
        {
            yield return new WaitForSeconds(0.5f);
            Debug.Log("Boy: " + net.isSafe + " " + net.isHost + " " + net.isConn + " " + net.isFin);
        }
        if (net.IsClientConnected())
        {
            if (net.isHost)
            {
                net.SendToAllClients(BuiltinMsgTypes.Connectioninformation, new UserInformationMsg() { UUID = net.getUUID(), ActualMsgType = 0, invertTarget = false, name = sst + " "+HOSTSTR(), TargetUUID = -1 });
                //Debug.Log("Here!");
            }
            else
            {
                Debug.Log(net.SendToAllOtherClients(BuiltinMsgTypes.Connectioninformation, new UserInformationMsg() { UUID = net.getUUID(), name = PlayerPrefs.GetString("PlayerName") }));
                Debug.Log(net.SendToServer(BuiltinMsgTypes.Connectioninformation, new UserInformationMsg() { UUID = net.getUUID(), name = PlayerPrefs.GetString("PlayerName")}));
            }
        }
        else
        {
            Debug.Log("Not allowed to do that just yet!");
        }
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

        net.OnClientConnected += Net_OnClientConnected;
        net.OnRemoteDisconnected += removeUserName;
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

    private void Net_OnClientConnected(long userID)
    {
        // Create a new "connecting template"...
        // Send user our name...
        // Also should send all other names and IDs...
        
        net.SendToClient(userID, BuiltinMsgTypes.Connectioninformation, new UserInformationMsg() { UUID = -3, ActualMsgType = 0, invertTarget = false, name = PlayerPrefs.GetString("PlayerName") + " " + HOSTSTR(), TargetUUID = -1 });
        // Need to send all other users...
        // Have the server send over all players...
        foreach(long UUID in playerLabelss.Keys)
        {
            if(UUID == -2)
            {
                continue;
            }
            net.SendToClient(userID, BuiltinMsgTypes.Connectioninformation, new UserInformationMsg() { UUID = UUID, ActualMsgType = 0, invertTarget = false, name = playerLabelss[UUID].GetComponentInChildren<Text>().text, TargetUUID = -1 });
        }
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
