﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallTimer : MonoBehaviour
{
    public float TimeToCountTo;
    public delegate void SimpleDelegate();
    public event SimpleDelegate TimerExpired;
    public event SimpleDelegate TimerPaused;

    public delegate void TimeDelegate(float timeRem);
    public event TimeDelegate TimerTicked = delegate { };

    public bool Debug = false;
    public bool Debug2 = false;
    public bool isStopped = false;
    
    public void StartClock(float timeToRun)
    {
        TimeToCountTo = timeToRun + Time.time;
        StopAllCoroutines();
        StartCoroutine(ClockTimer());
    }
    public void StopTimer()
    {
        if (!isStopped)
        {
            StopAllCoroutines();
            isStopped = true;
            TimerPaused();
            
        }
    }

    private IEnumerator ClockTimer()
    {
        int prevTimeInt = Mathf.CeilToInt(TimeToCountTo - Time.time);
        while(TimeToCountTo - Time.time > 0)
        {
            TimerTicked(TimeToCountTo - Time.time);
            yield return new WaitForSeconds(0.05f);
        }
        TimerTicked(0);
        TimerExpired();
    }
    

}
