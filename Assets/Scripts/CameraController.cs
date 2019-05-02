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

    // Set during update to see if the player wants to interact.
    private bool interact = false;

    // This function triggers the subscribed events on the target object in the 
    // center of the screen. Call this function at the end of LateUpdate so we 
    // can calculate the ray AFTER camera movement.
    private void CheckForObject()
    {
        // Check for an object in the middle if the screen
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(
            new Vector3(
                Screen.width / 2f,
                Screen.height / 2f, 0));
        
        // Test if there is something in the middle
        if (Physics.Raycast(ray, out hit, 20f))
        {
            // Check if the target is interactable
            Interactable obj = hit.collider.gameObject.GetComponent<Interactable>();
            if(obj != null)
            {
                // Trigger the corresponding events
                obj.LookingAt();
                if (interact)
                    obj.Interact();
            }
        }

        // Reset intractability
        interact = false;
    }

    void Update () 
    {
        //Adjust camera with mouse
        yaw += hSpeed * Input.GetAxis("Mouse X");
        pitch -= vSpeed * Input.GetAxis("Mouse Y");

        // Correct for Gimbo Lock (Player Camera should not rotate past 90'
        if (pitch >= 90)
            pitch = 89.9f;
        if (pitch <= -90)
            pitch = -89.9f;

        // This can be whatever "Use" input we want to accept
        if (Input.GetKey(KeyCode.E))
            interact = true;
    }

    void LateUpdate ()
    {
        //Apply camera adjustments
        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        //Rotate player with camera
        player.transform.rotation = Quaternion.Euler(player.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, player.transform.rotation.eulerAngles.z);

        // Interact with whatever we might be looking at after movement
        CheckForObject();
    }

}
