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

	// Numbers resprestented by current lock position
	private int leftVal;
	private int midVal;
	private int rightVal;

	//Lock GameObjects
	public GameObject leftLock;
	public GameObject midLock;
	public GameObject rightLock;
	public GameObject door;

    // Start is called before the first frame update
    void Start()
    {
        Subscribe();
    }

    // Update is called once per frame
    void Update()
    {
    	// Open door if combo is right
    	if (CheckCombo())
    	{
    		door.transform.localRotation = Quaternion.Euler(0.0f, -90.0f, 90.0f);
    	}
    	SetVal (ref leftVal, ref left);
    	SetVal (ref midVal, ref middle);
    	SetVal (ref rightVal, ref right);
    	RotateLock();
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
        // Handle event. . . Set whatever you want. . .
        // Call whatever function you want. . .
    }

    private void Interacted(CameraController cc)
    {
        // Handle event. . . Set whatever you want. . .
        // Call whatever function you want. . . 
    }
}
