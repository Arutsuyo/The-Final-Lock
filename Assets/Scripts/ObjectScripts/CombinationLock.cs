﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinationLock : MonoBehaviour
{
	//"Winning" combination
	public int[] combo;
	//Current lock numbers
	public int[] curState;
	//Tracks which lock the player is turning
	public int current = 0;
	private Quaternion[] targetStates;

	//Lock GameObjects
	public GameObject[] locks;
	public GlowObject[] glow;
	public GameObject door;
	private Quaternion doorTarget;

	//Camera position variables
	public Transform lockPosition;
	private Vector3 prevPosition;
	private Quaternion prevRotation;
	public CameraController curPlayer;
	private Interactable ia;

	// Lerp info
	public float spinnerLerpFactor = 5f;
	public float DoorLerpFactor = 1f;
	public float cameraLerpTime = 3f;
	public bool cutsceneFinished = false;

	//Tracks whether the player is playing the game
	private static bool inGame = false;

	//Tracks whether the door is opened
	private bool isOpen = false;

	// Randomize values on Start()?
	public bool randomize;

	// Made this Awake so the poster clue can initialize during start
	void Awake()
	{
		isOpen = false;
		inGame = false;
		current = 0;
		combo = new int[3];
		curState = new int[3];
		targetStates = new Quaternion[3];

		glow = new GlowObject[] {
			locks[0].GetComponent<GlowObject>(),
			locks[1].GetComponent<GlowObject>(),
			locks[2].GetComponent<GlowObject>()};

		if (randomize)
		{
			for (int i = 0; i < 3; i++)
			{
				// Set current rotation
				curState[i] = UnityEngine.Random.Range(0, 10);
				locks[i].transform.localRotation = Quaternion.Euler(-36.0f * curState[i] + 36.0f, 0.0f, -90.0f);

				// Set combo target
				combo[i] = UnityEngine.Random.Range(0, 10);
				targetStates[i] = Quaternion.Euler(-36.0f * combo[i] + 36.0f, 0.0f, -90.0f);
                Debug.Log(combo[i] + " " + targetStates[i].ToString() + " " + locks[i].transform.localRotation.ToString() + " " +curState[i]);
			}
		}
		else
		{
			// Set DEBUG settings
			curState[0] = 2;
			curState[1] =
				curState[2] =
				combo[0] =
				combo[1] =
				combo[2] = 1;
			locks[0].transform.localRotation =
				Quaternion.Euler(-36.0f * curState[0] + 36.0f, 0.0f, -90.0f);
			locks[1].transform.localRotation =
				locks[2].transform.localRotation =
				Quaternion.Euler(-36.0f * curState[1] + 36.0f, 0.0f, -90.0f);
			targetStates[0] =
				targetStates[1] =
				targetStates[2] =
				Quaternion.Euler(-36.0f * combo[0] + 36.0f, 0.0f, -90.0f);
		}

		doorTarget = Quaternion.Euler(0.0f, -90.0f, 90.0f);

		Subscribe();
	}

	void Ejection()
	{
		if (curPlayer != null)
		{
			inGame = false;
			Debug.Log("Stopping unlock attempt");
			cutsceneFinished = false;
			StartCoroutine("PlayZoomInBackward");
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (curPlayer != null && cutsceneFinished)
		{
			if (isOpen || Input.GetKeyDown(KeyCode.Escape))
			{
				inGame = false;
				Debug.Log("Stopping unlock attempt");
				cutsceneFinished = false;
				StartCoroutine("PlayZoomInBackward");
				if (!isOpen)
					ia.SendAbort();

				else
					ia.SendSF();
			}
		}

		if (isOpen == false && inGame)
		{
			// Get inputs
			if (Input.GetKeyDown(KeyCode.W))
			{
				if (curState[current] == 9)
					curState[current] = 0;
				else
					curState[current]++;
			}

			if (Input.GetKeyDown(KeyCode.S))
			{
				if (curState[current] == 0)
					curState[current] = 9;
				else
					curState[current]--;
			}

			if (Input.GetKeyDown(KeyCode.D))
			{
				glow[current].DisableGlow();
				current++;
				if (current > 2)
					current = 0;
				glow[current].EnableGlow();
			}

			if (Input.GetKeyDown(KeyCode.A))
			{
				glow[current].DisableGlow();
				current--;
				if (current < 0)
					current = 2;
				glow[current].EnableGlow();
			}

			if (RotateLock())
				isOpen = true;
		}
	}

	private void OpenTheDoor()
	{
		StartCoroutine(OpenDoor());
	}

	IEnumerator OpenDoor()
	{
		while (isOpen &&
			Quaternion.Angle(door.transform.localRotation, doorTarget) > 1f)
		{
			door.transform.localRotation = Quaternion.Slerp(
				door.transform.localRotation,
				doorTarget,
				Time.deltaTime * DoorLerpFactor);
			yield return null;
		}
	}

	// Move lock position, return true if the lock is open
	public bool RotateLock()
	{
		bool unlocked = true;
		for (int i = 0; i < 3; i++)
		{
			Quaternion target =
				Quaternion.Euler(-36.0f * curState[i] + 36.0f, 0.0f, -90.0f);
			float angle = Quaternion.Angle(locks[i].transform.localRotation, target);
			if (angle != 0)
			{
				locks[i].transform.localRotation = Quaternion.Slerp(
					locks[i].transform.localRotation,
					target,
					spinnerLerpFactor * Time.deltaTime);
			}

			angle = Quaternion.Angle(locks[i].transform.localRotation, targetStates[i]);
			if (angle < 1f)
			{
				unlocked = false;
			}
		}
		return unlocked;
	}

	// Zoom camera forward
	IEnumerator PlayZoomInForward()
	{
		curPlayer.HideMarkers();
		curPlayer.BanCursorFreedom();
		float StartLerpTime = Time.time;
		float CurLerpTime = Time.time;
		while (CurLerpTime - StartLerpTime < cameraLerpTime)
		{
			curPlayer.cam.transform.position = Vector3.Lerp(prevPosition, lockPosition.position, (CurLerpTime - StartLerpTime) / cameraLerpTime);
			//curPlayer.cam.transform.localPosition = new Vector3(curPlayer.cam.transform.localPosition.x, curPlayer.cam.transform.localPosition.y, curPlayer.cam.transform.localPosition.z - 0.5f);
			curPlayer.cam.transform.rotation = Quaternion.Slerp(prevRotation, lockPosition.rotation * Quaternion.Euler(0.0f, -90.0f, 0.0f), (CurLerpTime - StartLerpTime) / cameraLerpTime);
			yield return null;
			CurLerpTime = Time.time;
		}
		curPlayer.cam.transform.position = lockPosition.position;
		//curPlayer.cam.transform.localPosition = new Vector3(curPlayer.cam.transform.localPosition.x, curPlayer.cam.transform.localPosition.y, curPlayer.cam.transform.localPosition.z - 0.5f);
		curPlayer.cam.transform.rotation = lockPosition.rotation * Quaternion.Euler(0.0f, -90.0f, 0.0f);
		cutsceneFinished = true;
		glow[current].EnableGlow();
	}

	// Return camera to player view
	IEnumerator PlayZoomInBackward()
	{
		foreach (GlowObject go in glow)
			go.DisableGlow();

		float StartLerpTime = Time.time;
		float CurLerpTime = Time.time;
		while (CurLerpTime - StartLerpTime < cameraLerpTime)
		{
			curPlayer.cam.transform.position = Vector3.Lerp(prevPosition, lockPosition.position, 1.0f - ((CurLerpTime - StartLerpTime) / cameraLerpTime));
			curPlayer.cam.transform.rotation = Quaternion.Slerp(prevRotation, lockPosition.rotation, 1.0f - ((CurLerpTime - StartLerpTime) / cameraLerpTime));
			yield return null;
			CurLerpTime = Time.time;
		}
		curPlayer.cam.transform.position = prevPosition;
		curPlayer.cam.transform.rotation = prevRotation;
		curPlayer.isInCutscene = false;
		curPlayer.ShowMarkers();
		curPlayer.BanCursorFreedom();
		curPlayer = null;
	}

	public void Subscribe()
	{
		// Get the Interactable script reference
		ia = gameObject.GetComponent<Interactable>();

		// Make sure it's not null
		if (ia)
		{
			// Subscribe to the event
			ia.lookEvent += LookedAt;
			ia.interactEvent += Interacted;
			ia.escapeInteractEvent += Ejection;
			ia.gameInteractComplete += OpenTheDoor;
		}
	}

	public void Unsubscribe()
	{
		// Get the Interactable script reference
		ia = gameObject.GetComponent<Interactable>();

		// Make sure it's not null
		if (ia)
		{
			// Unsubscribe to the event
			ia.lookEvent -= LookedAt;
			ia.interactEvent -= Interacted;
			ia.escapeInteractEvent -= Ejection;
			ia.gameInteractComplete -= OpenTheDoor;
		}
	}

	private void LookedAt(CameraController cc)
	{

	}

	private bool Interacted(CameraController cc)
	{
		if (isOpen)
			return false;

		Debug.Log("Unlocking safe...");
		cc.isInCutscene = true;
		curPlayer = cc;
		cutsceneFinished = false;
		prevPosition = cc.cam.transform.position;
		prevRotation = cc.cam.transform.rotation;
		StartCoroutine("PlayZoomInForward");
		inGame = true;
		Debug.Log("Use W and S to scroll the locks. Use A and D to switch locks. Press ESC to exit.");
		return true;
	}
}

