using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableDisableOverTime : MonoBehaviour
{
	public GameObject[] toChangeState;
	public bool disableStates = false;
	public float timeToGoFor = 0.5f;
	private bool forward = true;


	public bool Debug = false;
	public bool DDebug = false;
	public void StartChange()
	{
		forward = true;
		StartCoroutine(RunCS());
	}
	public void StartBackwards()
	{
		forward = false;
		StartCoroutine(RunCS());
	}

	public void Update()
	{
		if (Debug)
		{
			Debug = false;
			StartChange();
		}
		if (DDebug)
		{
			DDebug = false;
			StartBackwards();
		}
	}
	public IEnumerator RunCS()
	{
		float st = Time.time;
		if (toChangeState.Length < 2)
		{
			yield break;
		}
		int pi = -1;
		int ti = 0;
		while (Time.time - st < timeToGoFor)
		{
			ti = Mathf.FloorToInt((Time.time - st) * (toChangeState.Length - 1) / timeToGoFor);
			if (pi != ti)
			{
				for (int i = 0; i < (ti - pi); i++)
				{
					toChangeState[(forward ? pi + 1 + i : toChangeState.Length - (pi + 2 + i))].SetActive(forward ^ disableStates);
				}
				pi = ti;
			}
			yield return null;
		}
		ti = toChangeState.Length - 1;
		if (pi != ti)
		{
			for (int i = 0; i < (ti - pi); i++)
			{
				toChangeState[(forward ? pi + 1 + i : toChangeState.Length - (pi + 2 + i))].SetActive(forward ^ disableStates);
			}
			pi = ti;
		}
		// :D
	}
}
