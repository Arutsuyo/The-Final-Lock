using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManagement : MonoBehaviour
{
	//Placeholder for interacting with pickups and activators
	public bool activated = false;

	private float originalZ;

	void start ()
	{
		originalZ = transform.position.z;
	}

	void Update ()
	{
		if (activated)
		{
			if (gameObject.tag == "Book")
			{
				transform.position = new Vector3(transform.position.x, transform.position.y, -0.7f);
				transform.eulerAngles = new Vector3(45.0f, transform.eulerAngles.y, transform.eulerAngles.z);
			}
		}
		else
		{
			if (gameObject.tag == "Book")
			{
				transform.position = new Vector3(transform.position.x, transform.position.y, -0.6f);
				transform.eulerAngles = new Vector3(0.0f, transform.eulerAngles.y, transform.eulerAngles.z);
			}
		}
	}
}

