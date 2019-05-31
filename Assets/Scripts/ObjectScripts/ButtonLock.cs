using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonLock : MonoBehaviour
{
	public Interactable button;
    public GameLock gameLock;
    public bool finishedObj = false;

	public void Start()
	{
		button.interactEvent += interacted;
		button.gameInteractComplete += finished;
	}

	public void finished()
	{
        finishedObj = true;
        gameLock.GFinished(RoomManager.instance.Player.cam);

    }

	public bool interacted(CameraController cc)
	{
        if (finishedObj)
            return false;
        finishedObj = true;
        button.SendSF();

		return false;
	}
}
