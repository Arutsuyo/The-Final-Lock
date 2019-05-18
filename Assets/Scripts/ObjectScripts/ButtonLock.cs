using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonLock : MonoBehaviour
{
	public Interactable button;

	public void Start()
	{
		button.interactEvent += interacted;
		button.gameInteractComplete += finished;
	}

	public void finished()
	{
		if (RoomManager.instance.CMMP.nm.net.isHost)
		{
			RoomManager.instance.roomTimer.CmdStopTimer();
		}
	}

	public bool interacted(CameraController cc)
	{
		button.SendSF();
		return false;
	}
}
