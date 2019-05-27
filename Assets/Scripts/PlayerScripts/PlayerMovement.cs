using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Freeze Rotation on movement: https://gamedev.stackexchange.com/questions/99094/how-to-move-a-cube-withour-rolling-it
//Freeze rotation on collision source: https://answers.unity.com/questions/768581/stop-rotation-on-collision.html

public class PlayerMovement : MonoBehaviour
{
	public float moveSpeed;
	public float turnSpeed;
	public Rigidbody rb;
	public CameraController cam;
	private Vector3 movement = new Vector3();

	void Start()
	{
		//Prevent the player from falling over when moving
		rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
	}

	void FixedUpdate()
	{
		GetInput();
		Move();
	}

	void GetInput()
	{
		//Get movement inputs
		movement.z = Input.GetAxisRaw("Vertical");
		movement.x = Input.GetAxisRaw("Horizontal");
	}

	void Move()
	{
		if (cam.isInCutscene)
			return;
        movement.y = 0;
		movement.Normalize();
        //movement.y = rb.velocity.y;
		rb.velocity = rb.transform.forward*movement.z * moveSpeed + rb.transform.right*movement.x * moveSpeed + new Vector3(0,rb.velocity.y, 0);
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
