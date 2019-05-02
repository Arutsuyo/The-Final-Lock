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

    private float verticalInput;
    private float horizontalInput;

    void Start()
    {
    	//Prevent the player from falling over when moving
		rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
    	//Get movement inputs
		verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
    	//Update movement
    	Move();
    }

    void Move()
    {
    	//Get movement values from speed and input
    	Vector3 verticalMovement = transform.forward * verticalInput * moveSpeed;
    	Vector3 HorizontalMovement = transform.right * horizontalInput * moveSpeed;
        rb.MovePosition(rb.position + verticalMovement + HorizontalMovement);
    }

	//prevent player from rotating violently on collisions
    void OnCollisionEnter ()
    {
    	rb.angularVelocity= new Vector3(0,0,0);
    }

    void OnCollisionStay ()
    {
    	rb.angularVelocity = new Vector3(0,0,0);
    }
}
