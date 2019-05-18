using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class MylifeRandom
{
	public static double NextGaussianDouble()
	{
		double u, v, S;
		do
		{
			u = 2.0 * Random.Range(0, 1f) - 1.0;
			v = 2.0 * Random.Range(0, 1f) - 1.0;
			S = u * u + v * v;
		}
		while (S >= 1.0);

		double fac = Mathf.Sqrt((float)(-2.0 * Mathf.Log((float)S) / S));
		return u * fac;
	}

	public static double NormalizedRandom(double minValue, double maxValue)
	{
		double mean = (minValue + maxValue) / 2;
		double sigma = (maxValue - mean) / 3;
		return NextGaussianDouble() * sigma + mean;
	}
}
public class FakeNetworkManager : NetworkManagerBridge
{
	/*
	 * This object is to simulate a multiplayer environment by providing
	 * "faked" networking in a singleplayer environment. For sakes of consistency,
	 * we will have single player implement all MP aspects with this class.
	 * 
	 */
	[Header("Faked Network Settings")]
	public float NetworkDelay = 0.1f;
	public float NetworkGuassDelay = 0.2f;
	public bool IsClientSide = false;
	Dictionary<int, PQueue<TNETMSG>> messagesDict = new Dictionary<int, PQueue<TNETMSG>>();
	public new bool IsClient()
	{
		return IsClientSide;
	}
	public bool SendMessageToServer(UserNETMSG nm, int channel)
	{
		TNETMSG mmg = new TNETMSG() { msg = nm, time = (float)MylifeRandom.NormalizedRandom(NetworkDelay, NetworkGuassDelay * 2 + NetworkDelay) };
		messagesDict[channel].Enqueue(mmg, -mmg.time);
		return true;
	}

	public bool SendMessageToClient(UserNETMSG nm, int channel, long UUID)
	{
		if (!IsClientSide)
		{
			return false;
		}
		TNETMSG mmg = new TNETMSG() { msg = nm, time = (float)MylifeRandom.NormalizedRandom(NetworkDelay, NetworkGuassDelay * 2 + NetworkDelay) };
		messagesDict[channel].Enqueue(mmg, -mmg.time);
		return true;
	}

	public bool SendMessageToAllClients(UserNETMSG nm, int channel)
	{
		if (!IsClientSide)
		{
			return false;
		}
		TNETMSG mmg = new TNETMSG() { msg = nm, time = (float)MylifeRandom.NormalizedRandom(NetworkDelay, NetworkGuassDelay * 2 + NetworkDelay) };
		messagesDict[channel].Enqueue(mmg, -mmg.time);
		return true;
	}
	public bool SendMessageToAllOtherClients(UserNETMSG nm, int channel, long exceptUUID)
	{
		if (!IsClientSide)
		{
			return false;
		}
		TNETMSG mmg = new TNETMSG() { msg = nm, time = (float)MylifeRandom.NormalizedRandom(NetworkDelay, NetworkGuassDelay * 2 + NetworkDelay) };
		messagesDict[channel].Enqueue(mmg, -mmg.time);
		return true;
	}

	public new void Start()
	{
		if (instance != null)
		{
			Destroy(this.gameObject);
			return;
		}
		instance = this;
		DontDestroyOnLoad(gameObject);
		NetChannels.Add(0, UnityEngine.Networking.QosType.Reliable);
		NetChannels.Add(Networker.AddChannel(UnityEngine.Networking.QosType.ReliableStateUpdate), UnityEngine.Networking.QosType.ReliableStateUpdate);
		NetChannels.Add(Networker.AddChannel(UnityEngine.Networking.QosType.StateUpdate), UnityEngine.Networking.QosType.StateUpdate);
		NetChannels.Add(Networker.AddChannel(UnityEngine.Networking.QosType.ReliableSequenced), UnityEngine.Networking.QosType.ReliableSequenced);
		foreach (int k in NetChannels.Keys)
		{
			messagesDict.Add(k, new PQueue<TNETMSG>());
		}
		// Technically need to add that :P
	}
	public void Update()
	{
		// Simulate the network here :)
		foreach (int k in messagesDict.Keys)
		{
			switch (NetChannels[k])
			{
				case UnityEngine.Networking.QosType.Reliable: // Reliable, but may appear out of order. (All messages sent in are randomized on time)
					{
						TNETMSG ss = messagesDict[k].Peek();
						ArbObj obj = new ArbObj() { id = ss.msg.OP, uuid = -1, msg = (UserNETMSG)ss.msg };
						while (ss != null && ss.time < Time.time)
						{
							// Process the messages :D
							HandleNetMsg(ss.msg.OP, ((UserNETMSG)obj.msg).subID, obj);
							messagesDict[k].Dequeue();
							ss = messagesDict[k].Peek();
						}
					}
					break;
				case UnityEngine.Networking.QosType.ReliableStateUpdate:
					{
						TNETMSG ss = messagesDict[k].Peek();
						ArbObj obj = new ArbObj() { id = ss.msg.OP, uuid = -1, msg = (UserNETMSG)ss.msg };
						while (ss != null && ss.time < Time.time)
						{
							// Process the messages :D
							HandleNetMsg(ss.msg.OP, ((UserNETMSG)obj.msg).subID, obj);
							messagesDict[k].Dequeue();
							ss = messagesDict[k].Peek();
						}
					}
					break;
				case UnityEngine.Networking.QosType.StateUpdate:
					{
						TNETMSG ss = messagesDict[k].Peek();
						ArbObj obj = new ArbObj() { id = ss.msg.OP, uuid = -1, msg = (UserNETMSG)ss.msg };
						while (ss != null && ss.time < Time.time)
						{
							// Process the messages :D
							HandleNetMsg(ss.msg.OP, ((UserNETMSG)obj.msg).subID, obj);
							messagesDict[k].Dequeue();
							ss = messagesDict[k].Peek();
						}
					}
					break;
				case UnityEngine.Networking.QosType.ReliableSequenced:
					{
						TNETMSG ss = messagesDict[k].Peek();
						ArbObj obj = new ArbObj() { id = ss.msg.OP, uuid = -1, msg = (UserNETMSG)ss.msg };
						while (ss != null && ss.time < Time.time)
						{
							// Process the messages :D
							HandleNetMsg(ss.msg.OP, ((UserNETMSG)obj.msg).subID, obj);
							messagesDict[k].Dequeue();
							ss = messagesDict[k].Peek();
						}
					}
					break;
				case UnityEngine.Networking.QosType.UnreliableFragmentedSequenced:
					// Technically not used normally, but it is used in the local multiplayer.
					{
						TNETMSG ss = messagesDict[k].Peek();

						while (ss != null && ss.time < Time.time)
						{
							// Does stuff :)
						}
					}
					break;
				default:
					Debug.Log("Unsupported message type!");
					break;
			}
		}
	}

	public new void StartServer(int Port, CampaignManagerMP CMMP)
	{
		CLPS = CMMP;

		this.SocketInfo = new IPAP("127.0.0.1:" + Port);

	}
}
class TNETMSG
{
	public NETMSG msg;
	public float time;
}

public class PQueue<T>
{
	// Implements a > sorting, so higher priority is first.
	private class ST
	{
		public T t;
		public double sorting;
	}
	private LinkedList<ST> q = new LinkedList<ST>();
	public void Enqueue(T obj, double priority)
	{
		LinkedListNode<ST> n = q.First;
		while (n != null && n.Value.sorting < priority)
		{
			n = n.Next;
		}
		if (n == null)
		{
			q.AddLast(new ST() { t = obj, sorting = priority });
		}
		else
		{
			q.AddBefore(n, new ST() { t = obj, sorting = priority });
		}
	}
	public T Dequeue()
	{
		if (!isEmpty())
		{
			ST ss = q.Last.Value;
			q.RemoveLast();
			return ss.t;
		}
		else
		{
			return default(T); // Empty!
		}
	}
	public T Peek()
	{
		if (!isEmpty())
		{
			ST ss = q.Last.Value;
			return ss.t;
		}
		else
		{
			return default(T); // Empty!
		}
	}
	public bool isEmpty()
	{
		return !q.Any();
	}
}
