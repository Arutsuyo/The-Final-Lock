using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEvent : MonoBehaviour
{
    public GameLock input;
    public AudioSource sound;
    public AudioSource altSound; // For true->false
    public bool oneshot = false;
    public bool val = false;
    // Start is called before the first frame update
    void Start()
    {
        if (oneshot)
        {
            input.GameFinished += Input_GameFinished;
        }
        else
        {
            input.GameStateSet += Input_GameStateSet;
            input.GameStateToggle += Input_GameStateToggle;
        }
    }

    private void Input_GameStateToggle(CameraController cc, int eventID)
    {
        val = !val;
        if (val)
        {
            sound.Play();
        }
        else
        {
            altSound.Play();
        }
    }

    private void Input_GameStateSet(CameraController cc, bool state, int eventID)
    {
        val = state;
        if (val)
        {
            sound.Play();
        }
        else
        {
            altSound.Play();
        }
    }

    private void Input_GameFinished(CameraController cc, int eventID)
    {
        sound.Play();
    }
    
}
