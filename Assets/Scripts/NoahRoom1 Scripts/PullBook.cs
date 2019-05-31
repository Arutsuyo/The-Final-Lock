using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullBook : MonoBehaviour
{
    public Interactable ia;

    public GameObject crate1;
    public GameObject crate2;
    public GameObject crate3;

    // Start is called before the first frame update
    void Start()
    {
        Subscribe();
    }

    public void Subscribe()
    {
        if (ia)
        {
            // Subscribe to the event
            ia.lookEvent += LookedAt;
            ia.interactEvent += Interacted;
        }
    }
    public void Unsubscribe()
    {
        // Unsubscribe to the event
        ia.lookEvent -= LookedAt;
        ia.interactEvent -= Interacted;
    }

    private void LookedAt(CameraController cc)
    {
        //
    }

    private bool Interacted(CameraController cc)
    {
        //move book
        gameObject.transform.SetPositionAndRotation(new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z - 0.25f), 
            Quaternion.Euler(45, gameObject.transform.rotation.y, gameObject.transform.rotation.z));
        //Spawn crates
        crate1.SetActive(true);
        crate2.SetActive(true);
        crate3.SetActive(true);
        return false;
    }
}
