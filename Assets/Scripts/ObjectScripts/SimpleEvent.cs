﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEvent : MonoBehaviour
{
	public GameLock locks;
	public GameObject[] toEnable;
	public GameObject[] toDisable;
	// Start is called before the first frame update
	void Start()
	{
		locks.GameFinished += Locks_GameFinished;
	}

	private void Locks_GameFinished(CameraController cc)
	{
		// Dispatch a game finished... o-o
		foreach (GameObject go in toDisable)
			go.SetActive(false);

		foreach (GameObject go in toEnable)
			go.SetActive(true);
	}

}
