using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Freeze Rotation on movement: https://gamedev.stackexchange.com/questions/99094/how-to-move-a-cube-withour-rolling-it
//Freeze rotation on collision source: https://answers.unity.com/questions/768581/stop-rotation-on-collision.html

public class PlayerMovement : PlayerMovementMP
{
	private Vector2 movement = new Vector2();

	void Start()
	{
		//Prevent the player from falling over when moving
		rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        GameObject go = GameObject.FindGameObjectWithTag("MainCamera");
        cam = go.GetComponent<CameraController>();
        cam.player = rb.gameObject;
        cam.enabled = true;
    }


	void FixedUpdate()
	{
        
		GetInput();
		Move();
	}

	void GetInput()
	{
		//Get movement inputs
		movement.x = Input.GetAxisRaw("Vertical");
		movement.y = Input.GetAxisRaw("Horizontal");
	}

	void Move()
	{
		if (cam.isInCutscene)
			return;

		movement.Normalize();
		rb.velocity = movement * moveSpeed;
	}

	//prevent player from rotating violently on collisions
	void OnCollisionEnter()
	{
		rb.angularVelocity = new Vector3(0, 0, 0);
	}

	void OnCollisionStay()
	{
		rb.angularVelocity = new Vector3(0, 0, 0);
	}
}
