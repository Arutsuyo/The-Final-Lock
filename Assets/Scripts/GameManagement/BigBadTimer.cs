using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBadTimer : TimerScript
{
    public NumberController DIG0;
    public NumberController DIG1;
    public NumberController DIG2;
    public NumberController DIG3;
    public float TimeToRunFor = 0;
    public float TimeSpeed = 1;
    public AudioSource AUS;
    public AudioSource Tick;
    public bool isStopped = false;
    public float TimeToCountTo;
    public float prevTimeRem = 0;
    public override void Ticked(int timeRem)
    {
        Tick.Play();
        if(timeRem <= 60f && prevTimeRem > 60f)
            AUS.Play();
        int LHS;
        int RHS;
        if (timeRem < 60.0f)
        {
            // Less than 1 minute... (show seconds and millis)
            AUS.volume = Mathf.Min(1, ((60.0f - timeRem) / 30.0f));
            Tick.volume = Mathf.Max(0, (timeRem - 45.0f) / 15f);
            LHS = Mathf.FloorToInt(timeRem);
            RHS = Mathf.FloorToInt((timeRem - LHS) * 100);
        }
        else if (timeRem < 3600.0f)
        {
            // Less than 1 hour (show minutes and seconds)
            LHS = Mathf.FloorToInt(timeRem / 60.0f);
            RHS = Mathf.FloorToInt((timeRem - LHS * 60));
        }
        else
        {
            // Greater than 1 hour (show hours and minutes)
            LHS = Mathf.FloorToInt(timeRem / 3600.0f);
            RHS = Mathf.FloorToInt((timeRem - LHS * 3600) / 60.0f);
        }

        DIG0.NUM = Mathf.FloorToInt(LHS / 10.0f);
        DIG1.NUM = Mathf.FloorToInt((LHS - 10 * DIG0.NUM));
        DIG2.NUM = Mathf.FloorToInt(RHS / 10.0f);
        DIG3.NUM = Mathf.FloorToInt((RHS - 10 * DIG2.NUM));
        DIG0.ChangeUpdate();
        DIG1.ChangeUpdate();
        DIG2.ChangeUpdate();
        DIG3.ChangeUpdate();
    }
    public override void Stopped()
    {
        if (!isStopped)
        {
            AUS.Stop();
            Tick.Stop();
            isStopped = true;
            StartCoroutine(ClockFlash());
        }
    }


    private IEnumerator ClockFlash()
    {
        float ff = TimeToCountTo - Time.time;
        int LHS = 0;
        int RHS = 0;

        if (ff < 60.0f)
        {
            // Less than 1 minute... (show seconds and millis)
            LHS = Mathf.FloorToInt(ff);
            RHS = Mathf.FloorToInt((ff - LHS) * 100);
        }
        else if (ff < 3600.0f)
        {
            // Less than 1 hour (show minutes and seconds)
            LHS = Mathf.FloorToInt(ff / 60.0f);
            RHS = Mathf.FloorToInt((ff - LHS * 60));
        }
        else
        {
            // Greater than 1 hour (show hours and minutes)
            LHS = Mathf.FloorToInt(ff / 3600.0f);
            RHS = Mathf.FloorToInt((ff - LHS * 3600) / 60.0f);
        }

        DIG0.NUM = Mathf.FloorToInt(LHS / 10.0f);
        DIG1.NUM = Mathf.FloorToInt((LHS - 10 * DIG0.NUM));
        DIG2.NUM = Mathf.FloorToInt(RHS / 10.0f);
        DIG3.NUM = Mathf.FloorToInt((RHS - 10 * DIG2.NUM));
        int D0 = DIG0.NUM;
        int D1 = DIG1.NUM;
        int D2 = DIG2.NUM;
        int D3 = DIG3.NUM;
        while (isStopped)
        {
            DIG0.NUM = -1;
            DIG1.NUM = -1;
            DIG2.NUM = -1;
            DIG3.NUM = -1;
            DIG0.ChangeUpdate();
            DIG1.ChangeUpdate();
            DIG2.ChangeUpdate();
            DIG3.ChangeUpdate();

            yield return new WaitForSeconds(0.4f);
            DIG0.NUM = D0;
            DIG1.NUM = D1;
            DIG2.NUM = D2;
            DIG3.NUM = D3;
            DIG0.ChangeUpdate();
            DIG1.ChangeUpdate();
            DIG2.ChangeUpdate();
            DIG3.ChangeUpdate();

            yield return new WaitForSeconds(0.5f);
        }

    }
}
