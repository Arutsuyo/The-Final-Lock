using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TornadoController : MonoBehaviour
{

    public AudioSource lightRain;
    public AudioSource heavyRain;
    public AudioSource wind;
    public AudioSource[] lightning;

    public WallTimer wt;
    // Start is called before the first frame update
    void Start()
    {
        wt.TimerTicked += Wt_TimerTicked;
        // First ensure all are playing just muted....
        lightRain.pitch = 1f;
        lightRain.volume = 0f;
        lightRain.mute = false;
        heavyRain.volume = 0f;
        heavyRain.pitch = 1f;
        heavyRain.mute = false;
        wind.volume = 0f;
        wind.pitch = 1f;
        wind.mute = false;
        heavyRain.Play();
        wind.Play();
        lightRain.Play();
        foreach(AudioSource ad in lightning)
        {
            ad.Stop();
            ad.volume = 1f;
            ad.pitch = 1f;
            ad.loop = false;
            ad.mute = false;
        }
    }
    private float startTime = -1;
    private void Wt_TimerTicked(float timeRem)
    {
        if(Mathf.Approximately(startTime, -1f))
        {
            startTime = timeRem;
        }

        // For 0-30%, the light rain will pick up (+vol)....
        // For 15% - 70% the wind will pick up (+ vol)
        // For 30%-50% the light rain will die off (-vol)
        // For 25% - 60% the heavy rain will pick up (+vol)
        // For 60% - 100% the wind's pitch will ramp up
        // Plus other world effects :D
        float ratio =1 - ( timeRem / startTime);
        if (ratio <= .3f)
        {
            lightRain.volume = Mathf.Lerp(0, 1f, ratio / .3f);
        }
        if(ratio <= .7f && ratio >= .15f)
        {
            wind.volume = Mathf.Lerp(0,1f, (ratio - .15f)/(.7f-.15f));
        }
        if(ratio <= .5f && ratio >= .3f)
        {
            lightRain.volume = Mathf.Lerp(1, 0f, (ratio - .3f) / (.2f));
        }
        if(ratio >= .25f && ratio <= .6f)
        {
            heavyRain.volume = Mathf.Lerp(0, .35f, (ratio - .25f) / (.35f));
        }
        if(ratio >= .6f)
        {
            wind.pitch = Mathf.Lerp(1f, 1.5f, (ratio - .6f) / (.4f));
        }

        if (ratio > .5)
        {
            int count = 0;
            foreach (AudioSource au in lightning)
            {
                count = count + (au.isPlaying ? 1 : 0);
            }
            if (count != 2)
            {
                foreach (AudioSource au in lightning)
                {
                    if ((!au.isPlaying) && Mathf.Lerp(0, .5f, (ratio - .5f) / .5f) > Random.Range(0, 5f))
                    {
                        au.pitch = Random.Range(.95f, 1.05f);
                        au.Play();
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
