using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloodController : MonoBehaviour
{
    public WallTimer wt;
    public GameObject water;
    public GameObject fire;
    public float waterLevel;

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

        float ratio = 1 - (timeRem / startTime);

        water.transform.position = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(0, waterLevel, 0), ratio);

        if (water.transform.position.y > fire.transform.position.y)
            fire.transform.position = new Vector3(fire.transform.position.x, water.transform.position.y, fire.transform.position.z);
    }
}
