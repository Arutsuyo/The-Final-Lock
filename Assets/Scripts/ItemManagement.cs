using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManagement : MonoBehaviour
{
	//Placeholder for interacting with pickups and activators
	public bool activated = false; //Player can activate items but they still need to be a clue for something to happen
	public bool clue = false;

	private Vector3 original;
	private Vector3 angle;

	void Start ()
	{
		//Track original angle and position
		original = transform.localPosition;
		angle = transform.eulerAngles;
	}

	void Update ()
	{
		if (activated && clue)
		{
			if (gameObject.tag == "Book")
			{
				//Pull book out and angle downward
				transform.localPosition = original + transform.forward * 0.25f;
				transform.eulerAngles = new Vector3(angle.x + 45.0f, angle.y, angle.z);
			}
			else if (gameObject.tag == "Wall Item X")
			{
				//Rotate object slightly
				transform.eulerAngles = new Vector3(angle.x + 30.0f, angle.y, angle.z);
			}
			else if (gameObject.tag == "Wall Item Z")
			{
				//Rotate object slightly
				transform.eulerAngles = new Vector3(angle.x, angle.y, angle.z + 30.0f);
			}
		}
		else
		{
			//Restore object to original position
			transform.localPosition = original;
			transform.eulerAngles = new Vector3(angle.x, angle.y, angle.z);
		}
	}
}

