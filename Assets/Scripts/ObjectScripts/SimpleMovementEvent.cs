using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovementEvent : MonoBehaviour
{
    public GameLock locks;
    public MovementSet[] toPlay;
    public bool toggle = false;

    void Start()
    {
        locks.GameFinished += Locks_GameFinished;
        locks.GameStateSet += Locks_Set;
        locks.GameStateToggle += Locks_Toggle;
    }
    IEnumerator smm(MovementSet ms, bool dir)
    {
        float t = Time.time;
        Transform a;
        Transform b;
        if (dir)
        {
            a = ms.from;
            b = ms.to;
        }
        else
        {
            b = ms.from;
            a = ms.to;
        }

        Vector3 position = a.position, scale =a.localScale;
        Quaternion rot = a.rotation;
        while(Time.time - t < ms.when)
        {
            yield return null;
            ms.who.transform.position = Vector3.Lerp(position, b.position, ms.how.Evaluate((Time.time - t) / ms.when));
            ms.who.transform.localScale = Vector3.Lerp(scale, b.localScale, ms.how.Evaluate((Time.time - t) / ms.when));
            ms.who.transform.rotation = Quaternion.Slerp(rot, b.rotation, ms.how.Evaluate((Time.time - t) / ms.when));
        }
        ms.who.transform.position = b.position;
        ms.who.transform.localScale = b.localScale;
        ms.who.transform.rotation = b.rotation;
    }
    private void Locks_GameFinished(CameraController cc)
    {
        StopAllCoroutines();
        foreach (MovementSet ss in toPlay)
        {
            StartCoroutine(smm(ss, true));
        }
    }
    private void Locks_Set(CameraController cc, bool state)
    {
        if(toggle == state)
        {
            return;
        }
        StopAllCoroutines();
        foreach (MovementSet ss in toPlay)
        {
            StartCoroutine(smm(ss, state));
        }
    }
    private void Locks_Toggle(CameraController cc)
    {
        toggle = !toggle;
        StopAllCoroutines();
        foreach (MovementSet ss in toPlay)
        {
            StartCoroutine(smm(ss, toggle));
        }
    }

}
[System.Serializable]
public class MovementSet
{
    public GameObject who;
    public float when;
    public Transform to;
    public Transform from;
    public AnimationCurve how;
}
