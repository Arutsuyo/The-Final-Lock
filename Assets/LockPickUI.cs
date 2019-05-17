using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockPickUI : MonoBehaviour
{
	public int numPins = 8;
	public bool Debug = false;
	public bool Debug1 = false;

	public float moveX = 0;
	public int pickPos = 1;
	private int prevPickPos = 1;
    public RectTransform canvas;
	public RectTransform[] pins;
	public RectTransform[] pinHeads;
	private Vector2[] originalPP;
	private Vector2[] originalPHP;
	public bool[] heldHead;
	private float[] heldHeadPos;
	public float yResting;

    public Image ERR;
    public Image WARN;
    public Image FAIL;
    public Image SUCC;
	public RectTransform pick;
	public RectTransform pusher;
	private Vector2 pusherSizeDelta;
	private Vector2 pusherPosition;
	public float HeightMP = 0.3f;
	private float prevHeight = 0;
	public float height = 0; // Range from 0 to 0.3...but we will do the modification for 0 - 100%

	public void Start()
	{
		// Move the first one up by the yResting
		originalPHP = new Vector2[pins.Length];
		originalPP = new Vector2[pins.Length];
		heldHeadPos = new float[pins.Length];
		pusherSizeDelta = pusher.sizeDelta;
		pusherPosition = pusher.anchoredPosition;
		for (int i = 0; i < pins.Length; i++)
		{
			heldHeadPos[i] = 0;
			originalPHP[i] = pinHeads[i].anchoredPosition;
			originalPP[i] = pins[i].anchoredPosition;
		}
		pins[0].anchoredPosition += new Vector2(0, yResting);
		pinHeads[0].anchoredPosition += new Vector2(0, yResting);

	}
	public void Update()
	{
		if (Debug)
		{
			Debug = false;
			UpdatePins();
		}
		if (Debug1)
		{
			Debug1 = false;
			int temp = pickPos;
			pickPos = 1;
			UpdatePins();
			pickPos = temp;
		}
	}

    public void SpawnEXC()
    {
        // Spawn a ! where the pin is...
        Image fo = Instantiate<Image>(ERR, SUCC.transform.parent);
        fo.rectTransform.localPosition = ERR.rectTransform.localPosition;
        fo.rectTransform.SetParent(canvas, true);
        fo.gameObject.SetActive(true);
        StartCoroutine(DeleteMe(fo));
    }
    public void SpawnGood()
    {
        // Spawn a check where the pin is...
        Image fo = Instantiate<Image>(SUCC,SUCC.transform.parent);
        fo.rectTransform.localPosition = SUCC.rectTransform.localPosition;
        fo.rectTransform.SetParent(canvas, true);
        fo.gameObject.SetActive(true);
        StartCoroutine(DeleteMe(fo));
    }
    public void SpawnBad()
    {
        // Spawn a check where the pin is...
        Image fo = Instantiate<Image>(FAIL, SUCC.transform.parent);
        fo.rectTransform.localPosition = FAIL.rectTransform.localPosition;
        fo.rectTransform.SetParent(canvas, true);
        fo.gameObject.SetActive(true);
        StartCoroutine(DeleteMe(fo));
    }
    public void SetWarn(bool yes)
    {
        // Spawn a check where the pin is...
        WARN.gameObject.SetActive(yes);
    }


    IEnumerator DeleteMe(Image fo)
    {
        float ttt = Time.time;
        Color aa = fo.color;
        while(Time.time - ttt < 2)
        {
            aa.a = Mathf.Min(1f, 2 - (Time.time - ttt));
            fo.color = aa;
            yield return null;
        }
        Destroy(fo.gameObject);
    }

	public void UpdatePins()
	{
		if (prevPickPos != pickPos)
		{
			pick.anchoredPosition += new Vector2(moveX * (pickPos - prevPickPos), 0);
			if (!heldHead[prevPickPos - 1])
			{
				// the thing drops as well...could create a coroutine that lerps them down...
				pinHeads[prevPickPos - 1].anchoredPosition = originalPHP[prevPickPos - 1];
			}
			else
			{
				heldHeadPos[prevPickPos - 1] = pinHeads[prevPickPos - 1].anchoredPosition.y - originalPHP[prevPickPos - 1].y;
			}
			pins[prevPickPos - 1].anchoredPosition = originalPP[prevPickPos - 1];
			prevPickPos = pickPos;
			prevHeight = 0;
		}
		if (!heldHead[pickPos - 1] || yResting + height * (0.3f - yResting) / 100.0f >= heldHeadPos[pickPos - 1])
		{
			pinHeads[pickPos - 1].anchoredPosition = new Vector2(0, yResting + height * (0.3f - yResting) / 100.0f) + originalPHP[pickPos - 1];
		}
		pins[pickPos - 1].anchoredPosition = new Vector2(0, yResting + height * (0.3f - yResting) / 100.0f) + originalPP[pickPos - 1];

		pusher.sizeDelta = pusherSizeDelta + new Vector2(0, height * (HeightMP) / 100.0f);
		pusher.anchoredPosition = pusherPosition + new Vector2(0, height * (HeightMP) / 200.0f);
	}
}
