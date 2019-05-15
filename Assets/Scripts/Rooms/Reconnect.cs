using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reconnect : MonoBehaviour
{
    public delegate void ReconnectHandles(Interactable attached);

    public event ReconnectHandles OnSubPartNetworkSpawned;

    public void AttemptReconnect(Interactable a)
    {
        if (OnSubPartNetworkSpawned != null)
        {
            OnSubPartNetworkSpawned(a);
        }
    }

}
