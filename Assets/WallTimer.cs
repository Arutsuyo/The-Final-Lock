using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallTimer : MonoBehaviour
{
    public float TimeToCountTo;
    public delegate void SimpleDelegate();
    public event SimpleDelegate TimerExpired;
    public NumberController DIG0;
    public NumberController DIG1;
    public NumberController DIG2;
    public NumberController DIG3;
    public float TimeToRunFor = 0;
    public float TimeSpeed = 1;
    public bool Debug = false;
    public bool Debug2 = false;
    public void StartClock(float timeToRun)
    {
        TimeToCountTo = timeToRun + Time.time;
        StopAllCoroutines();
        StartCoroutine(ClockTimer());
    }
    public void Update()
    {
        if (Debug)
        {
            Debug = false;
            StartClock(TimeToRunFor);
        }
        if (Debug2)
        {
            Debug2 = false;
            Time.timeScale = TimeSpeed;
        }
    }

    private IEnumerator ClockTimer()
    {
        int LHS = 0;
        int RHS = 0;
        while (TimeToCountTo - Time.time > 0)
        {
            float ff = TimeToCountTo - Time.time;
            if (ff < 60.0f)
            {
                // Less than 1 minute... (show seconds and millis)
                LHS = Mathf.FloorToInt(ff);
                RHS = Mathf.FloorToInt((ff - LHS) * 100);
            }
            else if(ff < 3600.0f)
            {
                // Less than 1 hour (show minutes and seconds)
                LHS = Mathf.FloorToInt(ff / 60.0f);
                RHS = Mathf.FloorToInt((ff - LHS*60));
            }
            else
            {
                // Greater than 1 hour (show hours and minutes)
                LHS = Mathf.FloorToInt(ff / 3600.0f);
                RHS = Mathf.FloorToInt((ff - LHS*3600)/60.0f);
            }
            DIG0.NUM = Mathf.FloorToInt(LHS / 10.0f);
            DIG1.NUM = Mathf.FloorToInt((LHS - 10 * DIG0.NUM));
            DIG2.NUM = Mathf.FloorToInt(RHS / 10.0f);
            DIG3.NUM = Mathf.FloorToInt((RHS - 10 * DIG2.NUM));
            DIG0.ChangeUpdate();
            DIG1.ChangeUpdate();
            DIG2.ChangeUpdate();
            DIG3.ChangeUpdate();
            yield return null;
        }
        DIG0.NUM = 0;
        DIG1.NUM = 0;
        DIG2.NUM = 0;
        DIG3.NUM = 0;
        DIG0.ChangeUpdate();
        DIG1.ChangeUpdate();
        DIG2.ChangeUpdate();
        DIG3.ChangeUpdate();
        // Set it equal to 0...
    }

}
