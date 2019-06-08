using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TutorialController : MonoBehaviour
{
    public WallTimer wt;
    public GameObject tutorialText;
    public Text tutorial;
    public int time;

    // Start is called before the first frame update
    void Start()
    {
        wt.TimerTicked += Wt_TimerTicked;
    }

    private float startTime = -1;
    private Color cc = new Color();
    private void Wt_TimerTicked(float timeRem)
    {
        if (Mathf.Approximately(startTime, -1f))
        {
            startTime = timeRem;
            cc = tutorial.color;
        }
        tutorial.color = Color.Lerp(new Color(0, 0, 0, 0), cc, (startTime - timeRem) / ((float)time));
        if (startTime - timeRem > time)
        {
            tutorialText.SetActive(false);
        }
    }
}
