using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
	public NetworkManagerBridge net;
	public bool swapRoles = false;
	public bool SendHi = false;
	// Start is called before the first frame update
	void Start()
	{
		int port = 25565;
		Debug.Log("Hello? How are you? " + port);
#if UNITY_EDITOR

		if (!swapRoles) { Debug.Log("Hello? How are you? Server."); net.StartServer(port, null); } else { Debug.Log("Hello? How are you? Client."); StartCoroutine(net.StartClient("127.0.0.1:" + port, null, Nothing)); }
#else
		if (swapRoles) { Debug.Log("Hello? How are you? Server."); net.StartServer(port, null); } else { Debug.Log("Hello? How are you? Client."); StartCoroutine(net.StartClient("127.0.0.1:" + port, null, Nothing)); }
#endif

		StartCoroutine(TONN());
	}
	public IEnumerator TONN()
	{
		yield return new WaitForSecondsRealtime(10f);
		net.net.AcceptingConnections = false;

	}
	void Nothing()
	{
		Debug.Log("Connected.");
	}

	// Update is called once per frame
	void Update()
	{
		if (SendHi)
		{
			SendHi = false;
			StartCoroutine(net.SendName());
		}
	}
}
