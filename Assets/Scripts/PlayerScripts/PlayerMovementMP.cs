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
    private float verticalInput;
    private float horizontalInput;
    public Text nameTemplate;
    public Transform childCameraPosition;
    void Start()
    {
        //Prevent the player from falling over when moving
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        Debug.Log(isLocalPlayer + " " + this.localPlayerAuthority);
        if (!hasAuthority) { return; }


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
    void Update()
    {
        
        if (!hasAuthority) { return; }
        //Get movement inputs
        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        //Update movement
        if (!hasAuthority) { return; }
        Move();
    }

    void Move()
    {
        if (!hasAuthority) { return; }
        //Get movement values from speed and input
        if (cam.isInCutscene) { return; }
        Vector3 verticalMovement = transform.forward * verticalInput * moveSpeed;
        Vector3 HorizontalMovement = transform.right * horizontalInput * moveSpeed;
        rb.MovePosition(rb.position + verticalMovement + HorizontalMovement);
    }

    //prevent player from rotating violently on collisions
    void OnCollisionEnter ()
    {
        if (!hasAuthority) { return; }
        rb.angularVelocity= new Vector3(0,0,0);
    }

    void OnCollisionStay ()
    {
        if (!hasAuthority) { return; }
        rb.angularVelocity = new Vector3(0,0,0);
    }
}
#pragma warning restore CS0618 // Type or member is obsolete
