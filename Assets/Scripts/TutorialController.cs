using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public WallTimer wt;
    public GameObject tutorialText;
    public int time;

    // Start is called before the first frame update
    void Start()
    {
        wt.TimerTicked += Wt_TimerTicked;
    }

    private float startTime = -1;

    private void Wt_TimerTicked(float timeRem)
    {
        if (Mathf.Approximately(startTime, -1f))
        {
            startTime = timeRem;
        }

        if (startTime - timeRem > time)
        {
            tutorialText.SetActive(false);
        }
    }
}
