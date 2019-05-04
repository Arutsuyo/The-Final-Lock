﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public Interactable inter;
    public string ItemName;
    void Start()
    {
        inter.lookEvent += LookAt;
        inter.interactEvent += InterAt;
    }

    private void LookAt(CameraController cc)
    {
        // Display a tooltip...
        
    }
    private void InterAt(CameraController cc)
    {
        if (cc.playerMngr.inv.items.Count < cc.playerMngr.inv.maxItems)
        {
            // Add it :D
            cc.playerMngr.inv.PickUpItem(this);
        }
    }

}