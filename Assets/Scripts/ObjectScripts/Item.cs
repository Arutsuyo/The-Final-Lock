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
    }

    private void LookAt(CameraController cc)
    {
        // Display a tooltip...
        
    }
    private void Return()
    {
        if (prevCC != null)
        {
            prevCC.playerMngr.inv.SilentDelete(this);
            // Already deleted :P
        }
        else
        {
            // Silently disable this...
            gameObject.SetActive(false);
        }
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
