using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaisingTimer : TimerScript
{

    public GameObject toRaise;
    public Transform raiseTo;
    public Transform raiseFrom;
    public override void Stopped()
    {
        StartCoroutine(SimpleDrop());
    }
    public IEnumerator SimpleDrop()
    {
        float st = Time.time + 1f;
        Vector3 pp = toRaise.transform.position;
        while (st - Time.time >= 0)
        {
            toRaise.transform.position = Vector3.Lerp(raiseFrom.position, pp, st - Time.time);
            yield return null;
        }
    }

    [HideInInspector] public float startTime = -1;
    [HideInInspector] public float origSpeed = -1;
    public override void Ticked(float timeRem)
    {
        if(Mathf.Approximately(startTime, -1f))
        {
            startTime = timeRem;
            origSpeed = RoomManager.instance.Player.moveSpeed;
        }

        toRaise.transform.position = Vector3.Lerp(raiseTo.position, raiseFrom.position, timeRem / startTime);
        RoomManager.instance.Player.moveSpeed = Mathf.Lerp(origSpeed / 5f, origSpeed, timeRem / startTime);
    }

}
