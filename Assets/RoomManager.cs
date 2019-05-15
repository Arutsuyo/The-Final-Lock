using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
#pragma warning disable CS0618 // Type or member is obsolete
public class RoomManager : MonoBehaviour
{
    public CampaignManagerMP CMMP;
    public static RoomManager instance;
    public Dictionary<uint, Interactable> interactables = new Dictionary<uint, Interactable>();

    public void HandleRiots(NetworkMessage nm)
    {
        InteractablePacket IP = nm.ReadMessage<InteractablePacket>();
        // Basically the command without :D TODO
        if (interactables.ContainsKey((uint)IP.objectID))
        {
            interactables[(uint)IP.objectID].CmdTryPickUp(IP.playerRequesting);
        }
        else
        {
            Debug.Log("ID: " + IP.objectID + " WAS NOT PRESENT/REGISTERED!");
        }
    }
    public void HandleFin(NetworkMessage nm)
    {
        InteractablePacket IP = nm.ReadMessage<InteractablePacket>();
        if (interactables.ContainsKey((uint)IP.objectID))
        {
            interactables[(uint)IP.objectID].CmdServerFinished(IP.playerRequesting);
        }
        else
        {
            Debug.Log("ID: " + IP.objectID + " WAS NOT PRESENT/REGISTERED!");
        }
    }
    public void Start()
    {
        instance = this;
        GameObject CMMP1 = GameObject.FindGameObjectWithTag("CMMP");
        if(CMMP1 == null)
        {
            Debug.Log("Unable to find the manager!");
        }
        else
        {
            CMMP = CMMP1.GetComponent<CampaignManagerMP>();
            CMMP.nm.net.RegisterHandler(MPMsgTypes.Interactions, HandleRiots);
            CMMP.nm.net.RegisterHandler(MPMsgTypes.FinInteractions, HandleFin);
        }

    }
}
#pragma warning restore CS0618 // Type or member is obsolete