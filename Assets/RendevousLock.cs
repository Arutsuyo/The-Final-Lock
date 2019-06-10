using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RendevousLock : MonoBehaviour
{
    public GameLock[] locks;

    public GameLock output;
    [HideInInspector] public bool[] done;
    public void Start()
    {
        done = new bool[locks.Length];

        for(int i = 0; i < locks.Length; i++)
        {
            int k = i;
            locks[i].GameFinished += ((x, y) => GE(x, y, k));
        }
    }

    public void GE(CameraController c, int eID, int LP)
    {
        done[LP] = true;
        foreach(bool b in done)
        {
            if (!b)
            {
                return;
            }
        }
        output.GFinished(c);
    }


}
