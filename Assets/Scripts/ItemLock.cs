using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemLock : ButtonLock
{
    public string[] itemsRequired;
    public new void Start()
    {
        repeatable = false;
        base.Start();
    }
    public override bool interacted(CameraController cc)
    {
        if (finishedObj)
            return false;
        bool canPass = true;
        foreach (string s in itemsRequired)
        {
            if (!cc.playerMngr.inv.HasItem(s))
            {
                Debug.Log("Player does not have [" + s);
                canPass = false;
                break;
            }
        }
        if (canPass)
            return base.interacted(cc);
        else
            return false;
    }
}
