using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
	public Interactable inter;
	public CameraController prevCC;
	public string ItemName;
	void Start()
	{
		inter.lookEvent += LookAt;
		inter.interactEvent += InterAt;
		inter.escapeInteractEvent += Return;
        inter.interactDestroyed += DestroyFromNetwork;
    }



    public void DestroyFromNetwork()
    {

        Debug.Log(RoomManager.instance);
        Debug.Log(RoomManager.instance.Player);
        Debug.Log(RoomManager.instance.Player.cam);

        Debug.Log(RoomManager.instance.Player.cam.playerMngr);
        Debug.Log(RoomManager.instance.Player.cam.playerMngr.inv);
        RoomManager.instance.Player.cam.playerMngr.inv.SilentDelete(this);
    }
    public void AskDestroy()
    {
        inter.DestroyFromNetwork(inter.owner);
    }

	private void LookAt(CameraController cc)
	{
		// Display a tooltip...
	}

	private void Return()
	{
        gameObject.SetActive(false);
        /// Actually add it to inventory...
        RoomManager.instance.Player.cam.playerMngr.inv.PickUpItem(this);
	}

	private bool InterAt(CameraController cc)
	{
		if (cc.playerMngr.inv.items.Count < cc.playerMngr.inv.maxItems)
		{
			// Add it :D
			cc.playerMngr.inv.PickUpItem(this);
			prevCC = cc;
			return true;
		}
		return false;
	}
}
