using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Freeze Rotation source: https://gamedev.stackexchange.com/questions/99094/how-to-move-a-cube-withour-rolling-it

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;
    public float turnSpeed;
    public Rigidbody rb;

    private float moveInput;
    private float turnInput;

    void Start()
    {
		rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
		moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
    	Turn();
    	Move();
    }

    void Turn()
    {
    	float turn = turnSpeed * turnInput;
    	Quaternion turnRot = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRot);
    }

    void Move()
    {
    	Vector3 movement = transform.forward * moveInput * moveSpeed;
        rb.MovePosition(rb.position + movement);
    }
}
