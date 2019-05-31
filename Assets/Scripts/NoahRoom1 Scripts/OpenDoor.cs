using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    public ItemManagement im;

    void Update()
    {
        if (im.activated)
        {
            gameObject.SetActive(false);
        }
    }
}
