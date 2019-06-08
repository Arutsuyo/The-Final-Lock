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
        go.transform.SetParent(childCameraPosition);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
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
        float ff = rb.velocity.y;
        rb.velocity = rb.rotation * movement * moveSpeed + new Vector3(0, ff, 0);
    }

	//prevent player from rotating violently on collisions
	void OnCollisionEnter(Collision c)
	{
        ContactPoint[] cp = new ContactPoint[c.contactCount];
        c.GetContacts(cp);
        foreach(ContactPoint c1 in cp)
        {
            if(c1.point.y <= playerFeet.position.y && c1.point.y >= playerBase.position.y)
            {
                rb.position += new Vector3(0, c1.point.y - playerBase.position.y, 0);
            }
        }
        rb.angularVelocity = new Vector3(0, 0, 0);
	}

	void OnCollisionStay()
	{
		rb.angularVelocity = new Vector3(0, 0, 0);
	}
}
