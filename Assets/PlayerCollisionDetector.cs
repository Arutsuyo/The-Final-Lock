using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionDetector : MonoBehaviour
{

    public GameLock toFire;
    public bool hasFired = false;
    void OnCollisionEnter(Collision other)
    {
        if(!hasFired && other.transform.name.Equals("Player Model"))
        {
            hasFired = true;
            toFire.GFinished(RoomManager.instance.Player.cam);
        }
    }
}
