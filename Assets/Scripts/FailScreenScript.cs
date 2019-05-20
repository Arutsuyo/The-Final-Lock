using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FailScreenScript : MonoBehaviour
{
    public string[] endings;

    public delegate void SimpleDelegate();
    public event SimpleDelegate Finished = delegate { };

    public Text text;
    public Color targColor;
    public Color compColor;
    public float LerpTime = 2f;
    public float WaitTime = 5f;
    public float StartTime = 0f;
    protected bool started = false;
    public void StartFadeIn()
    {
        if (endings.Length == 0) {
            text.text = "Your time is up.";
        }
        else
        {
            text.text = endings[Random.Range(0, endings.Length)];
        }
        
        StartCoroutine(LerpColors());
    }

    IEnumerator LerpColors()
    {
        if (started)
        {
            yield break;
        }

        started = true;
        targColor = text.color;
        compColor = new Color();
        compColor.r = targColor.r;
        compColor.g = targColor.g;
        compColor.b = targColor.b;
        compColor.a = 0;
        text.color = compColor;

        if (StartTime != 0)
        {
            yield return new WaitForSeconds(StartTime);
        }
        float ST = Time.time;
        while(Time.time - ST < LerpTime)
        {
            text.color = Color.Lerp(compColor, targColor, (Time.time - ST)/LerpTime);
            yield return null;
            
        }
        text.color = targColor;
        yield return new WaitForSeconds(WaitTime);
        Finished();
    }
}
