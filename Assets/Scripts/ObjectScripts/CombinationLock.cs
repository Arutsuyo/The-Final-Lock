using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinationLock : MonoBehaviour
{
	//"Winning" combination
	public int[] combo = new int[3];

	//Current lock numbers
	public float left;
	public float middle;
	public float right;

    //How much the player can move the lock
    public float increment;

	// Numbers resprestented by current lock position
	private int leftVal;
	private int midVal;
	private int rightVal;

	//Lock GameObjects
	public GameObject leftLock;
	public GameObject midLock;
	public GameObject rightLock;
	public GameObject door;

    //Camera position variables
    public Transform lockPosition;
    private Vector3 prevPosition;
    private Quaternion prevRotation;
    public CameraController curPlayer;
    public float LerpTime = 0;
    private float CurLerpTime = 0;
    private float StartLerpTime = 0;
    public bool cutsceneFinished = false;

    //Tracks whether the player is playing the game
    private bool inGame = false;

    //Tracks which lock the player is turning
    private int current = 0;

    //Tracks whether the door is opened
    private bool isOpen = false;

    // Start is called before the first frame update
    void Start()
    {
        Subscribe();
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
            }
        }

        if (isOpen == false && inGame)
        {
            if (Input.GetKey(KeyCode.W))
            {
                if (current == 0)
                {
                    left += increment;
                }
                else if (current == 1)
                {
                    middle += increment;
                }
                else if (current == 2)
                {
                    right += increment;
                }
            }

            if (Input.GetKey(KeyCode.S))
            {
                if (current == 0)
                {
                    left -= increment;
                }
                else if (current == 1)
                {
                    middle -= increment;
                }
                else if (current == 2)
                {
                    right -= increment;
                }
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                if (current > 0)
                {
                    current--;
                }
                else 
                {
                    Debug.Log("Cannot move further left!");
                }
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                if (current < 2)
                {
                    current++;
                }
                else
                {
                    Debug.Log("Cannot move further right!");
                }
            }
        }

        // Set the lock values and check for correctness
        SetVal (ref leftVal, ref left);
        SetVal (ref midVal, ref middle);
        SetVal (ref rightVal, ref right);
        RotateLock();

    	// Open door if combo is right
    	if (CheckCombo())
    	{
            isOpen = true;
    		door.transform.localRotation = Quaternion.Euler(0.0f, -90.0f, 90.0f);
    	}
    }

    // Set values based on what position the lock is closest to
    public void SetVal (ref int target, ref float val) 
    {
    	//Source for ref
    	//https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/ref
    	if (val >= 10)
    	{
    		val = val - 10;
    	}

    	if (val < 0)
    	{
    		val = 9.9f + val;
    	}

    	//Source for rounding
    	//https://docs.microsoft.com/en-us/dotnet/api/system.math.round?redirectedfrom=MSDN&view=netframework-4.8#System_Math_Round_System_Double_
    	target = (int) Math.Round(val);

    	if (target == 10)
    	{
    		target = 0;
    	}
    }

    // Move lock position
    public void RotateLock ()
    {
        leftLock.transform.localRotation = Quaternion.Euler(-36.0f*left + 36.0f, 0.0f, -90.0f);
        midLock.transform.localRotation = Quaternion.Euler(-36.0f*middle + 36.0f, 0.0f, -90.0f);
        rightLock.transform.localRotation = Quaternion.Euler(-36.0f*right + 36.0f, 0.0f, -90.0f);
    }

    // Check if the lock is correct
    public bool CheckCombo () 
    {
    	return (leftVal == combo[0] && midVal == combo[1] && rightVal == combo[2]);
    }

    // Zoom camera forward
    IEnumerator PlayZoomInForward()
    {
        StartLerpTime = Time.time;
        CurLerpTime = Time.time;
        while(CurLerpTime - StartLerpTime < LerpTime)
        {
            curPlayer.cam.transform.position = Vector3.Lerp(prevPosition, new Vector3(lockPosition.position.x, lockPosition.position.y, lockPosition.position.z), (CurLerpTime - StartLerpTime)/LerpTime);
            curPlayer.cam.transform.rotation = Quaternion.Slerp(prevRotation, lockPosition.rotation  * Quaternion.Euler(0.0f, 180.0f, 0.0f), (CurLerpTime - StartLerpTime) / LerpTime);
            yield return null;
            CurLerpTime = Time.time;
        }
        curPlayer.cam.transform.position = new Vector3(lockPosition.position.x, lockPosition.position.y, lockPosition.position.z);
        curPlayer.cam.transform.localPosition = new Vector3(curPlayer.cam.transform.localPosition.x - 0.5f, curPlayer.cam.transform.localPosition.y + 0.2f, curPlayer.cam.transform.localPosition.z - 0.5f);
        curPlayer.cam.transform.rotation = lockPosition.rotation * Quaternion.Euler(0.0f, 180.0f, 0.0f);
        curPlayer.AllowCursorFreedom();
        cutsceneFinished = true;
    } 

    // Return camera to player view
    IEnumerator PlayZoomInBackward()
    {
        StartLerpTime = Time.time;
        CurLerpTime = Time.time;
        while (CurLerpTime - StartLerpTime < LerpTime)
        {
            curPlayer.cam.transform.position = Vector3.Lerp(prevPosition, lockPosition.position, 1.0f - ((CurLerpTime - StartLerpTime) / LerpTime));
            curPlayer.cam.transform.rotation = Quaternion.Slerp(prevRotation, lockPosition.rotation, 1.0f - ((CurLerpTime - StartLerpTime) / LerpTime));
            yield return null;
            CurLerpTime = Time.time;
        }
        curPlayer.cam.transform.position = prevPosition;
        curPlayer.cam.transform.rotation = prevRotation;
        curPlayer.isInCutscene = false;
        curPlayer.BanCursorFreedom();
        curPlayer = null;
    }

    public void Subscribe()
    {
        // Get the Interactable script reference
        Interactable ia = gameObject.GetComponent<Interactable>();

        // Make sure it's not null
        if (ia)
        {
            // Subscribe to the event
            ia.lookEvent += LookedAt;
            ia.interactEvent += Interacted;
        }
    }

    public void Unsubscribe()
    {
        // Get the Interactable script reference
        Interactable ia = gameObject.GetComponent<Interactable>();

        // Make sure it's not null
        if (ia)
        {
            // Unsubscribe to the event
            ia.lookEvent -= LookedAt;
            ia.interactEvent -= Interacted;
        }
    }

    private void LookedAt(CameraController cc)
    {
        
    }

    private void Interacted(CameraController cc)
    {
        if (isOpen) { return; }
        Debug.Log("Unlocking safe...");
        cc.isInCutscene = true;
        curPlayer = cc;
        cutsceneFinished = false;
        prevPosition = cc.cam.transform.position;
        prevRotation = cc.cam.transform.rotation;
        StartCoroutine("PlayZoomInForward");
        inGame = true;
        Debug.Log("Use W and S to scroll the locks. Use A and D to switch locks. Press ESC to exit.");
    }
}

