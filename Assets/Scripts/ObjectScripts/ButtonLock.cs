using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonLock : MonoBehaviour
{
	public Interactable button;
    public GameLock gameLock;
    public bool finishedObj = false;
    public bool repeatable = false;
    private bool state = false;

	public void Start()
	{
		button.interactEvent += interacted;
		button.gameInteractComplete += finished;
        button.updateEvent += StateUpdate;

    }

	public void finished()
	{
        finishedObj = true;
        gameLock.GFinished(RoomManager.instance.Player.cam);

    }

    public void StateUpdate(string state)
    {
        Debug.Log("State updated: now " + this.state + " (should) be " + state);
        this.state = !this.state; // completely ignore state :D
        gameLock.GToggleState(RoomManager.instance.Player.cam);
    }

	public virtual bool interacted(CameraController cc)
	{
        if (finishedObj && !repeatable)
            return false;
        if (repeatable)
        {
            button.SendUpdate(state ? "0" : "1"); // Remember, the state is TOGGLED!
        }
        else
        {
            finishedObj = true;
            button.SendSF();
        }
		return false;
	}
}
