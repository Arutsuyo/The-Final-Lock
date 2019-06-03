using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAnimationEvent : MonoBehaviour
{
    public GameLock locks;
    public AnimationSet[] toPlay;
    public AnimationSet[] toPlayBackwards;
    public bool toggle = false;

    void Start()
    {
        locks.GameFinished += Locks_GameFinished;
        locks.GameStateSet += Locks_Set;
        locks.GameStateToggle += Locks_Toggle;
    }
    private void Locks_GameFinished(CameraController cc, int _)
    {
        foreach(AnimationSet ss in toPlay)
        {
            ss.anim.Play(ss.animationStateName);// :D
        }
    }
    private void Locks_Set(CameraController cc, bool state, int _)
    {
        if (toggle == state)
        {
            return;
        }
        foreach (AnimationSet ss in (state ? toPlay : toPlayBackwards))
        {
            ss.anim.Play(ss.animationStateName);
        }
    }
    private void Locks_Toggle(CameraController cc, int _)
    {
        toggle = !toggle;
        foreach (AnimationSet ss in (toggle ? toPlay : toPlayBackwards))
        {
            ss.anim.Play(ss.animationStateName);
        }
    }
    
}
[System.Serializable]
public class AnimationSet
{
    public Animator anim;
    public string animationStateName;
}
