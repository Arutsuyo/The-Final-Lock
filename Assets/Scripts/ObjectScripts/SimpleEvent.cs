using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEvent : MonoBehaviour
{
	public GameLock locks;
	public GameObject[] toEnable;
	public GameObject[] toDisable;
    public bool toggle = false;
    
	// Start is called before the first frame update
	void Start()
	{
		locks.GameFinished += Locks_GameFinished;
        locks.GameStateSet += Locks_Set;
        locks.GameStateToggle += Locks_Toggle;
    }

	private void Locks_GameFinished(CameraController cc)
	{
		// Dispatch a game finished... o-o
		foreach (GameObject go in toDisable)
			go.SetActive(false);

		foreach (GameObject go in toEnable)
			go.SetActive(true);
	}
    private void Locks_Set(CameraController cc, bool state)
    {
        if (toggle == state)
        {
            return;
        }
        foreach (GameObject go in toDisable)
            go.SetActive(!state);

        foreach (GameObject go in toEnable)
            go.SetActive(state);
    }
    private void Locks_Toggle(CameraController cc)
    {
        toggle = !toggle;
        foreach (GameObject go in toDisable)
            go.SetActive(!toggle);

        foreach (GameObject go in toEnable)
            go.SetActive(toggle);
    }
}
