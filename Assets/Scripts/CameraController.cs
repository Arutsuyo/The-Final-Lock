using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;

    void LateUpdate()
    {
    	//Camera is bound to player position
        transform.rotation = player.transform.rotation;
    }
}
