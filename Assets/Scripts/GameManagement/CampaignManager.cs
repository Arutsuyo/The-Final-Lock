using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

using System.Xml;
using System;

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
                    {
                        eerooms.Add(e);
                    }
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
    public override void Go(int id)
    {
        foreach (Button bb in Campaigns)
        {
            bb.interactable = false;
        }
        //pm.GetNumber(GetPort, 1000, 65535, "Please enter the port for hosting:", "" + 25565);
        ActiveID = id;

        Port = -1;
        // Now actually launch it.
        Debug.Log("Launching campaign ID " + ActiveID + " in 3...2....1...JUMP");
        PlayerAnimation.SetTrigger("GoToRoom");
        doorAnimation.SetTrigger("Door");
        holoAnimation.SetTrigger("ToDoor");
        // Swap rooms....
        // Actually, destroy the OTHER campaign manager :3
        // the other will destroy this one...
        Destroy(otherContestant);
    }
    public override void UpdateCampaignUI(int id)
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
}