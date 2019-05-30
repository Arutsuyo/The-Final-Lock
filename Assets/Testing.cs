using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody)
        {
            Debug.Log("Trigger me daddy! " + other.attachedRigidbody);
        }
    }
}
