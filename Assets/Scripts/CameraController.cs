using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Source for camera movement: https://gamedev.stackexchange.com/questions/104693/how-to-use-input-getaxismouse-x-y-to-rotate-the-camera
//Source for only rotating y-axis: https://answers.unity.com/questions/1111675/copy-only-y-axis-rotation-of-an-object.html

public class CameraController : MonoBehaviour
{
    public GameObject player;

	//Camera movement speeds
    public float hSpeed = 1.0f;
    public float vSpeed = 1.0f;

    //Variables that track camera position
	private float yaw = 0.0f;
    private float pitch = 0.0f;

    void Update () 
    {
    	//Adjust camera with mouse
        yaw += hSpeed * Input.GetAxis("Mouse X");
        pitch -= vSpeed * Input.GetAxis("Mouse Y");
    }

    void LateUpdate ()
    {
    	//Apply camera adjustments
		transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
		//Rotate player with camera
        player.transform.rotation = Quaternion.Euler(player.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, player.transform.rotation.eulerAngles.z);
    }

}
