using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

using System.Xml;
using System;
using UnityEngine.SceneManagement;

public class CampaignManager : CampaignManagerMP
{
	override public void QueryRooms()
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
					if (e.isSP)
						eerooms.Add(e);
				}
			}
			catch (Exception e)
			{
				Debug.LogError("Malformed escape room file! " + e.StackTrace);
			}
		}
		erooms = eerooms.ToArray<EscapeRoom>();

	}
    public new void Start()
    {
        HideUI();
        DontDestroyOnLoad(this.gameObject);
    }
    public override void Go(int id)
	{
		foreach (Button bb in Campaigns)
			bb.interactable = false;

		//pm.GetNumber(GetPort, 1000, 65535, "Please enter the port for hosting:", "" + 25565);
		ActiveID = id;

		Port = -1;
        instance = this;
        // Now actually launch it.
        Debug.Log("Launching campaign ID " + ActiveID + " in 3...2....1...JUMP");
		PlayerAnimation.SetTrigger("GoToRoom");
		doorAnimation.SetTrigger("Door");
		holoAnimation.SetTrigger("ToDoor");
		// Swap rooms....
		// Actually, destroy the OTHER campaign manager :3
		// the other will destroy this one...
		Destroy(otherContestant.gameObject);
        nm.StartSPServer(25565, this);
        nm.net.ClientSceneChanged += this.SendServerReady;
        // But actually you are hosting a single player world :D
        StartCoroutine(TransferRooms());
	}

    public IEnumerator TransferRooms()
    {
        yield return new WaitForSecondsRealtime(9.37f/.8f);
        //AOP = SceneManager.LoadSceneAsync(1);
        if (nm.net.isHost)
        {
            // Send all clients the room details
            // Send all clients the room details
            Scene ss = SceneManager.GetSceneByBuildIndex(erooms[ActiveID].roomID);

            nm.net.ServerChangeScene(ss.name);


        }
    }

	public override void UpdateCampaignUI(int id)
	{
		//Debug.Log("Clicked campaign " + id);
		if (holo != null)
			Destroy(holo);

		holo = Instantiate(holoDeck[Array.FindIndex(holoDeckNames, w => w.Equals(erooms[id].holoname))], holoStorage.transform);
		Debug.Log(erooms[id].holoname + " " + Array.FindIndex(holoDeckNames, w => w.Equals(erooms[id].holoname)));
		StartBtn.gameObject.SetActive(true);
		StartBtn.interactable = true;
		StartBtn.onClick.RemoveAllListeners();
		StartBtn.onClick.AddListener(() => Go(id));
	}
}