using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonLock1 : MonoBehaviour
{
    public Interactable button;

    public GameObject nextButton;

    public void Start()
    {
        button.interactEvent += interacted;
        button.gameInteractComplete += finished;
    }

    public void finished()
    {
        if (RoomManager.instance.CMMP.nm.net.isHost)
        {
            nextButton.SetActive(true);
        }
    }

    public bool interacted(CameraController cc)
    {
        button.SendSF();
        return false;
    }
}
