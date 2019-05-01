using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManagement : MonoBehaviour
{
	//Placeholder for interacting with pickups and activators
	public bool activated = false; //Player can activate items but they still need to be a clue for something to happen
	public bool clue = false;

	private Vector3 original;
	private float angle;

	void Start ()
	{
		//Track original angle and position
		original = transform.localPosition;
		angle = transform.eulerAngles.x;
	}

	void Update ()
	{
		if (activated && clue)
		{
			if (gameObject.tag == "Book")
			{
				//Pull book out and angle downward
				transform.localPosition = original + transform.forward * 0.25f;
				transform.eulerAngles = new Vector3(angle + 45.0f, transform.eulerAngles.y, transform.eulerAngles.z);
			}
		}
		else
		{
			if (gameObject.tag == "Book")
			{
				//Restore book to original position
				transform.localPosition = original;
				transform.eulerAngles = new Vector3(angle, transform.eulerAngles.y, transform.eulerAngles.z);
			}
		}
	}
}

