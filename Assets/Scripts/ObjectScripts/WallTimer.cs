using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallTimer : MonoBehaviour
{
    public float TimeToCountTo;
    public delegate void SimpleDelegate();
    public event SimpleDelegate TimerExpired;
    public event SimpleDelegate TimerPaused;

    public delegate void TimeDelegate(int timeRem);
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
            int id = Mathf.CeilToInt(TimeToCountTo - Time.time);
            if (prevTimeInt != id)
            {
                prevTimeInt = id;
                TimerTicked(id);
            }
            yield return new WaitForSeconds(0.25f);
        }
        TimerTicked(0);
        TimerExpired();
    }
    

}
