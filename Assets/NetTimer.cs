using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618 // Type or member is obsolete
public class NetTimer : NetworkBehaviour
{

    public WallTimer wallTimer;
    public float TimeAllotted = 3 * 60;
    public void StartTheTimers() { 
    
        RpcStartTimers(TimeAllotted);
        PersonalStartTimers(TimeAllotted);
    }

    public void PersonalStartTimers(float delta)
    {
        wallTimer.StartClock(delta);
    }
    [ClientRpc]
    public void RpcStartTimers(float delta)
    {
        wallTimer.StartClock(delta);
    }
}
#pragma warning restore CS0618 // Type or member is obsolete