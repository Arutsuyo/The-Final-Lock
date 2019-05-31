using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAnimationEvent : MonoBehaviour
{
    public GameLock locks;
    public AnimationSet[] toPlay;

    void Start()
    {
        locks.GameFinished += Locks_GameFinished;
    }
    private void Locks_GameFinished(CameraController cc)
    {
        foreach(AnimationSet ss in toPlay)
        {
            ss.anim.Play(ss.animationStateName);// :D
        }
    }
}
[System.Serializable]
public class AnimationSet
{
    public Animator anim;
    public string animationStateName;
}
