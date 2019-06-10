using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class speedingTimer : MonoBehaviour
{
    public GameObject toMove;
    public GameObject from;
    public GameObject to;

    public float startTime = -1;
    public WallTimer wt;
    public AnimationCurve ac;
    public void Start()
    {
        wt.TimerTicked += Wt_TimerTicked;
    }

    private void Wt_TimerTicked(float timeRem)
    {
        if(startTime < 0)
        {
            startTime = timeRem;
        }
        toMove.transform.position = Vector3.Lerp(to.transform.position, from.transform.position, ac.Evaluate(timeRem / startTime));
    }
}
