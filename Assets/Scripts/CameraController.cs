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
    public float yaw = 0.0f;
    public float pitch = 0.0f;

    // Set if something is in the middle of the screen
    Interactable obj;
    public string objName;
    private bool interact = false;

    private void CheckForObject()
    {
        // Check for an object in the middle if the screen
        Ray ray = Camera.main.ScreenPointToRay(
            new Vector3(
                Screen.width / 2f,
                Screen.height / 2f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 20f))
        {
            obj = hit.collider.gameObject.GetComponent<Interactable>();
            if(obj != null)
            {
                objName = obj.gameObject.name;
                obj.LookingAt();
                if (interact)
                    obj.Interact();
            }
        }

        interact = false;
    }

    void Update () 
    {
        //Adjust camera with mouse
        yaw += hSpeed * Input.GetAxis("Mouse X");
        pitch -= vSpeed * Input.GetAxis("Mouse Y");
        if (pitch >= 90)
            pitch = 89.9f;
        if (pitch <= -90)
            pitch = -89.9f;

        if (Input.GetKey(KeyCode.E))
            interact = true;
    }

    void LateUpdate ()
    {
        //Apply camera adjustments
        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        //Rotate player with camera
        player.transform.rotation = Quaternion.Euler(player.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, player.transform.rotation.eulerAngles.z);


        CheckForObject();
    }

}
