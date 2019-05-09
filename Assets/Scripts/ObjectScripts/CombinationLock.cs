using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinationLock : MonoBehaviour
{

	//"Winning" combination
	public int[] combo = new int[3];
	//Current lock numbers
	public int left;
	public int middle;
	public int right;
	//Lock GameObjects
	public GameObject leftLock;
	public GameObject midLock;
	public GameObject rightLock;


    // Start is called before the first frame update
    void Start()
    {
        Subscribe();
    }

    // Update is called once per frame
    void Update()
    {
        leftLock.transform.localRotation = Quaternion.Euler(-36.0f*left + 36.0f, 0.0f, -90.0f);
        midLock.transform.localRotation = Quaternion.Euler(-36.0f*middle + 36.0f, 0.0f, -90.0f);
        rightLock.transform.localRotation = Quaternion.Euler(-36.0f*right + 36.0f, 0.0f, -90.0f);
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
