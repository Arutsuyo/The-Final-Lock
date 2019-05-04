using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Source for camera movement: https://gamedev.stackexchange.com/questions/104693/how-to-use-input-getaxismouse-x-y-to-rotate-the-camera
//Source for only rotating y-axis: https://answers.unity.com/questions/1111675/copy-only-y-axis-rotation-of-an-object.html

public class CameraController : MonoBehaviour
{
    [Header("Player Related")]
    public GameObject player;
    public PlayerManager playerMngr;

    [Header("Camera")]
    public Camera cam;
    //Camera movement speeds
    public float hSpeed = 1.0f;
    public float vSpeed = 1.0f;
    public bool isInCutscene = false;

    //Variables that track camera position
    public float yaw = 0.0f;
    public float pitch = 0.0f;

    [Header("Interactive Variables")]
    public float interactDistance = 20f;
    public Image hitMarker;
    // Set during update to see if the player wants to interact.
    private bool interact = false;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        WavMusicConvert.register(WavMusicConvert.ConvertFromBytes(WavMusicConvert.ReadBytes("./Assets/Music/Censored.wav"), "Censored.wav"), "Censored.wav");

    }
    private Interactable DetermineifHit(Transform ob)
    {
        Interactable i = ob.GetComponent<Interactable>();
        if(i != null)
        {
            return i;
        }
        else if(ob.parent != null)
        {
            // Travel up the parent tree...
            return DetermineifHit(ob.parent);
        }
        return null;
    }

    public void AllowCursorFreedom()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void AllowCursorPrison()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
    public void BanCursorFreedom()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

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
        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // Check if the target is interactable
            Interactable obj = DetermineifHit(hit.collider.gameObject.transform);
            if (obj != null)
            {
                hitMarker.gameObject.SetActive(true);
                // Trigger the corresponding events
                obj.LookingAt(this);
                if (interact)
                {
                    obj.Interact(this);
                    interact = false;
                }
            }
            else
                hitMarker.gameObject.SetActive(false);
        }

        // Reset intractability
        interact = false;
    }

    void Update () 
    {
        //Adjust camera with mouse
        if (!isInCutscene)
        {
            yaw += hSpeed * Input.GetAxis("Mouse X");
            pitch -= vSpeed * Input.GetAxis("Mouse Y");
            
            // Correct for Gimbo Lock (Player Camera should not rotate past 90')
            if (pitch >= 90)
                pitch = 89.9f;
            if (pitch <= -90)
                pitch = -89.9f;

            // This can be whatever "Use" input we want to accept
            if (Input.GetKeyDown(KeyCode.E))
                interact = true;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                //Cursor.lockState = Cursor.lockState == CursorLockMode.None ? CursorLockMode.Confined : CursorLockMode.None;
                Debug.Log("Lock me?");
            }
        }
    }

    void LateUpdate ()
    {
        //Apply camera adjustments
        if (!isInCutscene)
        {
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
            //Rotate player with camera
            player.transform.rotation = Quaternion.Euler(player.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, player.transform.rotation.eulerAngles.z);

            // Interact with whatever we might be looking at after movement
            CheckForObject();
        }
    }

}
