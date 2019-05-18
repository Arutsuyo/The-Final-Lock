using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

//Freeze Rotation on movement: https://gamedev.stackexchange.com/questions/99094/how-to-move-a-cube-withour-rolling-it
//Freeze rotation on collision source: https://answers.unity.com/questions/768581/stop-rotation-on-collision.html

#pragma warning disable CS0618 // Type or member is obsolete
public class PlayerMovementMP : NetworkBehaviour
{
	public float moveSpeed;
	public float turnSpeed;
	public Rigidbody rb;
	public CameraController cam;
	private Vector3 movement = new Vector3();
	public Text nameTemplate;
	public Transform childCameraPosition;

	void Start()
	{
		//Prevent the player from falling over when moving
		rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		Debug.Log(isLocalPlayer + " " + this.localPlayerAuthority);
		if (!hasAuthority)
			return;

		// Get the camera and place it right where it should be :D
		GameObject go = GameObject.FindGameObjectWithTag("MainCamera");
		cam = go.GetComponent<CameraController>();

		go.transform.SetParent(childCameraPosition);
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.identity;
		cam.player = rb.gameObject;
		cam.enabled = true;
		nameTemplate.enabled = false;
	}

	[ClientRpc]
	public void RpcChangeName(string name)
	{
		nameTemplate.text = name;
	}
	public override void OnStartAuthority()
	{
		Debug.Log("AUTHORITY GRANTED!");
		base.OnStartAuthority();
		this.Start();
	}

	void FixedUpdate()
	{
		//Update movement
		if (!hasAuthority)
			return;

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
		if (!hasAuthority || cam.isInCutscene)
			return;

		movement.Normalize();
		float ff = rb.velocity.y;
		rb.velocity = rb.rotation*movement * moveSpeed + new Vector3(0,ff,0);
		
	}

	//prevent player from rotating violently on collisions
	void OnCollisionEnter()
	{
		if (!hasAuthority)
			return;

		rb.angularVelocity = new Vector3(0, 0, 0);
	}

	void OnCollisionStay()
	{
		if (!hasAuthority)
			return;

		rb.angularVelocity = new Vector3(0, 0, 0);
	}
}
#pragma warning restore CS0618 // Type or member is obsolete
