using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovementEvent : MonoBehaviour
{
    public GameLock locks;
    public MovementSet[] toPlay;

    void Start()
    {
        locks.GameFinished += Locks_GameFinished;
    }
    IEnumerator smm(MovementSet ms)
    {
        float t = Time.time;
        Vector3 position = ms.who.transform.position, scale =ms.who.transform.localScale;
        Quaternion rot = ms.who.transform.rotation;
        while(Time.time - t < ms.when)
        {
            yield return null;
            ms.who.transform.position = Vector3.Lerp(position, ms.where.position, ms.how.Evaluate((Time.time - t) / ms.when));
            ms.who.transform.localScale = Vector3.Lerp(scale, ms.where.localScale, ms.how.Evaluate((Time.time - t) / ms.when));
            ms.who.transform.rotation = Quaternion.Slerp(rot, ms.where.rotation, ms.how.Evaluate((Time.time - t) / ms.when));
        }
        ms.who.transform.position = ms.where.position;
        ms.who.transform.localScale = ms.where.localScale;
        ms.who.transform.rotation = ms.where.rotation;
    }
    private void Locks_GameFinished(CameraController cc)
    {
        foreach (MovementSet ss in toPlay)
        {
            StartCoroutine(smm(ss));
        }
    }
}
[System.Serializable]
public class MovementSet
{
    public GameObject who;
    public float when;
    public Transform where;
    public AnimationCurve how;
}
