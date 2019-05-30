using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#pragma warning disable CS0618 // Type or member is obsolete
public class CombinationLock : PuzzleObj
{
	//"Winning" combination

	public SyncListInt combo = new SyncListInt();
	//Current lock numbers
	public int[] curState;
	//Tracks which lock the player is turning
	public int current = 0;

	private Quaternion[] targetStates;

	//Lock GameObjects
	public GameObject hintText;
	public GameObject[] locks;
	public GlowObject[] glow;
	public GameObject door;
	private Quaternion doorTarget;

	//Camera position variables
	public Transform lockPosition;
	private Vector3 prevPosition;
	private Quaternion prevRotation;
	public CameraController curPlayer;
	public Interactable ia;

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

	public delegate void OnLockReady();
	public event OnLockReady PuzzleReady = delegate { };

	// Made this Awake so the poster clue can initialize during start
	public override void OnStartServer()
	{
		base.OnStartServer();

		// Set starting Values
		isOpen = false;
		inGame = false;
		current = 0;
		curState = new int[3];
		targetStates = new Quaternion[3];

		if (randomize)
		{
			for (int i = 0; i < 3; i++)
			{
				// Set combo target
				combo.Add(UnityEngine.Random.Range(0, 10));
				targetStates[i] = Quaternion.Euler(-36.0f * combo[i] + 36.0f, 0.0f, -90.0f);
				Debug.Log(combo[i] + " " + targetStates[i].ToString() + " " + locks[i].transform.localRotation.ToString() + " " + curState[i]);

				// Set current rotation
				do
					curState[i] = UnityEngine.Random.Range(0, 10);
				while (curState[i] == combo[i]);
				locks[i].transform.localRotation = Quaternion.Euler(-36.0f * curState[i] + 36.0f, 0.0f, -90.0f);
			}
		}
		else
		{
			// Set DEBUG settings
			curState[0] = 2;
			curState[1] =
				curState[2] = 1;
			combo.Add(1);
			combo.Add(1);
			combo.Add(1);
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

		PuzzleReady();
	}

	void Awake()
	{
		curState = new int[3];
		targetStates = new Quaternion[3];
		glow = new GlowObject[] {
			locks[0].GetComponent<GlowObject>(),
			locks[1].GetComponent<GlowObject>(),
			locks[2].GetComponent<GlowObject>()};
		doorTarget = Quaternion.Euler(0.0f, -90.0f, 90.0f);
		current = 0;
		Subscribe();
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		combo.Callback += CC;
		if (combo.Count == 3)
		{
			Debug.Log("Getting addition 31.");
			for (int i = 0; i < 3; i++)
				targetStates[i] = Quaternion.Euler(-36.0f * combo[i] + 36.0f, 0.0f, -90.0f);

			StartCoroutine(SCC());
		}
	}

	IEnumerator SCC()
	{
		while (RoomManager.instance == null || RoomManager.instance.CMMP == null)
			yield return null;

		if (!RoomManager.instance.CMMP.nm.net.isHost)
			PuzzleReady();
	}

	void CC(SyncListInt.Operation op, int itemIndex)
	{
		Debug.Log("Getting addition.");
		if (combo.Count == 3)
		{
			Debug.Log("Getting addition 3." + itemIndex);
			for (int i = 0; i < 3; i++)
				targetStates[i] = Quaternion.Euler(-36.0f * combo[i] + 36.0f, 0.0f, -90.0f);

			if (!RoomManager.instance.CMMP.nm.net.isHost)
				StartCoroutine(SCC());
		}
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
		isOpen = true;
		Debug.Log("Open the DOOR!");
		StartCoroutine(OpenDoor());
	}

	IEnumerator OpenDoor()
	{
		while (isOpen &&
			Mathf.Abs(
				Quaternion.Angle(door.transform.localRotation, 
				doorTarget)
				) > 1f)
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

			angle = Mathf.Abs(Quaternion.Angle(locks[i].transform.localRotation, targetStates[i]));
			if (angle > 1f)
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
			curPlayer.cam.transform.rotation = Quaternion.Slerp(prevRotation, lockPosition.rotation * Quaternion.Euler(0.0f, -90.0f, 0.0f), (CurLerpTime - StartLerpTime) / cameraLerpTime);
			yield return null;
			CurLerpTime = Time.time;
		}
		curPlayer.cam.transform.position = lockPosition.position;
		curPlayer.cam.transform.rotation = lockPosition.rotation * Quaternion.Euler(0.0f, -90.0f, 0.0f);
		cutsceneFinished = true;
		glow[current].EnableGlow();
		hintText.SetActive(true);
	}

	// Return camera to player view
	IEnumerator PlayZoomInBackward()
	{
		hintText.SetActive(false);
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

    public override void GenerateAsProp(long propSeed)
    {
        // The combination lock can not be a prop!
        throw new NotImplementedException();
    }

    public override void GenerateAsPuzzle(long puzzleSeed)
    {
        // Change to actually generate itself :|
        throw new NotImplementedException();
    }

    public override void RpcGeneratePropWithState(string state)
    {
        throw new NotImplementedException();
    }

    public override void GenerateAsProp(PuzzleObj po, PuzzleType pt)
    {
        throw new NotImplementedException();
    }
}

#pragma warning restore CS0618 // Type or member is obsolete