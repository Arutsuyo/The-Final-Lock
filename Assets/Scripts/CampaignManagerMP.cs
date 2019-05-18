using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

using System.Xml;
using System;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Text;
#pragma warning disable CS0618 // Type or member is obsolete
public class MPMsgTypes
{
	public static short RoundStarting = (short)(BuiltinMsgTypes.Highest + 1);
	public static short RoomInformation = (short)(MPMsgTypes.RoundStarting + 1);
	public static short GameFlow = (short)(MPMsgTypes.RoomInformation + 1);
	public static short Interactions = (short)(MPMsgTypes.GameFlow + 1);
	public static short FinInteractions = (short)(MPMsgTypes.Interactions + 1);
    public static short GameFailed = (short)(MPMsgTypes.FinInteractions + 1);
    public static short GameSucceed = (short)(MPMsgTypes.GameFailed + 1);
	public static short Highest = GameSucceed;
}



public class CampaignManagerMP : MonoBehaviour
{
	[Header("Object References")]
	public CampaignManagerMP otherContestant; // Other CMMP to DESTROY! (If they go MP...that is)
	public Button BackBtn;
	public Button StartBtn;
	public ScrollRect CampaignRect;
	public RectTransform CampaignsContent;
	public Button CampaignClone;
	public Button Selected = null;
	public GameObject[] holoDeck;
	public string[] holoDeckNames;
	public GameObject holoStorage;
	public Animator PlayerAnimation;
	public PromptManager pm;
	public NetworkManagerBridge nm;
	public Animator doorAnimation;
	public Animator holoAnimation;
	public Text lobbyWaiter;
	public Camera cam;
	public AsyncOperation AOP;
    public DestroyEverything Thanos;
	public GameObject SpawnPlayerPrefab;
    public GameObject Reticle;
	public int countDone = 0;
	public Image startControl;
	public int ActiveID = 0;

	public int Port;
	public Dictionary<long, GameObject> playerObjs = new Dictionary<long, GameObject>();
	public RoomManager roomMngr;
	public void AnnounceRoomManager()
	{
		roomMngr = RoomManager.instance;
	}

	[Header("Rooms Offered")]
	public string[] Rooms;
	public string RoomSearchPath;

	[Header("Debugging")]
	public EscapeRoom[] erooms;
	public Button[] Campaigns; // Created at run time.
	public GameObject holo;

	public static CampaignManagerMP instance;
	public static CampaignManagerMP getInstance()
	{
		return instance;
	}


	public void GenerateRoom()
	{
		// do nothing for now for generation

		// do nothing for now for sending room

		// Send message to those who have the room loaded to wake up.
		Debug.Log("Yo yall here?!");
		nm.net.SendToAllClients(MPMsgTypes.GameFlow, new SimpleStringMessage() { payload = "Wakeup." });
		//StartCoroutine(HandleWakeup());
	}

	public void SendServerReady(long uuid)
	{
		StartCoroutine(HandleWakeup());
	}

	public void RoomGenerationAcceptor(NetworkMessage nm)
	{

	}
    public void Cleanup()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        instance = null;
        RoomManager.instance = null;
        NetworkManagerBridge.instance = null;
        NetworkServer.Shutdown();
        NetworkServer.Reset();
        try { NetworkClient.ShutdownAll(); } catch (Exception) { };
        try { MDNetworker.singleton.StopServer(); } catch (Exception) { };
        try { MDNetworker.singleton.StopClient(); } catch (Exception) { };
        try { MDNetworker.singleton.StopHost(); } catch (Exception) { };
        DestroyEverything NewThanos = Instantiate<DestroyEverything>(Thanos);
        NewThanos.DoubleThanosSnap(0);
    }
    public void ShowLoseScreen(NetworkMessage nm1)
    {
        Reticle.SetActive(false);
        Debug.Log("Hey Lose Screen!");
        SimpleStringMessage ssm = nm1.ReadMessage<SimpleStringMessage>();
        if(ssm.payload.Equals("You died."))
        {
            roomMngr.StartFail(Cleanup);
        }
        else
        {
            Debug.LogError("Unexpected lose screen verification!");
        }
    }
    public void ShowWinScreen(NetworkMessage nm1)
    {
        Reticle.SetActive(false);
        Debug.Log("Hey Win Screen!");
        SimpleStringMessage ssm = nm1.ReadMessage<SimpleStringMessage>();
        if(ssm.payload.Equals("You win."))
        {
            roomMngr.StartSucc(delegate { }, Cleanup);
        }
        else
        {
            Debug.LogError("Unexpected win screen verification!");
        }
    }

	public void CountIncrementer()
	{
		// ASSUMES SYNCHRONIZED, WHICH IS DANGEROUS!
		countDone++;
		Debug.Log(countDone);
	}
	public String getSeperated<T>(List<T> t)
	{
		StringBuilder sb = new StringBuilder();
		foreach (T t1 in t)
		{
			sb.Append(t1.ToString() + ", ");
		}
		return sb.ToString();
	}
    public void OutOfTime()
    {
        Debug.Log("Hey out of time!");
        NetworkServer.SendByChannelToAll(MPMsgTypes.GameFailed, new SimpleStringMessage() { payload = "You died." }, 0);
    }
	public IEnumerator HandleWakeup()
	{
		if (cam != null && cam.gameObject != null)
		{
			cam.gameObject.SetActive(false);
		}

		if (nm.net.isHost)
		{
			List<long> UUIDs = nm.net.GetAllPlayers();
			Debug.Log((UUIDs.Count - 1) + " " + getSeperated(UUIDs));

			while (countDone != UUIDs.Count - 1)
			{
				yield return null;
			}
			playerObjs.Add(-3, Instantiate(SpawnPlayerPrefab));
			NetworkServer.Spawn(playerObjs[-3]);
			playerObjs[-3].GetComponent<NetworkIdentity>().AssignClientAuthority(nm.net.connections[UUIDs[0]]);
			Debug.Log("BOY: " + UUIDs[0]);
			playerObjs[-3].GetComponent<PlayerMovementMP>().RpcChangeName(nm.userNames[UUIDs[0]]);
			Debug.Log("UUID counts: " + UUIDs.Count);

			foreach (long ID in UUIDs)
			{
				Debug.Log(ID + " " + nm.net.GetConnection(ID).ToString());
				if (ID < 0) { continue; }
				playerObjs.Add(ID, Instantiate(SpawnPlayerPrefab));
				NetworkServer.SpawnWithClientAuthority(playerObjs[ID], nm.net.connections[ID]);
				playerObjs[ID].GetComponent<PlayerMovementMP>().RpcChangeName(nm.userNames[ID]);
			}
			NetworkServer.SpawnObjects();
			while (roomMngr == null)
			{
				yield return null;
			}
			roomMngr.roomTimer.CmdStartTheTimers();
            roomMngr.roomTimer.wallTimer.TimerExpired += OutOfTime;
		}
		else
		{
			nm.net.SendToServer(MPMsgTypes.GameFlow, new SimpleStringMessage() { payload = "Room Loaded." });
		}
		GameObject go = GameObject.FindGameObjectWithTag("MainCamera");
        

        Debug.Log(go);
		go.GetComponent<Camera>().enabled = true;
        Reticle = GameObject.FindGameObjectWithTag("HitMarkers");
        // Enable character here...but need additional help.
    }
	public void GameFlowSM(NetworkMessage nm)
	{
		// Find the main camera, turn it on, disable your camera.
		SimpleStringMessage ssm = nm.ReadMessage<SimpleStringMessage>();
		switch (ssm.payload)
		{
			case "Wakeup.":
				Debug.Log("WAKE ME UP INSIDE!");
				StartCoroutine(HandleWakeup());
				break;
			case "Room Loaded.":
				CountIncrementer();
				break;
		}
	}

	private void RegisterListenersHere()
	{
		nm.net.RegisterHandler(MPMsgTypes.RoundStarting, SimpleStringPayload);
		nm.net.RegisterHandler(MPMsgTypes.RoomInformation, RoomGenerationAcceptor);
		nm.net.RegisterHandler(MPMsgTypes.GameFlow, GameFlowSM);
        nm.net.RegisterHandler(MPMsgTypes.GameFailed, ShowLoseScreen);
        nm.net.RegisterHandler(MPMsgTypes.GameSucceed, ShowWinScreen);
	}



	public virtual void UpdateCampaignUI(int id)
	{
		//Debug.Log("Clicked campaign " + id);
		if (holo != null)
		{
			Destroy(holo);
		}
		holo = Instantiate(holoDeck[Array.FindIndex(holoDeckNames, w => w.Equals(erooms[id].holoname))], holoStorage.transform);
		Debug.Log(erooms[id].holoname + " " + Array.FindIndex(holoDeckNames, w => w.Equals(erooms[id].holoname)));
		StartBtn.gameObject.SetActive(true);
		StartBtn.interactable = true;
		StartBtn.onClick.RemoveAllListeners();
		StartBtn.onClick.AddListener(() => Go(id));
	}
	public void PromptTryJoin()
	{
		pm.GetString(GetJoin, 5, 25, "Please enter the server's ip:port combo: ", "127.0.0.1:" + 25565, "Connect");
		lobbyWaiter.text = "Waiting for host to start the game!";
	}
	public void DoConnect()
	{
		//Debug.Log("Do Connect!");
		//Debug.Log(Networker.isActive() + " " + Networker.isConnected());
		// Send a message of our name
		RegisterListenersHere();
	}

	public void ShortCutJoin()
	{
		lobbyWaiter.text = "Waiting for host to start the game!";
		startControl.gameObject.SetActive(false);
		StartCoroutine(nm.StartClient("127.0.0.1:25565", this, DoConnect));
		Debug.Log("ToLOBBY!");
		PlayerAnimation.Play("NewToLobby", -1, 0.0f);
	}
	public void ShortCutHost()
	{
		lobbyWaiter.text = "Waiting on you to start the game!";
		startControl.gameObject.SetActive(true);
		nm.StartServer(25565, this);
		RegisterListenersHere();
		PlayerAnimation.Play("NewToLobby", -1, 0.0f);

	}
	public void GetJoin(System.Object o)
	{
		Debug.Log("GetJOIN");
		if (o != null)
		{
			PlayerAnimation.SetTrigger("ToLobby");
			lobbyWaiter.text = "Waiting for host to start the game!";
			startControl.gameObject.SetActive(false);
			StartCoroutine(nm.StartClient((string)o, this, DoConnect));

			Debug.Log("ToLOBBY!");
		}
	}

	public void GetPort(System.Object o)
	{
		int oo;
		if (int.TryParse((string)o, out oo))
		{
			Port = oo;
			// Now actually launch it.
			//Debug.Log("Launching campaign ID " + ActiveID + " in 3...2....1...JUMP");
			startControl.gameObject.SetActive(true);
			lobbyWaiter.text = "Waiting on you to start the game!";
			PlayerAnimation.SetTrigger("ToLobby");
			nm.StartServer(Port, this);
			RegisterListenersHere();
			//doorAnimation.SetTrigger("Door");
			//holoAnimation.SetTrigger("ToDoor");
		}
		else
		{
			// Boy! Cancelled... :(
			foreach (Button bb in Campaigns)
			{
				bb.interactable = true;
			}
		}
	}

	public virtual void Go(int id)
	{
		foreach (Button bb in Campaigns)
		{
			bb.interactable = false;
		}
		pm.GetNumber(GetPort, 1000, 65535, "Please enter the port for hosting:", "" + 25565, "Start Server");
		ActiveID = id;
	}
	public void Starting()
	{
		BackBtn.interactable = false;
		StartBtn.interactable = false;
	}
	public void HideUI()
	{
		BackBtn.interactable = false;
		StartBtn.interactable = false;
		CampaignRect.gameObject.SetActive(false);
		BackBtn.gameObject.SetActive(false);
		StartBtn.gameObject.SetActive(false);
		Selected = null;
		foreach (Button b in Campaigns)
		{
			Destroy(b.gameObject);
		}
		if (holoStorage != null)
		{
			Destroy(holo);
		}
	}

	public void SimpleStringPayload(NetworkMessage nm)
	{
		// We know it is a simple string payload!
		SimpleStringMessage ms = nm.ReadMessage<SimpleStringMessage>();
		switch (ms.payload)
		{
			case "Round Starting!":
				StartCoroutine(CountDown());
				break;
			default:
				Debug.LogError("Uncaught simple string payload: " + ms.payload);
				break;
		}
	}

	public void StartMission()
	{
		// Send out the starting mission signal
		// Start it ourselves...count to 3...GO
		// Then call ToCampaign on the animator and swap scenes...note: the server should also send the user's the scene when it next appears o-o
		nm.net.AcceptingConnections = false;
		countDone = 0;
		NetworkServer.SetAllClientsNotReady();
		nm.net.SendToAllClients(MPMsgTypes.RoundStarting, new SimpleStringMessage() { payload = "Round Starting!" });
		StartCoroutine(CountDown());
	}

	public IEnumerator CountDown()
	{
		Destroy(otherContestant.gameObject);
		instance = this;
		nm.net.ClientSceneChanged += SendServerReady;
		lobbyWaiter.text = "Round starting in 3";
		for (int i = 3; i >= 1; i--)
		{
			yield return new WaitForSecondsRealtime(0.25f);
			lobbyWaiter.text += '.';
			yield return new WaitForSecondsRealtime(0.25f);
			lobbyWaiter.text += '.';
			yield return new WaitForSecondsRealtime(0.25f);
			lobbyWaiter.text += '.';
			yield return new WaitForSecondsRealtime(0.25f);
			lobbyWaiter.text += (i - 1);
		}
		lobbyWaiter.text += "!";

		PlayerAnimation.SetTrigger("ToCampaign");
		// Swap scenes to Room 1 empty...but for right now just to scene 2.
		yield return new WaitForSecondsRealtime(1.3f);
		//AOP = SceneManager.LoadSceneAsync(1);
		if (nm.net.isHost)
		{
			// Send all clients the room details
			nm.net.ServerChangeScene("Room 1");

		}
	}
	public void GoBack(System.Object o)
	{
		PlayerAnimation.SetTrigger("Back");
		HideUI();
	}

	public void GoBackBridge()
	{
		GoBack(null);
	}
	public void ShowUI()
	{
		CampaignRect.transform.parent.gameObject.SetActive(true);
		BackBtn.interactable = true;
		Selected = null;
		QueryRooms();
		List<Button> bb = new List<Button>();
		int count = 0;
		foreach (EscapeRoom ee in erooms)
		{
			RectTransform gg = Instantiate<RectTransform>(CampaignClone.GetComponent<RectTransform>(), CampaignsContent.transform);
			gg.localPosition += new Vector3(0, -count * gg.sizeDelta.y, 0);
			bb.Add(gg.GetComponent<Button>());
			gg.GetChild(0).GetComponent<Text>().text = ee.name;
			Debug.Log(count);
			int copy = count + 0;
			gg.GetComponent<Button>().onClick.AddListener(delegate { UpdateCampaignUI(copy); });
			count++;
		}
		Campaigns = bb.ToArray<Button>();
		BackBtn.gameObject.SetActive(true);
		CampaignRect.gameObject.SetActive(true);

		Selected = null;
		Debug.Log("Show UI!");
	}

	public void Start()
	{
		HideUI();
		DontDestroyOnLoad(this.gameObject);

		
	}
	protected EscapeRoom ParseFile(XmlDocument x)
	{
		foreach (XmlElement node in x)
		{
			if (node.Name.ToLower().Equals("room"))
			{
				return new EscapeRoom(node);
			}
			else
			{
				Debug.LogError("Unexpected node found in base of XML tree: " + node.Name);
			}
		}
		Debug.LogError("File did not contain an escape room!");
		return null;
	}

	public virtual void QueryRooms()
	{
		TextAsset[] ss = Resources.LoadAll<TextAsset>(RoomSearchPath);
		List<EscapeRoom> eerooms = new List<EscapeRoom>();
		// Debug.Log(ss.Length);
		foreach (TextAsset sss in ss)
		{
			try
			{
				XmlDocument xx = new XmlDocument();
				Debug.Log(sss.text);
				xx.LoadXml(sss.text);
				EscapeRoom e = ParseFile(xx);
				if (e != null)
				{

					eerooms.Add(e);
					continue;
				}
			}
			catch (Exception e)
			{
				Debug.LogError("Malformed escape room file! " + e.StackTrace);
			}
		}
		erooms = eerooms.ToArray<EscapeRoom>();

	}
}

[System.Serializable]
public class EscapeRoom
{
	public string name;
	public string holoname;
	public int numRooms;
	public int[] sizeofRooms;
	public bool isRandom;
	public bool isSP = false;
	public Puzzle mainPuzzle;

	public EscapeRoom(XmlElement xe)
	{
		foreach (XmlElement ee in xe)
		{
			switch (ee.Name.ToLower())
			{
				case "name":
					name = ee.InnerText;
					break;
				case "holoname":
					holoname = ee.InnerText;
					break;
				case "issingleplayer":
					isSP = true;
					break;
				case "numrooms":
					numRooms = int.Parse(ee.InnerText);
					break;
				case "userandomgeneration":
					isRandom = int.Parse(ee.InnerText) == 1;
					break;
				case "sizeofrooms":
					DetermineRooms(ee);
					break;
				case "puzzle":
					DeterminePuzzles(ee);
					break;
				default:
					Debug.LogWarning("Unknown tag: " + ee.Name);
					break;
			}
		}
	}
	private void DetermineRooms(XmlElement e)
	{
		List<int> sizes = new List<int>();
		foreach (XmlElement es in e)
		{
			if (es.Name.ToLower().Equals("roomsize"))
			{
				sizes.Add(int.Parse(es.InnerText));
			}
		}
		sizeofRooms = sizes.ToArray<int>();
	}

	private void DeterminePuzzles(XmlElement e)
	{
		mainPuzzle = new Puzzle(e);
	}
}
public class Puzzle
{
	public int mechanismID;
	public Key[] keys;
	public Puzzle[] puzzles; // note, puzzles are topologically ordered.
							 // That is, parents may depend on children, but children CAN NOT
							 // depend on parents...
	public Clue[] clues;
	public Puzzle(XmlElement e)
	{
		List<Puzzle> puz = new List<Puzzle>();
		List<Clue> clues1 = new List<Clue>();
		foreach (XmlElement eel in e)
		{
			switch (eel.Name.ToLower())
			{
				case "mechanism":
					List<Key> keys1 = new List<Key>();
					foreach (XmlElement ej in eel)
					{
						switch (ej.Name.ToLower())
						{
							case "id":
								mechanismID = int.Parse(ej.InnerText);
								break;
							case "keys":
								foreach (XmlElement ek in ej)
								{
									if (ek.Name.ToLower().Equals("key"))
									{
										keys1.Add(new Key(ek));
									}
									else
									{
										Debug.Log("Unknown tag in keys tag: " + ek.Name);
									}
								}
								break;
							default:
								Debug.LogError("Unknown key in mechanism tag: " + ej.Name);
								break;
						}
					}
					keys = keys1.ToArray<Key>();

					break;
				case "clues":
					foreach (XmlElement ee in eel)
					{
						switch (ee.Name.ToLower())
						{
							case "clue":
								clues1.Add(new Clue(ee));
								break;
							case "puzzle":
								puz.Add(new Puzzle(ee));
								break;
							default:
								Debug.LogError("Unknown key in clues tag: " + ee.Name);
								break;
						}
					}
					break;
				default:
					Debug.LogError("Unknown key in puzzle tag: " + eel.Name);
					break;
			}
		}

		puzzles = puz.ToArray<Puzzle>();
		clues = clues1.ToArray<Clue>();
	}

}
public class Key
{
	public bool isEnviron;
	public int ID;
	public int variant;
	public Key(XmlElement e)
	{
		foreach (XmlElement ej in e)
		{
			switch (ej.Name.ToLower())
			{
				case "type":
					isEnviron = int.Parse(ej.InnerText) == 1;
					break;
				case "id":
					ID = int.Parse(ej.InnerText);
					break;
				case "variant":
					variant = int.Parse(ej.InnerText);
					break;
				default:
					Debug.LogError("Unknown tag in key: " + ej.Name);
					break;
			}
		}
	}
}

public class Clue
{
	// Uh....
	public Clue(XmlElement e)
	{
		// :D
		Debug.LogWarning("Clues have yet to be implemented!");
	}
}
#pragma warning restore CS0618 // Type or member is obsolete