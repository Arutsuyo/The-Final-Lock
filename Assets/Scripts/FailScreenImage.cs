using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FailScreenImage : FailScreenScript
{
    private readonly new Text text;
    public Image image;
    public new event SimpleDelegate Finished = delegate { };
    public new void StartFadeIn()
    {
        targColor = image.color;
        compColor = targColor;
        compColor.a = 0;
        image.color=compColor;
        StartCoroutine(LerpColors());
    }

    IEnumerator LerpColors()
    {
        
        yield return new WaitForSeconds(StartTime);
        float ST = Time.time;
        while (Time.time - ST < LerpTime)
        {
            image.color = Color.Lerp(compColor, targColor, (Time.time - ST) / LerpTime);
            yield return null;

        }
        image.color = targColor;
        yield return new WaitForSeconds(WaitTime);
        Finished();
    }
}
