using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonLock : MonoBehaviour
{
	public Interactable button;
    public GameLock gameLock;

	public void Start()
	{
		button.interactEvent += interacted;
		button.gameInteractComplete += finished;
	}

	public void finished()
	{
        gameLock.GFinished(RoomManager.instance.Player.cam);
    }

	public bool interacted(CameraController cc)
	{
		button.SendSF();
		return false;
	}
}
