using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionDetector : MonoBehaviour
{

    public GameLock toFire;
    public bool hasFired = false;
    void OnCollisionEnter(Collision other)
    {
        //Debug.Log(other.gameObject);
        if(!hasFired && other.gameObject.CompareTag("Player"))
        {
            hasFired = true;
            toFire.GFinished(RoomManager.instance.Player.cam);
        }
    }
}
