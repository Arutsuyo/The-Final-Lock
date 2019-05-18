using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionScript : MonoBehaviour
{
    public AudioSource aud;
    public WallTimer wt;
    // Start is called before the first frame update
    void Start()
    {
        wt.TimerExpired += PlayExpl;
    }
    public void PlayExpl()
    {
        aud.Play();
    }
}
