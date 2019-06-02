using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TimerScript : MonoBehaviour
{
    public WallTimer WT;

    public void Start()
    {
        WT.TimerTicked += Ticked;
        WT.TimerPaused += Stopped;
    }

    public abstract void Stopped();
    public abstract void Ticked(float timeRem);
}